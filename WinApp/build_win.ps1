param (
    [Parameter(Mandatory=$false)]
    [ValidateSet("Standalone", "Lightweight")]
    [string]$Mode = "Standalone"
)

# OpentrackLink Windows Build & Packaging Script
$AppName = "OpentrackLink"
# Use absolute path to avoid permission/context issues
$DistDir = Join-Path $PSScriptRoot "..\dist"
$IconSource = "app_icon.png"
$IconOutput = "app_icon.ico"

Write-Host "=== Building OpentrackLink (Mode: $Mode) ===" -ForegroundColor Cyan

# 1. Generate Multi-Resolution Icon
if (Test-Path $IconSource) {
    Write-Host "Generating multi-resolution icon..." -ForegroundColor Yellow
    
    $TempDir = Join-Path $PSScriptRoot "icon_gen_temp"
    if (Test-Path $TempDir) { Remove-Item -Recurse -Force $TempDir }
    New-Item -ItemType Directory -Path $TempDir | Out-Null
    
    Copy-Item "Helpers\icon_gen.cs" "$TempDir\Program.cs"
    
    $ProjContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@
    $ProjContent | Out-File "$TempDir\icon_gen.csproj" -Encoding utf8
    
    Copy-Item $IconSource "$TempDir\$IconSource"
    dotnet run --project $TempDir
    
    if (Test-Path "$TempDir\$IconOutput") {
        Copy-Item "$TempDir\$IconOutput" "." -Force
        Write-Host "Icon generated successfully." -ForegroundColor Green
    }
    
    Remove-Item -Recurse -Force $TempDir
}

# 2. Build and Publish
if (Test-Path $DistDir) { 
    Write-Host "Cleaning dist folder..." -ForegroundColor Gray
    # Try to stop potentially running instances to prevent file lock
    Get-Process $AppName -ErrorAction SilentlyContinue | Stop-Process -Force
    Start-Sleep -Seconds 1 # Wait for OS to release locks
    Remove-Item -Recurse -Force $DistDir -ErrorAction SilentlyContinue
}

$PublishArgs = @("publish", "-c", "Release", "-o", $DistDir)

if ($Mode -eq "Standalone") {
    Write-Host "Mode: Standalone (Self-Contained Single File)..." -ForegroundColor Yellow
    $PublishArgs += "-r", "win-x64"
    $PublishArgs += "--self-contained", "true"
    $PublishArgs += "-p:PublishSingleFile=true"
    $PublishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
} else {
    Write-Host "Mode: Lightweight (Framework Dependent Single File)..." -ForegroundColor Yellow
    $PublishArgs += "--self-contained", "false"
    $PublishArgs += "-p:PublishSingleFile=true"
}

# General optimizations
$PublishArgs += "-p:PublishReadyToRun=false"
$PublishArgs += "-p:DebugType=none"
$PublishArgs += "-p:DebugSymbols=false"

dotnet @PublishArgs

Write-Host ""
Write-Host "=== Build Complete ===" -ForegroundColor Green

# 3. Final Verification (Add slight delay for AV scan)
$TargetFile = Join-Path $DistDir "$AppName.exe"
Start-Sleep -Milliseconds 500

if (Test-Path $TargetFile) {
    $fileSize = (Get-Item $TargetFile).Length / 1MB
    Write-Host "Executable: $TargetFile" -ForegroundColor Cyan
    Write-Host "File Size:  $($fileSize.ToString("F2")) MB" -ForegroundColor Gray
} else {
    Write-Error "Build failed: $AppName.exe not found at $TargetFile"
}
