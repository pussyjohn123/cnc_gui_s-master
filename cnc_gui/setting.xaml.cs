using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static cnc_gui.setting;
using Logic;
namespace cnc_gui
{
    
    /// <summary>
    /// setting.xaml 的互動邏輯
    /// </summary>
    public partial class setting : Page
    {
        //json檔變數宣告
        public class Config
        {
            public static readonly object lockObj = new object();
            public int Flusher_level_bar_R2 {get; set;}
            public int Flusher_level_bar_R1 { get; set; }
            public string Cncip { get; set; }
            public string Cncport { get; set; }
            public double Flusher_level_bar { get; set; }
            public double Excluder_level_bar { get; set; }
            public string Flusher_address { get; set; }
            public string Excluder_address { get; set; }
            public string Flusher_address_decimal { get; set; }
            public string Excluder_address_decimal { get; set; }
            public string Flusher_address_handl { get; set; }
            public string Excluder_address_handl { get; set; }
            public long spindle_load { get; set; }
            public string[] FlusherLevels { get; set; }
            public string[] FlusherLevelsl { get; set; }
            public string[] FlusherlevelSt { get; set; }
            public string[] FlusherTime { get; set; }
            public string[] ExcluderLevels { get; set; }
            public string[] ExcluderLevelsl { get; set; }
            public string[] ExcluderlevelSt { get; set; }
            public string[] ExcluderTime { get; set; }
            public string Excluder_period { get; set; }
            public string Image_processing_time { get; set; }
            public string Total_flusher_time { get; set; }
            public string Delay_time { get; set; }
        }

        public string configFilePath = "config.json"; // 存放設定的檔案路徑
        public Config config { get; set; } // 儲存設定的物件


        public static string Cncip { get; private set; } = "192.168.0.200"; // 預設 IP 地址
        public static string Cncport { get; private set; } = "8193"; // 預設 Port 號

        public static string flusher_address { get; set; } = "7000"; //底座環沖地址
        public static string excluder_address { get; set; } = "7000";//排屑機地址
        public static string flusher_address_handl { get; set; } = "5"; //底座環沖手動運行
        public static string excluder_address_handl { get; set; } = "5";//排屑機手動運行
        public static string flusher_binary { get; set; } = ""; //底座環沖二元數
        public static string excluder_binary { get; set; } = "";//排屑機二元數
        public static string flusher_address_decimal { get; private set; } = "0"; //底座環沖地址小數點
        public static string excluder_address_decimal { get; private set; } = "0";//排屑機地址小數點
        //public static double flusher_level_bar { get; private set; } = 0; //底座環沖積屑量級條狀圖
        public static double Excluder_level_bar { get; private set; } = 0;//排屑機排屑量級條狀圖
        public static long Spindle_load { get; private set; } = 0; //主軸負載
        public string flusherPercentage { get; set; } = "選擇"; //底座環沖運行規則選單
        public string excluderPercentage { get; set; } = "選擇";//排屑機運行規則選單
        public static int Flusher_level_bar_R2 { get; set; } = 0;
        public static int Flusher_level_bar_R1 { get; set; } =0;
        public static string[] flusherLevels { get; private set; } = { "20", "40", "60", "80" };//底座環沖運行規則level(可供更改)
        public static string[] flusherLevelsl { get; private set; } = { "21", "41", "61", "81" };
        public static string[] flusherlevel_st = { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" };//用於字串顯示底座環沖量級
        public static string[] flusher_time = { "0", "10", "20", "30", "40" };//底座環沖運行時間

        public static string[] excluderLevels { get; private set; } = { "20", "40", "60", "80" };//排屑機運行規則level(可供更改)
        public static string[] excluderLevelsl { get; private set; } = { "21", "41", "61", "81" };
        public static string[] excluderlevel_st = { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" };//用於字串顯示排屑機量級
        public static string[] excluder_time = { "0", "10", "20", "30", "40" };//排屑機運行時間
        public static string excluder_period = "5";//排屑機運行週期(由N個底座環沖時間段組成)

        public static string Image_processing_time { get; private set; } = "10";//影像處理時間
        public static string Total_flusher_time { get; private set; } = "10";//底座環沖總時間
        public static string Delay_time { get; private set; } = "10";//延遲時間

        private SeriesCollection _pie_Flusher_time_total;
        public SeriesCollection pie_Flusher_time_total
        {
            get => _pie_Flusher_time_total;
            set
            {
                _pie_Flusher_time_total = value;
                OnPropertyChanged(nameof(pie_Flusher_time_total)); // 通知 WPF 刷新 UI
            }
        }
        //警告並跳出messagebox系統
        public string messageboxtext = "test";
        public string caption = "警告";
        MessageBoxButton button = MessageBoxButton.YesNoCancel;
        MessageBoxImage icon = MessageBoxImage.Warning;
        MessageBoxResult result;


        //讀取config.json
        public void LoadConfig()
        {
            lock (Config.lockObj)
            {
                // 如果設定檔不存在，就建立一個預設的 Config
                if (!File.Exists(configFilePath))
                {
                    config = new Config
                    {
                        Cncip = "192.168.0.200",
                        Cncport = "8193",
                        Excluder_period = "5",
                        Image_processing_time = "10",
                        Total_flusher_time = "10",
                        Delay_time = "10",
                        spindle_load = 40,
                        Flusher_address = "7000",
                        Excluder_address = "7000",
                        Flusher_address_handl = "0",
                        Excluder_address_handl = "0",
                        Flusher_address_decimal = "0",
                        Excluder_address_decimal = "0",
                        // Flusher_level_bar = 4,
                        Excluder_level_bar = 1,
                        FlusherLevels = new[] { "20", "40", "60", "80" },
                        FlusherLevelsl = new[] { "21", "41", "61", "81" },
                        FlusherlevelSt = new[] { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" },
                        FlusherTime = new[] { "0", "10", "20", "30", "40" },
                        ExcluderLevels = new[] { "20", "40", "60", "80" },
                        ExcluderLevelsl = new[] { "21", "41", "61", "81" },
                        ExcluderlevelSt = new[] { "0%~20%", "21%~40%", "41%~60%", "61%~80%", "81%~100%", "", "" },
                        ExcluderTime = new[] { "0", "10", "20", "30", "40" },
                        Flusher_level_bar_R2= 0,
                        Flusher_level_bar_R1 = 0

                    };

                    SaveConfig(); // 預設儲存設定檔
                }
                else
                {
                    // 讀取已存在的設定檔
                    string json = File.ReadAllText(configFilePath);
                    config = JsonConvert.DeserializeObject<Config>(json);


                    //flusher_level_bar = config.Flusher_level_bar;
                    Excluder_level_bar = config.Excluder_level_bar;
                    Spindle_load = config.spindle_load;
                    flusherLevels = config.FlusherLevels;
                    flusherLevelsl = config.FlusherLevelsl;
                    flusherlevel_st = config.FlusherlevelSt;
                    flusher_time = config.FlusherTime;
                    Flusher_level_bar_R2 = config.Flusher_level_bar_R2;
                    excluderLevels = config.ExcluderLevels;
                    excluderLevelsl = config.ExcluderLevelsl;
                    excluderlevel_st = config.ExcluderlevelSt;
                    excluder_time = config.ExcluderTime;
                    excluder_period = config.Excluder_period;

                    Image_processing_time = config.Image_processing_time;
                    Total_flusher_time = config.Total_flusher_time;
                    Delay_time = config.Delay_time;
                    flusher_address_decimal = config.Flusher_address_decimal;
                    excluder_address_decimal = config.Excluder_address_decimal;
                    flusher_address = config.Flusher_address;
                    excluder_address = config.Excluder_address;
                    flusher_address_handl = config.Flusher_address_handl;
                    excluder_address_handl = config.Excluder_address_handl;

                }
            }
        }
        //儲存config
        public void SaveConfig()
        {
            lock (Config.lockObj)
            {
                config.ExcluderLevels = excluderLevels; // 更新配置中的 excluderLevels
                config.ExcluderLevelsl = excluderLevelsl;
                config.ExcluderlevelSt = excluderlevel_st;
                config.ExcluderTime = excluder_time;
                config.FlusherLevels = flusherLevels;
                config.FlusherLevelsl = flusherLevelsl;
                config.FlusherlevelSt = flusherlevel_st;
                config.FlusherTime = flusher_time;
                config.Flusher_level_bar_R2 = Flusher_level_bar_R2;
                //config.Flusher_level_bar = flusher_level_bar;
                config.Excluder_level_bar = Excluder_level_bar;
                config.spindle_load = Spindle_load;
                config.Excluder_period = excluder_period;
                config.Image_processing_time = Image_processing_time;
                config.Total_flusher_time = Total_flusher_time;
                config.Delay_time = Delay_time;


                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configFilePath, json);
            }
            }


        //%樹轉換成string
        public static void GenerateLevels()
        {
            // 第一個範圍從 0% 開始
            flusherlevel_st[0] = "0%~" + flusherLevels[0] + "%";
            excluderlevel_st[0] = "0%~" + excluderLevels[0] + "%";

            // 生成 flusherlevel_st
            for (int i = 0; i < flusherLevels.Length - 1; i++)
            {
                flusherlevel_st[i + 1] = flusherLevelsl[i] + "%~" + flusherLevels[i + 1] + "%";
            }
            flusherlevel_st[4] = flusherLevelsl[3] + "%~100%";  // 最後一個範圍

            // 生成 excluderlevel_st
            for (int i = 0; i < excluderLevels.Length - 1; i++)
            {
                excluderlevel_st[i + 1] = excluderLevelsl[i] + "%~" + excluderLevels[i + 1] + "%";
            }
            excluderlevel_st[4] = excluderLevelsl[3] + "%~100%";  // 最後一個範圍
        }
        public setting()
        {
            InitializeComponent();
            LoadConfig();
            // 初始化時，將靜態變數值顯示在 TextBox 上
            setting_ip.Text = config.Cncip;
            setting_port.Text = config.Cncport;

            excluder_period_st.Text = excluder_period;

            image_processing_time.Text = Image_processing_time;
            total_flusher_time.Text = Total_flusher_time;
            delay_time.Text = Delay_time;

            flusher_lev1_r.Text = flusherLevels[0];
            flusher_lev2_r.Text = flusherLevels[1];
            flusher_lev3_r.Text = flusherLevels[2];
            flusher_lev4_r.Text = flusherLevels[3];

            flusher_lev2_l.Text = flusherLevelsl[0];
            flusher_lev3_l.Text = flusherLevelsl[1];
            flusher_lev4_l.Text = flusherLevelsl[2];
            flusher_lev5_l.Text = flusherLevelsl[3];

            excluder_lev1_r.Text = excluderLevels[0];
            excluder_lev2_r.Text = excluderLevels[1];
            excluder_lev3_r.Text = excluderLevels[2];
            excluder_lev4_r.Text = excluderLevels[3];

            excluder_lev2_l.Text = excluderLevelsl[0];
            excluder_lev3_l.Text = excluderLevelsl[1];
            excluder_lev4_l.Text = excluderLevelsl[2];
            excluder_lev5_l.Text = excluderLevelsl[3];

            // 註冊 TextChanged 事件來監控文本變化
            setting_ip.TextChanged += Setting_ip_TextChanged;
            setting_port.TextChanged += Setting_port_TextChanged;

            flusher_time_st.TextChanged += flusher_time_st_TextChanged;
            excluder_time_st.TextChanged += excluder_time_st_TextChanged;

            excluder_period_st.TextChanged += excluder_period_st_TextChanged;
            total_flusher_time.TextChanged += total_flusher_time_TextChanged;
            image_processing_time.TextChanged += image_processing_time_TextChanged;
            delay_time.TextChanged += delay_time_TextChanged;

            flusher_lev1_r.TextChanged += flusher_lev1_r_TextChanged;
            flusher_lev2_r.TextChanged += flusher_lev2_r_TextChanged;
            flusher_lev3_r.TextChanged += flusher_lev3_r_TextChanged;
            flusher_lev4_r.TextChanged += flusher_lev4_r_TextChanged;

            excluder_lev1_r.TextChanged += excluder_lev1_r_TextChanged;
            excluder_lev2_r.TextChanged += excluder_lev2_r_TextChanged;
            excluder_lev3_r.TextChanged += excluder_lev3_r_TextChanged;
            excluder_lev4_r.TextChanged += excluder_lev4_r_TextChanged;
            


            total_work_time.Text = (int.Parse(Image_processing_time) + int.Parse(Total_flusher_time) + int.Parse(Delay_time)).ToString();
            pie_Flusher_time_total = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Image processing time",
                    Values = new ChartValues<int> { int.Parse(config.Image_processing_time) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
                },
                new PieSeries
                {
                    Title = "Total flusher time",
                    Values = new ChartValues < int > { int.Parse(config.Total_flusher_time) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
                },
                new PieSeries
                {
                    Title = "Delay time",
                    Values = new ChartValues<int> { int.Parse(config.Delay_time) },
                    DataLabels = true,
                    LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
                }
            };
            DataContext = this;
            // 初始化 PieSeries 並加入數據

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
        }

        private void UpdatePieChart()
        {
            pie_Flusher_time_total.Clear();
            pie_Flusher_time_total.Add(new PieSeries
            {
                Title = "Image Processing Time",
                Values = new ChartValues<int> { int.Parse(Image_processing_time) },
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
            });
            pie_Flusher_time_total.Add(new PieSeries
            {
                Title = "Total Flusher Time",
                Values = new ChartValues<int> { int.Parse(Total_flusher_time) },
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
            });
            pie_Flusher_time_total.Add(new PieSeries
            {
                Title = "Delay Time",
                Values = new ChartValues<int> { int.Parse(Delay_time) },
                DataLabels = true,
                LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation)
            });

            OnPropertyChanged(nameof(pie_Flusher_time_total));
        }


        //ip 更新
        private void Setting_ip_TextChanged(object sender, TextChangedEventArgs e)
        {
            Cncip = setting_ip.Text.ToString();
            config.Cncip = setting_ip.Text.ToString();
            SaveConfig(); // 儲存到 JSON
        }
        //port 更新
        private void Setting_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            Cncport = setting_port.Text.ToString();
            config.Cncport = setting_port.Text.ToString();
            SaveConfig(); // 儲存到 JSON
        }
        //選單欄狀態更新
        private void flusherButton_Checked(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            if (sender is RadioButton radioButton)
            {
                flusherPercentage = radioButton.Tag.ToString();
                DataContext = null;
                DataContext = this;
            }
            RadioButton selectedRadioButton = sender as RadioButton;
            if (selectedRadioButton != null)
            {
                // 取得選擇的範圍
                string selectedRange = selectedRadioButton.Tag.ToString();
                int index = Array.IndexOf(flusherlevel_st, selectedRange);

                if (index >= 0 && index < flusher_time.Length)
                {
                    // 根據選擇的範圍更新 TextBox
                    flusher_time_st.Text = flusher_time[index];
                    SaveConfig();
                }
            }
        }
        private void excluderButton_Checked(object sender, RoutedEventArgs e)
        {
            LoadConfig();

            if (sender is RadioButton radioButton)
            {
                excluderPercentage = radioButton.Tag.ToString();
                DataContext = null;
                DataContext = this;
            }
            RadioButton selectedRadioButton = sender as RadioButton;
            if (selectedRadioButton != null)
            {
                // 取得選擇的範圍
                string selectedRange = selectedRadioButton.Tag.ToString();
                int index = Array.IndexOf(excluderlevel_st, selectedRange);

                if (index >= 0 && index < excluder_time.Length)
                {
                    // 根據選擇的範圍更新 TextBox
                    excluder_time_st.Text = excluder_time[index];
                    SaveConfig();
                }
            }

        }

        private RadioButton FindCheckedRadioButtonForFlusher()
        {
            RadioButton[] radioButtons = { exp_flusher_time_lev1, exp_flusher_time_lev2, exp_flusher_time_lev3, exp_flusher_time_lev4, exp_flusher_time_lev5 };
            return radioButtons.FirstOrDefault(rb => rb.IsChecked == true);
        }
        private RadioButton FindCheckedRadioButtonForExcluder()
        {
            RadioButton[] radioButtons = { exp_excluder_time_lev1, exp_excluder_time_lev2, exp_excluder_time_lev3, exp_excluder_time_lev4, exp_excluder_time_lev5 };
            return radioButtons.FirstOrDefault(rb => rb.IsChecked == true);

        }
        //選單欄數值更新
        private void flusher_time_st_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string newTime = textBox.Text;

                // 更新 flusher_time 中的值
                RadioButton selectedRadioButton = FindCheckedRadioButtonForFlusher();
                if (selectedRadioButton != null)
                {
                    int index = Array.IndexOf(flusherlevel_st, selectedRadioButton.Tag.ToString());
                    if (index >= 0 && index < flusher_time.Length)
                    {
                        flusher_time[index] = newTime;
                        SaveConfig();
                    }
                }
            }
        }
        private void excluder_time_st_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string newTime = textBox.Text;

                // 更新 excluder_time 中的值
                RadioButton selectedRadioButton = FindCheckedRadioButtonForExcluder();
                if (selectedRadioButton != null)
                {
                    int index = Array.IndexOf(excluderlevel_st, selectedRadioButton.Tag.ToString());
                    if (index >= 0 && index < excluder_time.Length)
                    {
                        excluder_time[index] = newTime;
                        SaveConfig();
                    }
                }
            }
        }

     

        private void flusher_InitializeRadioButtons()
        {
            // 初始化 RadioButton 的 Tag
            RadioButton[] radioButtons = { exp_flusher_time_lev1, exp_flusher_time_lev2, exp_flusher_time_lev3, exp_flusher_time_lev4, exp_flusher_time_lev5 };
            for (int i = 0; i < radioButtons.Length; i++)
            {
                radioButtons[i].Tag = flusherlevel_st[i];
            }
        }
        private void excluder_InitializeRadioButtons()
        {
            // 初始化 RadioButton 的 Tag
            RadioButton[] radioButtons = { exp_excluder_time_lev1, exp_excluder_time_lev2, exp_excluder_time_lev3, exp_excluder_time_lev4, exp_excluder_time_lev5 };
            for (int i = 0; i < radioButtons.Length; i++)
            {
                radioButtons[i].Tag = excluderlevel_st[i];
            }
        }

        //更新底座環沖排屑機運行規則level按下enter更新

        private void flusher_lev1_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[0] = flusher_lev1_r.Text.ToString();
        }
        private void flusher_lev1_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[0], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev2_l.Text = level.ToString();
                    flusherLevelsl[0] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev2_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[1] = flusher_lev2_r.Text.ToString();
        }
        private void flusher_lev2_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[1], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev3_l.Text = level.ToString();
                    flusherLevelsl[1] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev3_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[2] = flusher_lev3_r.Text.ToString();
        }
        private void flusher_lev3_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[2], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev4_l.Text = level.ToString();
                    flusherLevelsl[2] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void flusher_lev4_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            flusherLevels[3] = flusher_lev4_r.Text.ToString();
        }
        private void flusher_lev4_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(flusherLevels[3], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    flusher_lev5_l.Text = level.ToString();
                    flusherLevelsl[3] = level.ToString();
                    GenerateLevels();
                    flusher_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }

        private void excluder_lev1_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[0] = excluder_lev1_r.Text.ToString();
        }
        private void excluder_lev1_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[0], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev2_l.Text = level.ToString();
                    excluderLevelsl[0] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev2_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[1] = excluder_lev2_r.Text.ToString();
        }
        private void excluder_lev2_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[1], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev3_l.Text = level.ToString();
                    excluderLevelsl[1] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev3_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[2] = excluder_lev3_r.Text.ToString();
        }
        private void excluder_lev3_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[2], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev4_l.Text = level.ToString();
                    excluderLevelsl[2] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }
        private void excluder_lev4_r_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluderLevels[3] = excluder_lev4_r.Text.ToString();
        }
        private void excluder_lev4_r_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluderLevels[3], out i))//防呆 只能輸入int
                {
                    int level = i + 1;
                    excluder_lev5_l.Text = level.ToString();
                    excluderLevelsl[3] = level.ToString();
                    GenerateLevels();
                    excluder_InitializeRadioButtons();
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }

        }

        private void excluder_period_st_TextChanged(object sender, TextChangedEventArgs e)
        {
            excluder_period = excluder_period_st.Text.ToString();
        }

        private void excluder_period_st_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(excluder_period, out i))//防呆 只能輸入int
                {
                    excluder_period = i.ToString();
                    config.Excluder_period = excluder_period;
                    SaveConfig();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        private void image_processing_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            Image_processing_time = image_processing_time.Text.ToString();
        }
        private void image_processing_time_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int i = 0;
                // 按下 Enter 鍵時修改數值
                if (int.TryParse(Image_processing_time, out i))//防呆 只能輸入int
                {
                    Image_processing_time = i.ToString();
                    config.Image_processing_time = Image_processing_time;
                    SaveConfig();
                    total_work_time.Text = (int.Parse(Image_processing_time) + int.Parse(Total_flusher_time) + int.Parse(Delay_time)).ToString();
                    UpdatePieChart();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void total_flusher_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            Total_flusher_time = total_flusher_time.Text.ToString();
        }
        private void total_flusher_time_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(Total_flusher_time, out int i))//防呆 只能輸入int
                {
                    Total_flusher_time = i.ToString();
                    config.Total_flusher_time = Total_flusher_time;
                    SaveConfig();
                    total_work_time.Text = (int.Parse(Image_processing_time) + int.Parse(Total_flusher_time) + int.Parse(Delay_time)).ToString();
                    UpdatePieChart();
                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void delay_time_TextChanged(object sender, TextChangedEventArgs e)
        {
            Delay_time = delay_time.Text.ToString();
        }
        private void delay_time_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (int.TryParse(Delay_time, out int i))//防呆 只能輸入int
                {
                    Delay_time = i.ToString();
                    config.Delay_time = Delay_time;
                    SaveConfig();
                    total_work_time.Text = (int.Parse(Image_processing_time) + int.Parse(Total_flusher_time) + int.Parse(Delay_time)).ToString();
                    UpdatePieChart();

                }
                else
                {
                    MessageBox.Show("請輸入整數", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        //10進制轉2進制陣列
        int[] flusher_ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            for (int i = 7; i >= 0; i--)
            {
                binaryarr[i] = decimalNumber % 2;
                decimalNumber /= 2;
                flusher_binary = binaryarr[i].ToString() + flusher_binary;
            }
            flusher_binary = string.Join("", binaryarr);
            return binaryarr;
        }
        int[] excluder_ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            excluder_binary = "";
            for (int i = 7; i >= 0; i--)
            {
                binaryarr[i] = decimalNumber % 2;
                decimalNumber /= 2;
                
                excluder_binary = binaryarr[i].ToString() + excluder_binary;
            }
            flusher_binary = string.Join("", binaryarr);
            return binaryarr;
        }

        //2進制轉10進制
        int flusher_ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                decimalValue += binaryArray[i] * (int)Math.Pow(2, binaryArray.Length - 1 - i);
            }
            return decimalValue;
        }
        int excluder_ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                decimalValue += binaryArray[i] * (int)Math.Pow(2, binaryArray.Length - 1 - i);
            }
            return decimalValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void address_load_Click(object sender, RoutedEventArgs e)
        {
            LoadConfig();
            int[] flusher_binaryArray = flusher_ConvertToBinaryArray(int.Parse(config.Flusher_address_decimal));
            int[] excluder_binaryArray = excluder_ConvertToBinaryArray(int.Parse(config.Excluder_address_decimal));
            int flusher_modifiedDecimalValue = flusher_ConvertBinaryArrayToDecimal(flusher_binaryArray);
            int excluder_modifiedDecimalValue = excluder_ConvertBinaryArrayToDecimal(excluder_binaryArray);
            flusher_address_binary_st.Text = flusher_binary;
            excluder_address_binary_st.Text = excluder_binary;
            flusher_address_decimal_st.Text = flusher_address_decimal;
            excluder_address_decimal_st.Text = excluder_address_decimal;
            flusher_address_st.Text = flusher_address;
            excluder_address_st.Text = excluder_address;
            flusher_address_handl_st.Text = flusher_address_handl;
            excluder_address_handl_st.Text = excluder_address_handl;
            
        }

        private void address_save_Click(object sender, RoutedEventArgs e)
        {
            if (flusher_address_binary_st.Text != "" && excluder_address_binary_st.Text != "")
            {
                int[] flusher_binaryArray = flusher_address_binary_st.Text.ToString().Select(c => int.Parse(c.ToString())).ToArray();
                int[] excluder_binaryArray = excluder_address_binary_st.Text.ToString().Select(c => int.Parse(c.ToString())).ToArray();
                int flusher_modifiedDecimalValue = flusher_ConvertBinaryArrayToDecimal(flusher_binaryArray);
                int excluder_modifiedDecimalValue = excluder_ConvertBinaryArrayToDecimal(excluder_binaryArray);
                config.Flusher_address_decimal = flusher_modifiedDecimalValue.ToString();
                config.Excluder_address_decimal = excluder_modifiedDecimalValue.ToString();
                config.Flusher_address_handl = flusher_address_handl_st.Text;
                config.Excluder_address_handl = excluder_address_handl_st.Text;
                config.Flusher_address = flusher_address_st.Text;
                config.Excluder_address = excluder_address_st.Text;
                SaveConfig();
                MessageBox.Show("寫入成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                flusher_address_decimal_st.Text = config.Flusher_address_decimal;
                excluder_address_decimal_st.Text = config.Excluder_address_decimal;
            }
            else
            {
                MessageBox.Show("請先在寫入之前按下讀取按鈕", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}