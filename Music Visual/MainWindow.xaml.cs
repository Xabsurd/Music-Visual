using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using NAudio.Wave;
using NAudio.Dsp;
using System.Threading;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
//using ReactiveUI;

namespace Music_Visual
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //DrawingContext context;
        internal SettingModel setting = new SettingModel();
        WasapiLoopbackCapture capture;
        int num = 0;
        int lastnum = 0;
        long bassD = DateTime.Now.Ticks;
        Thread? drawThread;

        private IntPtr workerw;
        private int SCREEN_WIDTH = Convert.ToInt32(SystemParameters.PrimaryScreenWidth);
        private int SCREEN_HEIGHT = Convert.ToInt32(SystemParameters.PrimaryScreenHeight);
        WriteableBitmap wBitmap;
        public MainWindow()
        {
            InitializeComponent();
            initSetting();

            this.Loaded += MainWindow_Loaded;
            this.WindowState = WindowState.Maximized;
            this.Closing += MainWindow_Closing; ;
            //new Window1().Show();


        }
        private void initSetting()
        {
            byte[] line = Properties.Settings.Default.LineColor.Split(",").Select(a => Convert.ToByte(a)).ToArray();
            byte[] Lline = Properties.Settings.Default.leftColor.Split(",").Select(a => Convert.ToByte(a)).ToArray();
            byte[] Rline = Properties.Settings.Default.rightColor.Split(",").Select(a => Convert.ToByte(a)).ToArray();
            int[] margin = Properties.Settings.Default.margin.Split(",").Select(a => Convert.ToInt32(a)).ToArray();
            setting.pen = new Pen(new SolidBrush(Color.FromArgb(line[0], line[1], line[2], line[3])), Properties.Settings.Default.LineWidth);
            setting.Target = Properties.Settings.Default.Target;
            setting.BarWidth = Properties.Settings.Default.BarWidth;
            setting.Take = Properties.Settings.Default.Take;
            setting.Skip = Properties.Settings.Default.Skip;
            setting.visualType = Properties.Settings.Default.visualType;
            setting.leftPen = new Pen(new SolidBrush(Color.FromArgb(Lline[0], Lline[1], Lline[2], Lline[3])), Properties.Settings.Default.LineWidth);
            setting.rightPen = new Pen(new SolidBrush(Color.FromArgb(Rline[0], Rline[1], Rline[2], Rline[3])), Properties.Settings.Default.LineWidth);
            setting.isLine = Properties.Settings.Default.isLine;
            setting.amplitude = Properties.Settings.Default.amplitude;
            setting.smooth = Properties.Settings.Default.smooth;
            setting.margin= new Thickness(margin[0], margin[1], margin[2], margin[3]);
            //this.img.Margin = new Thickness(margin[0], margin[1], margin[2], margin[3]);
        }
        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.tbi.Dispose();
            //this.grid.Children.Remove(this.tbi);
            Process.GetCurrentProcess().Kill();
            recovery();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                base.Topmost = false;
                base.WindowState = WindowState.Normal;
                var handle = new WindowInteropHelper(this).Handle;

                if (App.HWND != 0)
                {
                    //如果启动参数中有句柄则直接初始化
                    W32.SetParent(handle, (IntPtr)App.HWND);
                    W32.SetWindowLong(handle, W32.WindowLongFlags.GWL_STYLE, 1073741824);
                    W32.PostMessage((IntPtr)App.HWND, 1028U, IntPtr.Zero, handle);
                    W32.RECT rect = default(W32.RECT);
                    W32.GetWindowRect(handle, ref rect);
                    SCREEN_WIDTH = rect.Right - rect.Left;
                    SCREEN_HEIGHT = rect.Bottom - rect.Top;
                    wBitmap = new WriteableBitmap(SCREEN_WIDTH, SCREEN_HEIGHT, 72, 72, System.Windows.Media.PixelFormats.Bgra32, null);
                    this.img.Source = wBitmap;
                    W32.SetWindowPos(handle, IntPtr.Zero, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, W32.SetWindowPosFlags.NoReposition);
                }
                else
                {
                    initWPE(handle);
                    //this.bt_close.Visibility = System.Windows.Visibility.Visible;
                }
                capture = new WasapiLoopbackCapture();
                capture.DataAvailable += Capture_DataAvailable;
                capture.StartRecording();
            }
            catch (Exception)
            {

            }


        }
        /// <summary>
        /// 恢复壁纸
        /// </summary>
        public void recovery()
        {
            //this.tb.RemoveHandler
            W32.SetWindowPos(workerw, IntPtr.Zero, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, W32.SetWindowPosFlags.HideWindow);
            W32.SetWindowPos(workerw, IntPtr.Zero, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, W32.SetWindowPosFlags.ShowWindow);
        }
        /// <summary>
        /// 初始化壁纸引擎
        /// </summary>
        public void initWPE(IntPtr handle)
        {
            // 查找Progman窗口句柄
            IntPtr progman = W32.FindWindow("Progman", "Program Manager");

            IntPtr result = IntPtr.Zero;

            // 发送消息（0x052C）给Progman窗口
            // 此消息指示Progman窗口在桌面图标窗口下面创建WorkerW窗口
            // 如果已经创建则没有任何变化
            W32.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, W32.SendMessageTimeoutFlags.SMTO_NORMAL, 0x3e8, out result);
            workerw = IntPtr.Zero;
            List<IntPtr> l = new List<IntPtr>();
            // 枚举所有的窗口，直到找到子窗口包含一个SHELLDLL_DefView的
            // 如果发现该窗口，我们直接把该窗口的句柄赋值给workerw变量
            W32.EnumWindows(new W32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = W32.FindWindowEx(tophandle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
                if (p != IntPtr.Zero)
                {
                    // 获取当前的WorkerW窗口
                    workerw = W32.FindWindowEx(IntPtr.Zero, tophandle, "WorkerW", IntPtr.Zero);
                    l.Add(workerw);
                }
                return true;
            }), IntPtr.Zero);
            // 设置当前窗口父窗句柄为workerw
            W32.SetParent(new WindowInteropHelper(this).Handle, workerw);
            W32.SetWindowLong(handle, W32.WindowLongFlags.GWL_STYLE, 1073741824);
            W32.PostMessage(workerw, 1028U, IntPtr.Zero, handle);
            W32.RECT rect = default(W32.RECT);
            W32.GetWindowRect(workerw, ref rect);
            SCREEN_WIDTH = rect.Right - rect.Left;
            SCREEN_HEIGHT = rect.Bottom - rect.Top;
            wBitmap = new WriteableBitmap(SCREEN_WIDTH, SCREEN_HEIGHT, 72, 72, System.Windows.Media.PixelFormats.Bgra32, null);
            this.img.Source = wBitmap;
            W32.SetWindowPos(handle, IntPtr.Zero, 0, 0, SCREEN_WIDTH, SCREEN_HEIGHT, W32.SetWindowPosFlags.NoReposition);

        }
        public void UpdateDraw(object? o)
        {
            Stopwatch sw = new Stopwatch();

            WaveInEventArgs e = (WaveInEventArgs)o;
            if (o == null)
            {
                return;
            }
            float[] final;


            float[] allSamples = Enumerable      // 提取数据中的采样
                   .Range(0, e.BytesRecorded / 4)   // 除以四是因为, 缓冲区内每 4 个字节构成一个浮点数, 一个浮点数是一个采样
                   .Select(i => BitConverter.ToSingle(e.Buffer, i * 4))  // 转换为 float
                   .ToArray();
            if (allSamples.Length < 1)
            {
                return;
            }


            // 设定我们已经将刚刚的采样保存到了变量 AllSamples 中, 类型为 float[]
            int channelCount = capture.WaveFormat.Channels;   // WasapiLoopbackCapture 的 WaveFormat 指定了当前声音的波形格式, 其中包含就通道数
            float[][] channelSamples = Enumerable
                .Range(0, channelCount)
                .Select(channel => Enumerable
                    .Range(0, allSamples.Length / channelCount)
                    .Select(i => allSamples[channel + i * channelCount])
                    .ToArray())
                .ToArray();
            // 设定我们已经将分开了的采样保存到了变量 ChannelSamples 中, 类型为 float[][]
            // 例如通道数为2, 那么左声道的采样为 ChannelSamples[0], 右声道为 ChannelSamples[1]
            float[] averageSamples = Enumerable
                .Range(0, allSamples.Length / channelCount)
                .Select(index => Enumerable
                    .Range(0, channelCount)
                    .Select(channel => channelSamples[channel][index])
                    .Average())
                    .ToArray();
            // 我们将对 AverageSamples 进行傅里叶变换, 得到一个复数数组



            //this.Dispatcher.Invoke(new Action(() =>
            //{


            if (setting.visualType == "频域")
            {
                final = getPY(averageSamples);
            }
            else
            {
                final = averageSamples;
            }
            if (final.Length < 1)
            {
                return;
            }
            final = final.Skip(this.setting.Skip).Take(this.setting.Take).ToArray();
            Draw(final);
            ////final.Add(0);
            //StreamGeometry streamGeometry = new StreamGeometry();
            //streamGeometry.FillRule = FillRule.EvenOdd;
            //int bassHeight = SCREEN_HEIGHT - 50;
            //if (setting.visualType == "时域")
            //{
            //    bassHeight = SCREEN_HEIGHT / 2;
            //}
            //sw.Start();
            //List<System.Drawing.PointF> list = new List<System.Drawing.PointF>();
            //using (StreamGeometryContext ctx = streamGeometry.Open())
            //{
            //    ctx.BeginFigure(new Point(0, bassHeight), false, false);
            //    int iNum = (int)((double)final.Length / setting.Target);
            //    int amplitude = setting.amplitude * (setting.visualType == "时域" ? setting.amplitude * 3 : 1);
            //    double itemW = (SCREEN_WIDTH / (double)final.Length);
            //    if (iNum < 1)
            //    {
            //        iNum = 1;
            //    }


            //    for (int i = 0; i < final.Length; i += iNum)
            //    {
            //        Point point = new Point((i + iNum) * itemW, bassHeight - final[i]* amplitude);
            //        list.Add(new System.Drawing.PointF((float)point.X, (float)point.Y));
            //        if (setting.isLine)
            //        {
            //            ctx.LineTo(point, true, false);
            //        }
            //        else
            //        {
            //            ctx.LineTo(new Point(point.X, bassHeight), false, false);
            //            ctx.LineTo(point, true, false);
            //        }
            //    }
            //    streamGeometry.Freeze();
            //}
            //sw.Stop();
            //this.Dispatcher.Invoke(new Action(() =>
            //{

            //    Draw(streamGeometry, list.ToArray());
            //    this.tb1.Text = (sw.Elapsed.Ticks / 1000.0) + "ms";
            //}));

        }
        private void Draw1(float[] final)
        {
        }
        private void Draw(float[] final)
        {
            if (final.Length < 1)
            {
                return;
            }
            int bassHeight = SCREEN_HEIGHT - (int)setting.margin.Bottom;
            if (setting.visualType == "时域")
            {
                bassHeight = SCREEN_HEIGHT / 2;
            }
            int iNum = (int)((double)final.Length / setting.Target);
            int amplitude = setting.amplitude * (setting.visualType == "时域" ? setting.amplitude * 20 : 1);
            double itemW = ((SCREEN_WIDTH-setting.margin.Left-setting.margin.Right) / ((double)final.Length + 1.0));
            if (iNum < 1)
            {
                iNum = 1;
            }
            this.Dispatcher.Invoke(new Action(() =>
            {
                wBitmap.Lock();
                Bitmap backBitmap = new Bitmap(SCREEN_WIDTH, SCREEN_HEIGHT, wBitmap.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, wBitmap.BackBuffer);

                Graphics graphics = Graphics.FromImage(backBitmap);
                //graphics.Clear(Color.FromArgb(255, 255, 255, 255));
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                //graphics.DrawLine(new Pen(Color.FromArgb(255, 255, 255,255), 10),new PointF(0,0), new PointF(SCREEN_WIDTH, SCREEN_HEIGHT));
                //graphics.DrawLine(setting.pen, new PointF(0, bassHeight), new PointF((float)(iNum * itemW), bassHeight - final[iNum] * amplitude));\
                if (setting.isLine)
                {
                    List<PointF> points = new List<PointF>();
                    points.Add(new PointF((int)setting.margin.Left, bassHeight));
                    for (int i = 0; i < final.Length - 1; i += iNum)
                    {
                        points.Add(new PointF((float)((i + iNum) * itemW)+ (int)setting.margin.Left, bassHeight - final[i] * amplitude));
                        //graphics.DrawLine(setting.pen, new PointF((float)((i + iNum) * itemW), bassHeight - final[i] * amplitude), new PointF((float)((i + iNum * 2) * itemW), bassHeight - final[i + 1] * amplitude));
                    }
                    points.Add(new PointF((float)((final.Length + iNum) * itemW) + (int)setting.margin.Left, bassHeight));
                    if (setting.smooth)
                    {
                        graphics.DrawCurve(setting.pen, points.ToArray(), 0.5f);
                    }
                    else
                    {
                        graphics.DrawLines(setting.pen, points.ToArray());
                    }
                }
                else
                {
                    for (int i = 0; i < final.Length - 1; i += iNum)
                    {
                        graphics.DrawLine(setting.pen, new PointF((float)((i + iNum) * itemW)+(int)setting.margin.Left, bassHeight), new PointF((float)((i + iNum) * itemW)+(int)setting.margin.Left, bassHeight - final[i] * amplitude));
                    }
                }

                graphics.Flush();
                graphics.Dispose();
                graphics = null;

                backBitmap.Dispose();
                backBitmap = null;

                wBitmap.AddDirtyRect(new Int32Rect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT));

                wBitmap.Unlock();
            }));
        }
        private float[] getPY(float[] array)
        {
            // 因为对于快速傅里叶变换算法, 需要数据长度为 2 的 n 次方, 这里进行
            int log = (int)Math.Ceiling(Math.Log(array.Length, 2));   // 取对数并向上取整
            int newLen = (int)Math.Pow(2, log);                             // 计算新长度
            float[] filledSamples = new float[newLen];
            Array.Copy(array, filledSamples, array.Length);   // 拷贝到新数组
            Complex[] complexSrc = filledSamples
                .Select(v => new Complex() { X = v })        // 将采样转换为复数
                .ToArray();
            FastFourierTransform.FFT(false, log, complexSrc);   // 进行傅里叶变换

            // 变换之后, complexSrc 则已经被处理过, 其中存储了频域信息
            // NAudio 的傅里叶变换结果中, 似乎不存在直流分量(这使我们的处理更加方便了), 但它也是有共轭什么的(也就是数据左右对称, 只有一半是有用的)
            // 仍然使用刚刚的 complexSrc 作为变换结果, 它的类型是 Complex[]

            Complex[] halfData = complexSrc
                .Take(complexSrc.Length / 2)
                .ToArray();    // 一半的数据
            float[] dftData = halfData
                .Select(v => (float)Math.Sqrt(v.X * v.X + v.Y * v.Y))  // 取复数的模
                .ToArray();    // 将复数结果转换为我们所需要的频率幅度
            return dftData;
        }
        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {

            if (drawThread == null)
            {
                drawThread = new Thread(UpdateDraw);
                drawThread.Start(e);
            }
            else
            {
                if (drawThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    drawThread = new Thread(UpdateDraw);
                    drawThread.Start(e);
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }


        private void MenuItem_Click_Setting(object sender, RoutedEventArgs e)
        {
            SettingWindow s = new SettingWindow(this);
            s.Show();
        }

        private void MenuItem_Click_Close(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
