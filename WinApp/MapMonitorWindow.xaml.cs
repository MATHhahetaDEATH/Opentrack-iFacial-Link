using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace VTubeLink
{
    public class TagColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tag)
            {
                if (tag == "OPT") return new SolidColorBrush(Colors.Blue);
                if (tag == "UDP") return new SolidColorBrush(Colors.Green);
                if (tag == "SYS") return new SolidColorBrush(Colors.Orange);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MapMonitorWindow : Window
    {
        private readonly OpentrackManager _opentrackManager;
        
        public ObservableCollection<LogLine> FilteredLogs { get; set; } = new();

        public MapMonitorWindow(OpentrackManager opentrackManager)
        {
            InitializeComponent();
            _opentrackManager = opentrackManager;
            
            LogListBox.ItemsSource = FilteredLogs;

            AppLogger.Logs.CollectionChanged += Logs_CollectionChanged;
            _opentrackManager.MappingUpdated += OpentrackManager_MappingUpdated;

            this.Loaded += (s, e) => UpdateLogsFilter();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            UpdateLogsFilter();
        }

        private void UpdateLogsFilter()
        {
            if (!IsLoaded) return;

            bool showUdp = ChkLogUdp.IsChecked ?? false;
            bool showOpt = ChkLogOpt.IsChecked ?? false;
            bool showSys = ChkLogSys.IsChecked ?? false;

            FilteredLogs.Clear();
            foreach (var log in AppLogger.Logs.ToList())
            {
                if ((log.Tag == "UDP" && showUdp) ||
                    (log.Tag == "OPT" && showOpt) ||
                    (log.Tag == "SYS" && showSys))
                {
                    FilteredLogs.Add(log);
                }
            }

            // Scroll to end
            if (FilteredLogs.Count > 0)
            {
                LogListBox.ScrollIntoView(FilteredLogs[FilteredLogs.Count - 1]);
            }
        }

        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    bool showUdp = ChkLogUdp.IsChecked ?? false;
                    bool showOpt = ChkLogOpt.IsChecked ?? false;
                    bool showSys = ChkLogSys.IsChecked ?? false;

                    foreach (LogLine log in e.NewItems)
                    {
                        if ((log.Tag == "UDP" && showUdp) ||
                            (log.Tag == "OPT" && showOpt) ||
                            (log.Tag == "SYS" && showSys))
                        {
                            FilteredLogs.Add(log);
                        }
                    }

                    if (FilteredLogs.Count > 500)
                    {
                        FilteredLogs.RemoveAt(0);
                    }

                    if (FilteredLogs.Count > 0 && VisualTreeHelper.GetChildrenCount(LogListBox) > 0)
                    {
                        LogListBox.ScrollIntoView(FilteredLogs[FilteredLogs.Count - 1]);
                    }
                });
            }
        }

        private void OpentrackManager_MappingUpdated(MappingUpdate update)
        {
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ArkitGrid.ItemsSource = update.ArkitParams.OrderBy(x => x.Key).ToList();
                MappedGrid.ItemsSource = update.MappedParams.OrderBy(x => x.Key).ToList();
            });
        }

        private void BtnClearLogs_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AppLogger.Logs.Clear();
                FilteredLogs.Clear();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AppLogger.Logs.CollectionChanged -= Logs_CollectionChanged;
            _opentrackManager.MappingUpdated -= OpentrackManager_MappingUpdated;
            base.OnClosing(e);
        }
    }
}
