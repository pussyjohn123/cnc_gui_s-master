using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Focas;
using System.Windows.Threading;
using System.Configuration;
using System.Windows.Media.Imaging;

namespace cnc_gui
{
    /// <summary>
    /// cam.xaml 的互動邏輯
    /// </summary>
    public partial class cam : Page
    {

        private setting settingsPage;

        private Random _random = new Random();
        private List<DateTime> _timestamps = new List<DateTime>(); // 
        private const int MaxDataPoints = 10;

        private DispatcherTimer timer;

        public cam()
        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            settingsPage.SaveConfig();
            InitializeComponent();

            flusher_lv1_str.Text = setting.flusherlevel_st[0];
            flusher_lv2_str.Text = setting.flusherlevel_st[1];
            flusher_lv3_str.Text = setting.flusherlevel_st[2];
            flusher_lv4_str.Text = setting.flusherlevel_st[3];
            flusher_lv5_str.Text = setting.flusherlevel_st[4];

            flusher_lv1_time.Text = setting.flusher_time[0];
            flusher_lv2_time.Text = setting.flusher_time[1];
            flusher_lv3_time.Text = setting.flusher_time[2];
            flusher_lv4_time.Text = setting.flusher_time[3];
            flusher_lv5_time.Text = setting.flusher_time[4];

            flusher_run_time = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Sample Data",
                    Values = new ChartValues<double> { }
                }
            };

            Formatter = value =>
            {
                int index = (int)value;
                if (index >= 0 && index < _timestamps.Count)
                {
                    return _timestamps[index].ToString("HH:mm:ss");
                }
                return string.Empty;
            };

            DataContext = this;
            Task.Run(UpdateChart);

            UpdateProgressBars();

            // 初始化並配置 DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 設置為每 10 秒觸發一次
            timer.Tick += Timer_Tick; // 每次觸發執行的事件
            timer.Start();
            Task.Run(UpdateChart);
        }
        public void LoadImage(string filePath, Image imageControl)
        {
            try
            {
                // 確認圖片檔案是否存在
                if (File.Exists(filePath))
                {
                    // 建立 BitmapImage 物件
                    BitmapImage bitmap = new BitmapImage();

                    // 開始初始化
                    bitmap.BeginInit();

                    // 設定圖片來源
                    bitmap.UriSource = new Uri(filePath, UriKind.Absolute);

                    // 防止圖片被鎖定，允許快取選項
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    // 結束初始化
                    bitmap.EndInit();

                    // 將圖片設定到指定的 Image 控制項
                    imageControl.Source = bitmap;
                }
                else
                {
                    // 如果圖片不存在，顯示預設圖片
                    imageControl.Source = new BitmapImage(new Uri("icon/no_img.png", UriKind.Relative));
                    Console.WriteLine($"圖片不存在：{filePath}");
                }
            }
            catch (Exception ex)
            {
                // 處理錯誤並顯示預設圖片
                imageControl.Source = new BitmapImage(new Uri("icon/no_img.png", UriKind.Relative));
                Console.WriteLine($"錯誤訊息：{ex.Message}");
            }
        }

        // 此方法用於載入設定並更新 UI
        private void Timer_Tick(object sender, EventArgs e)
        {
            settingsPage.LoadConfig(); // 重新載入配置
            UpdateProgressBars();      // 更新進度條
            LoadImage(@"D:\cnc_gui_s-master\method_AI\origin\ori.jpg", source_img_cam);
            LoadImage(@"D:\cnc_gui_s-master\method_AI\r1\r1.jpg", roi_1_img);
            LoadImage(@"D:\cnc_gui_s-master\method_AI\r2\r2.jpg", roi_2_img);
        }

        private void UpdateProgressBars()
        {
            flusher_level_cam.Text = setting.Flusher_level_bar_R2.ToString();
            switch (setting.Flusher_level_bar_R2)
            {
                case 1:
                    flusher_level_bar.Value = double.Parse(setting.flusherLevels[0]);
                    flusher_level_time.Text = setting.flusher_time[0];
                    break;
                case 2:
                    flusher_level_bar.Value = double.Parse(setting.flusherLevels[1]);
                    flusher_level_time.Text = setting.flusher_time[1];
                    break;
                case 3:
                    flusher_level_bar.Value = double.Parse(setting.flusherLevels[2]);
                    flusher_level_time.Text = setting.flusher_time[2];
                    break;
                case 4:
                    flusher_level_bar.Value = double.Parse(setting.flusherLevels[3]);
                    flusher_level_time.Text = setting.flusher_time[3];
                    break;
                case 5:
                    flusher_level_bar.Value = setting.Excluder_level_bar * 20;
                    flusher_level_time.Text = setting.flusher_time[4];
                    break;
            }

        }

        public SeriesCollection flusher_run_time { get; set; }
        public Func<double, string> Formatter { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task UpdateChart()
        {
            while (true)
            {
                await Task.Delay(10000); // 每秒更新一次
                var currentTime = DateTime.Now;
                var newValue = _random.NextDouble() * 10; // 模擬新數據
                Application.Current.Dispatcher.Invoke(() =>
                {
                    flusher_run_time[0].Values.Add(newValue);
                    _timestamps.Add(currentTime);

                    if (flusher_run_time[0].Values.Count > MaxDataPoints)
                    {
                        flusher_run_time[0].Values.RemoveAt(0);
                        _timestamps.RemoveAt(0);
                    }
                });

                OnPropertyChanged(nameof(flusher_run_time));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
