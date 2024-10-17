using System;
using System.Diagnostics;  // Process 類所在命名空間
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.IO;
using System.Timers;
using System.Threading;
using System.Threading.Tasks; // Task 類所在命名空間
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using static Logic.Focas1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using cnc_gui;
using static cnc_gui.setting;
using System.Data; 
using MySql.Data.MySqlClient;
using LiveCharts.Maps;
using static Focas;
namespace Logic
{
    public class core
    {
        private readonly object lockObj = new object();
        public static bool OnOff = true;   // 用來控制是否繼續運行
        public static Thread mainThread = null;  // 保存運行的線程
        public static Thread excluderThread = null;// 用於執行排屑機的線程
        public static Thread RdspmeterThread = null;// 用於執行主軸負載的線程
        public static CancellationTokenSource cancellationTokenSource; // 用來取消任務的標記
        public static ushort FFlibHndl;
        private static string FAddress; //cnc ip
        private static ushort FPort;   //cnc port
        public static short R; //cnc handle
        private static int T = 0;  // 計數器
        private static int C = 0;//平台換算成積屑等級
        private static int CurrentParam = 5; //目前排屑機所帶入的c值
        public static setting settingsPage;
        public static short FIdCode;  //cnc沖水點位idcord
        public static short EIdCode;  //cnc排屑點位idcord
        public static ushort Fdatano; //cnc沖水點位address
        public static ushort Edatano; //cnc沖水點位address
        private static int Excluder_Period;//排屑機啟動週期
        public core()
        {
            
            settingsPage = new setting();
            settingsPage.LoadConfig();
            FAddress = settingsPage.config.Cncip;
            Excluder_Period = StringToInt(settingsPage.config.Excluder_period);
            FIdCode = StringToShort(settingsPage.config.Flusher_address_handl);
            EIdCode = StringToShort(settingsPage.config.Excluder_address_handl);
            Fdatano = StringToUshort(settingsPage.config.Flusher_address);
            Edatano = StringToUshort(settingsPage.config.Excluder_address);
            FPort = StringToUshort(settingsPage.config.Cncport);
            R = Focas1.cnc_allclibhndl3(FAddress, FPort, 1, out FFlibHndl);
        }
        public static void MainProcessing(bool start)
        {
            var mainCore = new core();
            var home = new home();
            if (start)
            {
                if (mainThread == null || !mainThread.IsAlive)
                {
                    // 初始化 CancellationTokenSource
                    if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested)
                    {
                        cancellationTokenSource = new CancellationTokenSource();
                    }
                    var token = cancellationTokenSource.Token;
                    OnOff = true;
                    // 開啟背景執行的主任務
                    mainThread = new Thread(() =>
                    {
                        
                        while (OnOff && !token.IsCancellationRequested)
                        {

                            DateTime startTime = DateTime.Now;
                            mainCore.ImageProcess();  // 拍照+AI
                            int level = settingsPage.config.Flusher_level_bar_R2; // 讀取量級
                            mainCore.Flusher(level, Fdatano, Fdatano, 0, FIdCode);// 沖水                                                  
                            lock (mainCore.lockObj)
                            {
                                if (T < 5)
                                {
                                    C += level; // 換算排屑機量級 
                                    T += 1;
                                }
                                if (T == 5)
                                {
                                    CurrentParam = C;
                                    C = 0; // 清空 C
                                    T = 0; // 重置 T 以便重新計數
                                }
                            }
                            DateTime endTime = DateTime.Now;
                            TimeSpan Duratiom = (endTime - startTime);
                            int remainingTime = 100000 - (int)Duratiom.TotalMilliseconds;
                            if (remainingTime > 0)
                            { 
                                Thread.Sleep(remainingTime);
                            }
                        }
                    });
                    mainThread.Start();
                    // 排屑機執行緒
                    excluderThread = new Thread(() =>
                    {
                        
                        while (OnOff && !token.IsCancellationRequested)
                        {
                            int currentParam;//當前currentParam值，避免出現競爭
                            lock (mainCore.lockObj)
                            {
                                currentParam = CurrentParam; // 在這裡讀取靜態CurrentParam
                            }
                            mainCore.Excluder(currentParam, Fdatano, Fdatano, FIdCode); 

                        }
                    });
                    excluderThread.Start();
                    // 開啟主軸負載執行緒
                    RdspmeterThread = new Thread(() =>
                    {
                        while (OnOff && !token.IsCancellationRequested)
                        {
                            lock (mainCore.lockObj) 
                            {
                                long get = mainCore.GetData();
                                mainCore.Writejson(get);
                                Thread.Sleep(10000);  //每秒檢查一次
                            }
                           
                        }
                    });
                    RdspmeterThread.Start();
                }
            }
            else
            {
                if (mainThread != null && mainThread.IsAlive)
                {
                    // 將 OnOff 設為 false，並取消 CancellationTokenSource
                    OnOff = false;
                    cancellationTokenSource.Cancel();

                    // 設置最大等待時間，避免長時間卡住
                    mainThread.Join(1000);  // 等待線程結束最多 1 秒
                    excluderThread.Join(1000);  // 等待線程結束最多1 秒
                    RdspmeterThread.Join(1000);  // 等待線程結束最多 1秒

                    // 線程結束後設置為 null
                    mainThread = null;
                    excluderThread = null;
                    RdspmeterThread = null;
                }
            }
        }

        //seeting雜項讀取
        public void IoDataLoad()
        {
            int pmc_decimal = 0;
            settingsPage = new setting();
            settingsPage.LoadConfig();
            FAddress = settingsPage.config.Cncip;
            FPort = StringToUshort(settingsPage.config.Cncport);
            R = Focas1.cnc_allclibhndl3(FAddress, FPort, 1, out FFlibHndl);
            FIdCode = StringToShort(settingsPage.config.Flusher_address_handl);
            Fdatano = StringToUshort(settingsPage.config.Flusher_address);
            pmc_decimal = ReadByteParam(Fdatano, Fdatano, FIdCode);
            settingsPage.config.Flusher_address_decimal = pmc_decimal.ToString();
            settingsPage.config.Excluder_address_decimal = pmc_decimal.ToString();
            settingsPage.SaveConfig();
        }
        //seeting雜項寫入
        public void IoDataWrite()

        {
            settingsPage = new setting();
            settingsPage.LoadConfig();
            short length = (short)(8 + (Fdatano - Fdatano + 1));
            int pmcData = ReadByteParam(Fdatano, Fdatano, FIdCode);
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            buf.cdata[0] = Convert.ToByte(settingsPage.config.Flusher_address_decimal);
            short ret = Focas1.pmc_wrpmcrng(FFlibHndl, length, buf);
        }
        public int StringToInt(string inString)
        {
            int.TryParse(inString, out int result);
            return result;
        }

        //string轉成ushort
        public ushort StringToUshort(string inString)
        {
            ushort.TryParse(inString, out ushort result);
            return result;
        }
        //string轉乘short
        public short StringToShort(string inString)
        {
            short.TryParse(inString, out short result);
            return result;

        }
        //寫主軸負載進去json檔
        void Writejson(long m)
        {
            settingsPage.config.spindle_load = m;
            settingsPage.SaveConfig();
        }
        //影像處理+AI
        void ImageProcess()
        {
            // 設定 Python檔絕對路徑
            string pythonFilePath = @"D:\cnc_gui_s-master\method_AI\take_pic.py";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = @"C:\Program Files\Python311\python.exe";  //exe路徑
            psi.Arguments = pythonFilePath;            
            psi.CreateNoWindow = true;                 // 不顯示命令行視窗
            psi.UseShellExecute = false;               // 必須設為 false，以便不重定向輸出
            psi.RedirectStandardOutput = true;         // 不需要捕捉標準輸出
            psi.RedirectStandardError = true;          // 不需要捕捉標準錯誤
            try
            {
                using (Process process = Process.Start(psi))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
            }
        }
        //讀取主軸負載
        public long GetData()
        {
            long data = -1;
            short data_num = 1;
            Focas1.Odbspload spindleLoad = new Focas1.Odbspload();
            var ret = Focas1.cnc_rdspmeter(FFlibHndl, 0, ref data_num, spindleLoad);

            if (ret == Focas1.EW_OK)
            {
                data = spindleLoad.spload_data.spload.data;
            }
            else
            {
                Console.WriteLine("Failed to read spindle load.");
            }
            return data;
        }

        //10進制轉2進制陣列
        int[] ConvertToBinaryArray(int decimalNumber)
        {
            int[] binaryarr = new int[8];
            for (int i = 0; i < 8; i++)
            {
                binaryarr[7 - i] = (decimalNumber >> i) & 1;
            }
            return binaryarr;
        }

        //2進制轉10進制
        int ConvertBinaryArrayToDecimal(int[] binaryArray)
        {
            int decimalValue = 0;
            int length = binaryArray.Length;
            for (int i = 0; i < length; i++)
            {
                if (binaryArray[i] != 0 && binaryArray[i] != 1)
                    throw new FormatException("錯誤的二進制數值。");
                decimalValue += binaryArray[length - 1 - i] * (int)Math.Pow(2, i);
            }
            return decimalValue;
        }


        //讀取十進制數值，之前資料型態是long
        public int ReadByteParam(ushort datano_s, ushort datano_e, short IdCode) //起始位置，結束位置，idcode
        {
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, IdCode, 0, datano_e, datano_s, length, buf);
            return buf.cdata[0];
        }
        //寫入修改好的10進制數值，要修改的時候就呼叫一次
        public void WritePmcData(ushort datano_s, ushort datano_e, int i, short IdCode) //起始位置，結束位置，i=要修改的bit，idcode
        {
            ReadByteParam(datano_s, datano_e, IdCode);
            ushort length = (ushort)(8 + (datano_e - datano_s + 1));
            Focas1.Iodbpmc buf = new Focas1.Iodbpmc();
            short ret = Focas1.pmc_rdpmcrng(FFlibHndl, IdCode, 0, datano_e, datano_s, length, buf);
            int[] binaryArray = ConvertToBinaryArray(buf.cdata[0]); //10轉2
            if (binaryArray.Length > 0)
            {
                int machineIndex = binaryArray.Length - 1 - i; // 映射i到機器的位址
                binaryArray[machineIndex] = binaryArray[machineIndex] == 0 ? 1 : 0;
            }
            int modifiedDecimalValue = ConvertBinaryArrayToDecimal(binaryArray);//2轉10
            buf.cdata[0] = (byte)modifiedDecimalValue;
            short rt = Focas1.pmc_wrpmcrng(FFlibHndl, (short)length, buf);
        }
        //底座環沖控制
        public void Flusher(int level, ushort datano_s, ushort datano_e, int i, short IdCode)
        {
            if (level == 1)
            {

            }

            if (level == 2)
            {
                WritePmcData(datano_s, datano_e, 0, IdCode);
                Thread.Sleep(2000);
                WritePmcData(datano_s, datano_e, 0, IdCode);
            }
            if (level == 3)
            {
                WritePmcData(datano_s, datano_e, 0, IdCode);
                Thread.Sleep(3000);
                WritePmcData(datano_s, datano_e, 0, IdCode);
            }
            if (level == 4)
            {
                WritePmcData(datano_s, datano_e, 0, IdCode);
                Thread.Sleep(5000);
                WritePmcData(datano_s, datano_e, 0, IdCode);
            }
            if (level == 5)
            {
                WritePmcData(datano_s, datano_e, 0, IdCode);
                Thread.Sleep(6000);
                WritePmcData(datano_s, datano_e, 0, IdCode);
            }
        }
        //排屑機控制
        void Excluder(int c, ushort datano_s, ushort datano_e, short IdCode)
        {
            int T = 100;
            int OnTime;
            int FullTime;
            if (c == Excluder_Period)
            {
                settingsPage.config.Excluder_level_bar = 1;
                settingsPage.SaveConfig();
                FullTime = T * 1000;
                OnTime = T * 100;
                var task = Task.Delay(FullTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                Thread.Sleep(OnTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                task.Wait();
            }
            if (c > Excluder_Period && c < (Excluder_Period*2)+1)
            {
                settingsPage.config.Excluder_level_bar = 2;
                settingsPage.SaveConfig();
                FullTime = T * 1000;
                OnTime = T * 300;
                var task = Task.Delay(FullTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                Thread.Sleep(OnTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                task.Wait();
            }
            if (c > (Excluder_Period * 2)  && c < (Excluder_Period * 3) + 1)
            {
                settingsPage.config.Excluder_level_bar = 3;
                settingsPage.SaveConfig();
                FullTime = T * 1000;
                OnTime = T * 500;
                var task = Task.Delay(FullTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                Thread.Sleep(OnTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                task.Wait();
            }
            if (c > (Excluder_Period * 3)  && c < (Excluder_Period * 4) + 1)
            {
                settingsPage.config.Excluder_level_bar = 4;
                settingsPage.SaveConfig();
                FullTime = T * 1000;
                OnTime = T * 700;
                var task = Task.Delay(FullTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                Thread.Sleep(OnTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
                task.Wait();
            }
            if (c > (Excluder_Period * 4) && c < (Excluder_Period * 5) + 1)
            {
                settingsPage.config.Excluder_level_bar = 5;
                settingsPage.SaveConfig();
                FullTime = T * 1000;
                WritePmcData(datano_s, datano_e, 1, IdCode);
                Thread.Sleep(FullTime);
                WritePmcData(datano_s, datano_e, 1, IdCode);
            }
        }
    }
    public class Focas1
    {
        // Declare constants and methods from FOCAS library
        public const short EW_OK = 0;
        [DllImport("D:/cnc_gui_s-master/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_allclibhndl3(string ip, ushort port, int timeout, out ushort libhndl);

        [DllImport("D:/cnc_gui_s-master/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_freelibhndl(ushort libhndl);

        [DllImport("D:/cnc_gui_s-master/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short cnc_rdspmeter(ushort libhndl, short type, ref short data_num, [MarshalAs(UnmanagedType.LPStruct), Out] Odbspload spmeter);

        [DllImport("D:/cnc_gui_s-master/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short pmc_rdpmcrng(ushort FlibHndl, short adr_type, short data_type, ushort s_number, ushort e_number, ushort length, [MarshalAs(UnmanagedType.LPStruct), Out] Iodbpmc buf);

        [DllImport("D:/cnc_gui_s-master/cnc_gui/fwlib_fanuc/Fwlib32.dll")]
        public static extern short pmc_wrpmcrng(ushort FlibHndl, short length, [MarshalAs(UnmanagedType.LPStruct), In] Iodbpmc buf);
        [StructLayout(LayoutKind.Sequential)]

        public class Odbspload
        {
            public Odbspload_data spload_data = new Odbspload_data();
        }
        [StructLayout(LayoutKind.Sequential)]
        public class Odbspload_data
        {
            public Loadlm spload = new Loadlm();
            public Loadlm spspeed = new Loadlm();
        }
        [StructLayout(LayoutKind.Sequential)]
        public class Loadlm
        {
            public long data;       /* load meter data, motor speed */
            public short dec;        /* place of decimal point */
            public short unit;       /* unit */
            public char name;       /* spindle name */
            public char suff1;      /* subscript of spindle name 1 */
            public char suff2;      /* subscript of spindle name 2 */
            public char reserve;    /* */

        }

        [StructLayout(LayoutKind.Explicit)]
        public class Iodbpmc
        {
            [FieldOffset(0)]
            public short type_a;   /* Kind of PMC address */
            [FieldOffset(2)]
            public short type_d;   /* Type of the PMC data */
            [FieldOffset(4)]
            public ushort datano_s; /* Start PMC address number */
            [FieldOffset(6)]
            public ushort datano_e;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            [FieldOffset(8)]
            public byte[] cdata;
        }
    }
}