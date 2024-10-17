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
using Logic;
using System.Windows.Threading;
using System.Windows.Media.Imaging;


namespace cnc_gui
{
    /// <summary>
    /// home.xaml 的互動邏輯
    /// </summary>
    public partial class home : Page
    {
        private setting settingsPage;

        private Random _random = new Random();
        private List<DateTime> _timestamps = new List<DateTime>(); // 
        private const int MaxDataPoints = 10;

        private DispatcherTimer timer;
        public home()
        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            settingsPage.SaveConfig();
            InitializeComponent();

            // 在初始化時，將 setting.xaml 中的靜態變數的值顯示在 TextBlock

            home_ip.Text = setting.Cncip;
            home_port.Text = setting.Cncport;


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

            /*
            if (Logic.core.r == 0)
            {
                connect_light.Foreground = new SolidColorBrush(Colors.Green);
            }
            else
            {
                connect_light.Foreground = new SolidColorBrush(Colors.Red);
            }
            */

            //圖表更新
            energy_run_time = new SeriesCollection
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

            // 初始化並配置 DispatcherTimer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10); // 設置為每 10 秒觸發一次
            timer.Tick += Timer_Tick; // 每次觸發執行的事件
            timer.Start();
            Task.Run(UpdateChart);
        }

        public void LoadImage(string imagePath)
        {
            try
            {
                // 建立 BitmapImage 物件
                BitmapImage bitmap = new BitmapImage();

                // 開始初始化
                bitmap.BeginInit();

                // 設定圖片來源
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);

                // 防止圖片被鎖定，允許快取選項
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                // 結束初始化
                bitmap.EndInit();

                // 將圖片設定到 Image 控制項
                source_img_home.Source = bitmap;
            }
            catch (Exception ex)
            {
                // 如果發生錯誤，顯示預設圖片
                source_img_home.Source = new BitmapImage(new Uri("icon/no_img.png", UriKind.Relative));
                // 你也可以選擇記錄錯誤訊息
                Console.WriteLine(ex.Message);
            }
        }

        // 此方法用於載入設定並更新 UI
        public void Timer_Tick(object sender, EventArgs e)
        {
            settingsPage.LoadConfig(); // 重新載入配置
            UpdatehomeProgressBars();      // 更新進度條
        }
        public  void UpdateUI()
        {
            settingsPage.LoadConfig(); // 重新載入配置
            UpdatehomeProgressBars();      // 更新進度條
        }
        // 更新進度條的數值
        public void UpdatehomeProgressBars()
        {
            
                switch (setting.Flusher_level_bar_R2)
                {
                    case 1:
                        flusher_level_bar.Value = double.Parse(setting.flusherLevels[0]);
                        break;
                    case 2:
                        flusher_level_bar.Value = double.Parse(setting.flusherLevels[1]);
                        break;
                    case 3:
                        flusher_level_bar.Value = double.Parse(setting.flusherLevels[2]);
                        break;
                    case 4:
                        flusher_level_bar.Value = double.Parse(setting.flusherLevels[3]);
                        break;
                    case 5:
                        flusher_level_bar.Value = setting.Excluder_level_bar * 20;
                        break;
                }
                switch (setting.Excluder_level_bar)
                {
                    case 1:
                        excluder_level_bar.Value = double.Parse(setting.excluderLevels[0]);
                        break;
                    case 2:
                        excluder_level_bar.Value = double.Parse(setting.excluderLevels[1]);
                        break;
                    case 3:
                        excluder_level_bar.Value = double.Parse(setting.excluderLevels[2]);
                        break;
                    case 4:
                        excluder_level_bar.Value = double.Parse(setting.excluderLevels[3]);
                        break;
                    case 5:
                        excluder_level_bar.Value = setting.Excluder_level_bar * 20;
                        break;
                }
                flusher_level_home.Text = setting.Flusher_level_bar_R2.ToString();
                excluder_level_home.Text = setting.Excluder_level_bar.ToString();
                spindle_load_bar.Value = setting.Spindle_load;
                spindle_load_home.Text = setting.Spindle_load.ToString();
                LoadImage(@"D:\cnc_gui_s-master\method_AI\origin\ori.jpg");
            }
        

        public SeriesCollection energy_run_time { get; set; }
        public Func<double, string> Formatter { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        private async Task UpdateChart()
        {
            while (true)
            {

                await Task.Delay(10000); // 每秒更新一次
                var currentTime = DateTime.Now;
                var newValue = _random.NextDouble() * 10; // 模擬新數據 //setting.Flusher_level_bar
                Application.Current.Dispatcher.Invoke(() =>
                {
                    energy_run_time[0].Values.Add(newValue);
                    _timestamps.Add(currentTime);

                    if (energy_run_time[0].Values.Count > MaxDataPoints)
                    {
                        energy_run_time[0].Values.RemoveAt(0);
                        _timestamps.RemoveAt(0);
                    }
                });
                OnPropertyChanged(nameof(energy_run_time));
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private  void program_start_Checked(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("程式已啟動");
            core.MainProcessing(true);

        }
        private void program_stop_Checked(object sender, RoutedEventArgs e)
        {
            core.MainProcessing(false);
            core.OnOff = false;
            settingsPage.config.Excluder_level_bar=0;
            settingsPage.SaveConfig();
            settingsPage.config.Flusher_level_bar_R2=0;
            settingsPage.SaveConfig();
            // 發送取消請求
            if (core.cancellationTokenSource != null)
            {
                core.cancellationTokenSource.Cancel();
            }
            if (core.mainThread != null && core.mainThread.IsAlive)
            {
                core.mainThread.Join(1000);
            }
            if (core.excluderThread != null && core.excluderThread.IsAlive)
            {
                core.excluderThread.Join(1000); 
            }
            if (core.RdspmeterThread != null && core.RdspmeterThread.IsAlive)
            {
                core.RdspmeterThread.Join(1000); 
            }
            core.mainThread = null;
            core.excluderThread = null;
            core.RdspmeterThread = null;
            MessageBox.Show("程式已停止");
        }

    }
}
