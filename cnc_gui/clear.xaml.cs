using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static Focas;

namespace cnc_gui
{
    /// <summary>
    /// clear.xaml 的互動邏輯
    /// </summary>
    public partial class clear : Page
    {
        private setting settingsPage;
        private Random _random = new Random();
        private const int MaxDataPoints = 10;
        private List<DateTime> _firstChartTimestamps = new List<DateTime>();
        private List<DateTime> _secondChartTimestamps = new List<DateTime>();

        private DispatcherTimer timer;

        public clear()
        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            settingsPage.SaveConfig();
            InitializeComponent();
            excluder_lv1_str.Text = setting.excluderlevel_st[0];
            excluder_lv2_str.Text = setting.excluderlevel_st[1];
            excluder_lv3_str.Text = setting.excluderlevel_st[2];
            excluder_lv4_str.Text = setting.excluderlevel_st[3];
            excluder_lv5_str.Text = setting.excluderlevel_st[4];

            excluder_lv1_time.Text = setting.excluder_time[0];
            excluder_lv2_time.Text = setting.excluder_time[1];
            excluder_lv3_time.Text = setting.excluder_time[2];
            excluder_lv4_time.Text = setting.excluder_time[3];
            excluder_lv5_time.Text = setting.excluder_time[4];



            flusher_run_time = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "First Chart",
                    Values = new ChartValues<double> { }
                }
            };

            // 初始化第二個折線圖
            excluder_run_time = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Second Chart",
                    Values = new ChartValues<double> { }
                }
            };

            // 設定 X 軸的標籤格式化器1
            FirstChartFormatter = value =>
            {
                int index = (int)value;

                if (_firstChartTimestamps.Count > 0) // 確保 _firstChartTimestamps 有數據
                {
                    // 使用 mod 計算，確保 index 在範圍內進行環形
                    index = index % _firstChartTimestamps.Count;

                    // 返回對應時間的格式化字串
                    return _firstChartTimestamps[index].ToString("HH:mm:ss");
                }

                return string.Empty;
            };
            // 設定 X 軸的標籤格式化器2
            SecondChartFormatter = value =>
            {
                int index = (int)value;

                if (_secondChartTimestamps.Count > 0)
                {

                    index = index % _secondChartTimestamps.Count;
                    return _secondChartTimestamps[index].ToString("HH:mm:ss");
                }

                return string.Empty;
            };

            DataContext = this;

            Task.Run(UpdateFirstChart);
            Task.Run(UpdateSecondChart);

            UpdateProgressBars();

            // 初始化並配置 DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 設置為每 10 秒觸發一次
            timer.Tick += Timer_Tick; // 每次觸發執行的事件
            timer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            settingsPage.LoadConfig(); // 重新載入配置
            UpdateProgressBars();      // 更新進度條
        }

        private void UpdateProgressBars()
        {
            excluder_level_clear.Text = setting.Excluder_level_bar.ToString();
            switch (setting.Excluder_level_bar)
            {
                case 1:
                    excluder_level_bar.Value = double.Parse(setting.excluderLevels[0]);
                    excluder_level_time.Text = setting.excluder_time[0];
                    break;
                case 2:
                    excluder_level_bar.Value = double.Parse(setting.excluderLevels[1]);
                    excluder_level_time.Text = setting.excluder_time[1];
                    break;
                case 3:
                    excluder_level_bar.Value = double.Parse(setting.excluderLevels[2]);
                    excluder_level_time.Text = setting.excluder_time[2];
                    break;
                case 4:
                    excluder_level_bar.Value = double.Parse(setting.excluderLevels[3]);
                    excluder_level_time.Text = setting.excluder_time[3];
                    break;
                case 5:
                    excluder_level_bar.Value = setting.Excluder_level_bar * 20;
                    excluder_level_time.Text = setting.excluder_time[4];
                    break;
            }

        }

        public SeriesCollection flusher_run_time { get; set; }
        public Func<double, string> FirstChartFormatter { get; set; }

        // 第二個折線圖的數據
        public SeriesCollection excluder_run_time { get; set; }
        public Func<double, string> SecondChartFormatter { get; set; }

        private async Task UpdateFirstChart()
        {
            while (true)
            {
                await Task.Delay(1000);
                var newValue = _random.NextDouble() * 10;
                var currentTime = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    flusher_run_time[0].Values.Add(newValue);
                    _firstChartTimestamps.Add(currentTime);

                    if (flusher_run_time[0].Values.Count > MaxDataPoints)
                    {
                        flusher_run_time[0].Values.RemoveAt(0);
                        _firstChartTimestamps.RemoveAt(0);
                    }
                });

                
                OnPropertyChanged(nameof(flusher_run_time));
            }
        }

        private async Task UpdateSecondChart()
        {
            while (true)
            {
                await Task.Delay(1500); // 第二個圖表每 1.5 秒更新一次
                var newValue = _random.NextDouble() * 20;
                var currentTime = DateTime.Now;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    excluder_run_time[0].Values.Add(newValue);
                    _secondChartTimestamps.Add(currentTime);

                    if (excluder_run_time[0].Values.Count > MaxDataPoints)
                    {
                        excluder_run_time[0].Values.RemoveAt(0);
                        _secondChartTimestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(excluder_run_time));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
