using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace VTubeLink
{
    public class OpentrackManager : INotifyPropertyChanged
    {
        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<MappingUpdate>? MappingUpdated;

        private UdpClient? _udpClient;
        private const string IpAddress = "127.0.0.1";
        private const int Port = 4242;

        private bool _injectionLoopRunning;
        private CancellationTokenSource? _cancellationTokenSource;

        public IFacialMocapReceiver? DataSource { get; set; }

        public async Task ConnectAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            
            try
            {
                _udpClient = new UdpClient();
                _udpClient.Connect(IpAddress, Port);
                IsConnected = true;

                AppLogger.Log("OPT", $"Connected to Opentrack at {IpAddress}:{Port}. Starting injection loop.");
                _ = InjectionLoopAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Opentrack] Connection failed: {ex.Message}");
                AppLogger.Log("OPT", $"Opentrack connection failed: {ex.Message}");
                CleanupConnection();
            }
            await Task.CompletedTask;
        }

        public void Disconnect()
        {
            _injectionLoopRunning = false;
            _cancellationTokenSource?.Cancel();

            CleanupConnection();
        }

        private void CleanupConnection()
        {
            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient.Dispose();
                _udpClient = null;
            }
            IsConnected = false;
        }

        private DateTime _lastBroadcastTime = DateTime.MinValue;

        private async Task InjectionLoopAsync()
        {
            _injectionLoopRunning = true;
            var token = _cancellationTokenSource!.Token;

            while (_injectionLoopRunning && IsConnected && !token.IsCancellationRequested)
            {
                if (DataSource == null)
                {
                    await Task.Delay(16, token);
                    continue;
                }

                var capturedData = DataSource.LatestData;
                var paramsList = DataMapper.BuildParamsDict(capturedData);

                // Assuming the first 6 elements in paramsList are TX, TY, TZ, Yaw, Pitch, Roll in that order.
                var opentrackParams = paramsList.Take(6).ToList();

                if (opentrackParams.Count >= 6)
                {
                    byte[] packet = CreateOpentrackPacket(
                        opentrackParams[0].Value, // TX
                        opentrackParams[1].Value, // TY
                        opentrackParams[2].Value, // TZ
                        opentrackParams[3].Value, // Yaw
                        opentrackParams[4].Value, // Pitch
                        opentrackParams[5].Value  // Roll
                    );

                    try
                    {
                        await _udpClient!.SendAsync(packet, packet.Length);
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Log("OPT", $"Opentrack injection error: {ex.Message}");
                        break;
                    }
                }

                // Broadcast Mapping Updates
                var now = DateTime.Now;
                if ((now - _lastBroadcastTime).TotalMilliseconds >= 100)
                {
                    var update = new MappingUpdate();
                    foreach (var kvp in capturedData.Blendshapes) update.ArkitParams[kvp.Key] = kvp.Value;
                    foreach (var p in paramsList) update.MappedParams[p.Id] = p.Value;
                    MappingUpdated?.Invoke(update);
                    _lastBroadcastTime = now;
                }

                try
                {
                    await Task.Delay(16, token); // roughly 60fps
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            _injectionLoopRunning = false;
        }

        private byte[] CreateOpentrackPacket(double tx, double ty, double tz, double yaw, double pitch, double roll)
        {
            byte[] packet = new byte[48];
            Buffer.BlockCopy(BitConverter.GetBytes(tx), 0, packet, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(ty), 0, packet, 8, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(tz), 0, packet, 16, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(yaw), 0, packet, 24, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(pitch), 0, packet, 32, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(roll), 0, packet, 40, 8);
            return packet;
        }
    }
}
