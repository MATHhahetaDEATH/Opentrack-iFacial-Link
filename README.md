<p align="center">
  <img src="WinApp/app_icon.png" alt="Opentrack-iFacial-Link" width="200">
</p>

<p align="center">
  <strong>Opentrack-iFacial-Link</strong><br>
  A bridge tool for iFacialMocap tracking to Opentrack.
</p>

<p align="center">
  <a href="https://dotnet.microsoft.com/en-us/download/dotnet/9.0"><img src="https://img.shields.io/badge/.NET-9.0-blueviolet?style=for-the-badge&logo=dotnet" alt=".NET 9"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-blue.svg?style=for-the-badge" alt="MIT License"></a>
  <img src="https://img.shields.io/badge/Platform-Windows-0078D4?style=for-the-badge&logo=windows" alt="Windows Support">
</p>

## Overview

A bridge tool that maps facial tracking from **iFacialMocap** (iOS) to **Opentrack** via UDP, enabling full Apple ARKit 6DOF head tracking.

## Quick Start Guide

1.  In **Opentrack**, set the **Input** to **"UDP over network"**.
2.  Click the settings (hammer icon) for the UDP input and ensure the **Port** is set to **4242** (default).
3.  Enter the IP address provided by **iFacialMocap** on your iPhone into this app.
4.  Toggle the connection switches for both iFacialMocap and Opentrack. Green indicators signify a successful connection.
5.  Use the **Mapping Monitor** to visualize real-time 6DOF data and blendshapes.

## Tips on Background Service

The app uses a **System Tray** logic to stay out of your way:

-   Closing the window will **minimize** it to the system tray (look for the link icon near your clock).
-   Double-click the tray icon to restore the window, or right-click for a quick menu.
-   To fully exit the app, use the "Exit" option in the tray's right-click menu.

## Build from Source (Development)

**Requirements**: Windows 10/11, Visual Studio 2022, .NET 9.0 SDK.

We provide a PowerShell script `build_win.ps1` in the `WinApp` folder with two main distribution modes:

1.  **Standalone (Recommended for users)**:
    Includes the .NET 9 runtime inside the `.exe`. No installation required (~170MB).
    ```powershell
    .\build_win.ps1 -Mode Standalone
    ```

2.  **Lightweight (For developers)**:
    A tiny file (~2MB), but requires [.NET 9 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) installed on the target machine.
    ```powershell
    .\build_win.ps1 -Mode Lightweight
    ```

Find your application in the `dist/` folder at the project root.

## Supported Parameters

The tool sends **6DOF data (TX, TY, TZ, Yaw, Pitch, Roll)** to Opentrack and supports visualizing all **52 Apple ARKit blendshapes** in the real-time monitor.