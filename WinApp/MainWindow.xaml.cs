using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media;

namespace VTubeLink
{
    public partial class MainWindow : Window
    {
        private IFacialMocapReceiver _receiver;
        private OpentrackManager _opentrackManager;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            _receiver = new IFacialMocapReceiver();
            _opentrackManager = new OpentrackManager { DataSource = _receiver };

            _receiver.PropertyChanged += Receiver_PropertyChanged;
            _opentrackManager.PropertyChanged += OpentrackManager_PropertyChanged;

            IpTextBox.Text = ConfigManager.Instance.IpAddress;

            SetupNotifyIcon();
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            
            try
            {
                var assembly = typeof(MainWindow).Assembly;
                // Resource name is typically Namespace.Filename
                using (var stream = assembly.GetManifestResourceStream("VTubeLink.app_icon.ico"))
                {
                    if (stream != null)
                    {
                        _notifyIcon.Icon = new Icon(stream);
                    }
                    else
                    {
                        _notifyIcon.Icon = SystemIcons.Application;
                    }
                }
            }
            catch
            {
                _notifyIcon.Icon = SystemIcons.Application;
            }
            _notifyIcon.Visible = true;
            _notifyIcon.Text = "VTubeLink Running";
            _notifyIcon.DoubleClick += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };

            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) =>
            {
                _notifyIcon.Visible = false;
                Application.Current.Shutdown();
            };
            var showItem = new System.Windows.Forms.ToolStripMenuItem("Show Panel");
            showItem.Click += (s, e) =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };
            contextMenu.Items.Add(showItem);
            contextMenu.Items.Add(exitItem);
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void Receiver_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IFacialMocapReceiver.IsReceiving))
            {
                Dispatcher.Invoke(() =>
                {
                    bool isReceiving = _receiver.IsReceiving;
                    UdpStatusIndicator.Fill = isReceiving ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
                    UdpStatusText.Text = isReceiving ? "Listening on UDP 49983" : "Disconnected";
                    ToggleUdpButton.Content = isReceiving ? "Stop" : "Start";
                });
            }
        }

        private void OpentrackManager_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(OpentrackManager.IsConnected))
            {
                Dispatcher.Invoke(() =>
                {
                    bool isConnected = _opentrackManager.IsConnected;

                    if (isConnected)
                    {
                        OpentrackStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
                        OpentrackStatusText.Text = "Connected to Opentrack";
                        ToggleOpentrackButton.Content = "Disconnect";
                    }
                    else
                    {
                        OpentrackStatusIndicator.Fill = new SolidColorBrush(Colors.Red);
                        OpentrackStatusText.Text = "Disconnected";
                        ToggleOpentrackButton.Content = "Connect to Opentrack";
                    }
                });
            }
        }

        private async void ToggleUdpButton_Click(object sender, RoutedEventArgs e)
        {
            if (_receiver.IsReceiving)
            {
                _receiver.Stop();
            }
            else
            {
                string ip = IpTextBox.Text.Trim();
                ConfigManager.Instance.IpAddress = ip;
                ConfigManager.Instance.Save();
                
                UdpStatusText.Text = "Starting...";
                await _receiver.StartAsync(ip);
            }
        }

        private async void ToggleOpentrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_opentrackManager.IsConnected)
            {
                _opentrackManager.Disconnect();
            }
            else
            {
                OpentrackStatusText.Text = "Starting...";
                await _opentrackManager.ConnectAsync();
            }
        }

        private void ForceStopButton_Click(object sender, RoutedEventArgs e)
        {
            _receiver.Stop();
            _opentrackManager.Disconnect();
        }

        private MapMonitorWindow? _monitorWindow;

        private void ShowMonitorButton_Click(object sender, RoutedEventArgs e)
        {
            if (_monitorWindow == null || !_monitorWindow.IsLoaded)
            {
                _monitorWindow = new MapMonitorWindow(_opentrackManager);
                _monitorWindow.Show();
            }
            else
            {
                _monitorWindow.WindowState = WindowState.Normal;
                _monitorWindow.Activate();
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}