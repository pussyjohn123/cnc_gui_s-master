using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace cnc_gui
{
    /// <summary>
    /// energy.xaml 的互動邏輯
    /// </summary>
    public partial class energy : Page
    {
        private Random _random = new Random();
        private const int MaxDataPoints = 10;
        private List<DateTime> _firstChartTimestamps = new List<DateTime>();
        private List<DateTime> _secondChartTimestamps = new List<DateTime>();
        private List<DateTime> _thirdChartTimestamps = new List<DateTime>();

        public energy()
        {
            InitializeComponent();

            // 初始化 flusher_energy 折線圖
            flusher_energy = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Flusher Energy",
                    Values = new ChartValues<double> { }
                }
            };

            // 初始化 excluder_energy 折線圖
            excluder_energy = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Excluder Energy",
                    Values = new ChartValues<double> { }
                }
            };

            // 初始化 total_energy 折線圖
            total_energy = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Total Energy",
                    Values = new ChartValues<double> { }
                }
            };

            // 設定 X 軸的標籤格式化器為環形計數
            FirstChartFormatter = value =>
            {
                int index = (int)value;
                if (_firstChartTimestamps.Count > 0) // 確保 _firstChartTimestamps 有數據
                {
                    index = index % _firstChartTimestamps.Count; // 使用 mod 計算環形
                    return _firstChartTimestamps[index].ToString("HH:mm:ss");
                }
                return string.Empty;
            };

            SecondChartFormatter = value =>
            {
                int index = (int)value;
                if (_secondChartTimestamps.Count > 0) // 確保 _secondChartTimestamps 有數據
                {
                    index = index % _secondChartTimestamps.Count; // 使用 mod 計算環形
                    return _secondChartTimestamps[index].ToString("HH:mm:ss");
                }
                return string.Empty;
            };

            ThirdChartFormatter = value =>
            {
                int index = (int)value;
                if (_thirdChartTimestamps.Count > 0) // 確保 _thirdChartTimestamps 有數據
                {
                    index = index % _thirdChartTimestamps.Count; // 使用 mod 計算環形
                    return _thirdChartTimestamps[index].ToString("HH:mm:ss");
                }
                return string.Empty;
            };

            DataContext = this;

            // 啟動數據更新
            Task.Run(UpdateFirstChart);
            Task.Run(UpdateSecondChart);
            Task.Run(UpdateThirdChart);
        }

        // flusher_energy 折線圖的數據
        public SeriesCollection flusher_energy { get; set; }
        public Func<double, string> FirstChartFormatter { get; set; }

        // excluder_energy 折線圖的數據
        public SeriesCollection excluder_energy { get; set; }
        public Func<double, string> SecondChartFormatter { get; set; }

        // total_energy 折線圖的數據
        public SeriesCollection total_energy { get; set; }
        public Func<double, string> ThirdChartFormatter { get; set; }

        private async Task UpdateFirstChart()
        {
            while (true)
            {
                await Task.Delay(1000);
                var newValue = _random.NextDouble() * 10;
                var currentTime = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    flusher_energy[0].Values.Add(newValue);
                    _firstChartTimestamps.Add(currentTime);

                    if (flusher_energy[0].Values.Count > MaxDataPoints)
                    {
                        flusher_energy[0].Values.RemoveAt(0);
                        _firstChartTimestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(flusher_energy));
            }
        }

        private async Task UpdateSecondChart()
        {
            while (true)
            {
                await Task.Delay(1500);
                var newValue = _random.NextDouble() * 20;
                var currentTime = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    excluder_energy[0].Values.Add(newValue);
                    _secondChartTimestamps.Add(currentTime);

                    if (excluder_energy[0].Values.Count > MaxDataPoints)
                    {
                        excluder_energy[0].Values.RemoveAt(0);
                        _secondChartTimestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(excluder_energy));
            }
        }

        private async Task UpdateThirdChart()
        {
            while (true)
            {
                await Task.Delay(2000);
                var newValue = _random.NextDouble() * 30;
                var currentTime = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    total_energy[0].Values.Add(newValue);
                    _thirdChartTimestamps.Add(currentTime);

                    if (total_energy[0].Values.Count > MaxDataPoints)
                    {
                        total_energy[0].Values.RemoveAt(0);
                        _thirdChartTimestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(total_energy));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
