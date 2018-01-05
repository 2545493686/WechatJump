using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WechatJump
{
    public partial class Form2 : Form
    {
        static int length = 0;
        static Point lastChess = new Point();
        static Point lastRect = new Point();

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            string s = Text;
            Text = Adb.GetPhone();
            if (Text == "")
            {
                MessageBox.Show("请用数据线连接手机，并打开开发者选项中的ADB调试！", "未检测到手机连接！");
                Dispose();
            }
            else
            {
                Adb.ScreencapNow();
                pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //刷新连接设备，并刷新页面
            string s = Text;
            Text = Adb.GetPhone();
            if (Text == "")
            {
                Text = "未连接";
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
            else
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button4.Enabled = true;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            button1.Enabled = false;
            Adb.ScreencapNow();
            pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
            button1.Enabled = true;


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //初始化
            //if (Adb.IsWeChatApp())
            if (true)
            {
                #region 禁用UI
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                #endregion

                //截图并读取
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
                Adb.ScreencapNow();
                Bitmap bitmap = new Bitmap(@"E:\adb\1.png");

                #region 取棋子坐标
                Point chess = new Point();
                //棋子颜色 Color.FromArgb(55, 52, 92))
                for (int y = 1000; y < 1250; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        if (bitmap.GetPixel(x,y) == Color.FromArgb(57, 58, 102))
                        {
                            chess.X = x;
                            chess.Y = y;
                            break;
                        }
                    }
                    if (chess != new Point())
                    {
                        break;
                    }
                }
                if (chess == new Point())
                {
                    MessageBox.Show("找不到棋子！初始化失败！");
                    bitmap.Dispose();
                    return;
                }
                #endregion

                Point rectVertex = new Point();
                Point rectEnd = new Point();

                #region 取物体顶点坐标
                for (int y = 650; y < 1050; y++)
                {
                    for (int x = 1; x < bitmap.Width; x++)
                    {
                        //Console.WriteLine(bitmap.GetPixel(x, y).ToArgb());
                        //bitmap.SetPixel(x, y, Color.Red);
                        bool isColorAbout;
                        try
                        {
                            isColorAbout = !ColorAbout(bitmap.GetPixel(x - 1, y), bitmap.GetPixel(x, y));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("颜色容差只能为整数");
                            return;
                        }
                        if (bitmap.GetPixel(x, y) == Color.FromArgb(255, 238, 97))
                        {
                            //MessageBox.Show("黄色！手动一波！");
                            rectVertex.X = x;
                            rectVertex.Y = y;
                            rectEnd.X = rectVertex.X;
                            rectEnd.Y = rectVertex.Y + 75;
                            break;
                        }
                        else if ((x < chess.X - 75 || x > chess.X + 75) && isColorAbout)
                        {
                            rectVertex.X = x;
                            rectVertex.Y = y;
                            break;
                        }
                    }
                    if (rectVertex != new Point())
                    {
                        break;
                    }
                }
                if (rectVertex == new Point())
                {
                    MessageBox.Show("未知的物体！初始化失败！");
                    bitmap.Dispose();
                    return;
                }
                #endregion

                #region 取物体尾部坐标
                Color rectColor = bitmap.GetPixel(rectVertex.X,rectVertex.Y+1);
                if (rectEnd == new Point())
                {
                    for (int y = rectVertex.Y; y < 1200; y++)
                    {
                        //bitmap.SetPixel(rectVertex.X, y, Color.Red);
                        bool isColorAbout;
                        try
                        {
                            isColorAbout = ColorAbout(rectColor, bitmap.GetPixel(rectVertex.X, y));
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("颜色容差只能为整数");
                            return;
                        }
                        if (isColorAbout)
                        {
                            rectEnd.X = rectVertex.X;
                            rectEnd.Y = y;
                        }
                    }
                }
                #endregion

                #region 取物体中点
                Point rectCenter = new Point();
                rectCenter.X = (rectVertex.X + rectEnd.X) / 2;
                rectCenter.Y = (rectVertex.Y + rectEnd.Y) / 2;
                #endregion

                length = (int)(Math.Sqrt(Math.Pow(rectCenter.X - chess.X, 2) + Math.Pow(rectCenter.Y - chess.Y,2)));
                lastChess = chess;
                lastRect = rectCenter;

                Console.WriteLine(string.Format("length:{0} ; start:[{1},{2}] ; to[{3},{4}]",length,chess.X,chess.Y,rectCenter.X,rectCenter.Y));

                #region 画十字坐标
                for (int i = 0; i < 130; i++)
                {
                    bitmap.SetPixel(chess.X + i - 65, chess.Y, Color.Red);
                    bitmap.SetPixel(chess.X + i - 65, chess.Y - 1, Color.Red);
                    bitmap.SetPixel(chess.X + i - 65, chess.Y + 1, Color.Red);
                    bitmap.SetPixel(chess.X, chess.Y + i - 65, Color.Red);
                    bitmap.SetPixel(chess.X - 1, chess.Y + i - 65, Color.Red);
                    bitmap.SetPixel(chess.X + 1, chess.Y + i - 65, Color.Red);

                    bitmap.SetPixel(rectVertex.X + i - 65, rectVertex.Y, Color.Red);
                    bitmap.SetPixel(rectVertex.X + i - 65, rectVertex.Y - 1, Color.Red);
                    bitmap.SetPixel(rectVertex.X + i - 65, rectVertex.Y + 1, Color.Red);
                    bitmap.SetPixel(rectVertex.X, rectVertex.Y + i - 65, Color.Red);
                    bitmap.SetPixel(rectVertex.X - 1, rectVertex.Y + i - 65, Color.Red);
                    bitmap.SetPixel(rectVertex.X + 1, rectVertex.Y + i - 65, Color.Red);

                    bitmap.SetPixel(rectEnd.X + i - 65, rectEnd.Y, Color.Red);
                    bitmap.SetPixel(rectEnd.X + i - 65, rectEnd.Y - 1, Color.Red);
                    bitmap.SetPixel(rectEnd.X + i - 65, rectEnd.Y + 1, Color.Red);
                    bitmap.SetPixel(rectEnd.X, rectEnd.Y + i - 65, Color.Red);
                    bitmap.SetPixel(rectEnd.X - 1, rectEnd.Y + i - 65, Color.Red);
                    bitmap.SetPixel(rectEnd.X + 1, rectEnd.Y + i - 65, Color.Red);

                }
                #endregion

                pictureBox1.Image = null;
                pictureBox1.Image = (Image)bitmap.Clone();
                bitmap.Dispose();

                #region 恢复UI
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                #endregion
            }
            else
            {
                MessageBox.Show("请打开微信跳一跳再进行初始化！");
            }
        }

        /// <summary>
        /// 判断颜色是否相近
        /// </summary>
        /// <param name="color0"></param>
        /// <param name="color1"></param>
        /// <returns></returns>
        bool ColorAbout(Color color0, Color color1)
        {//70000
            int i = new int();
            i = Convert.ToInt32(textBox4.Text);
            return !(color0.ToArgb() - color1.ToArgb() > i || color0.ToArgb() - color1.ToArgb() < -i);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

            double i = new double();
            try
            {
                i = Convert.ToDouble(textBox1.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("微调系数必须为数值！");
                return;
            }
            Adb.Touch(lastChess.X,lastChess.Y,lastRect.X,lastRect.Y,(int)(length*i));
            Thread.Sleep(1000);
            pictureBox1.Image.Dispose();
            Adb.ScreencapNow();
            pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");



        }

        private void button4_Click(object sender, EventArgs e)
        {
            #region 禁用UI
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = null;
            }
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            timer1.Enabled = false;
            #endregion

            #region 取循环次数
            int count = new int();
            try
            {
                count = Convert.ToInt32(textBox3.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("循环次数只能为整数");
                return;
            }
            #endregion

            for (int ii = 0; ii < count; ii++)
            {
                //if (Adb.IsWeChatApp())
                if (true)
                {
                    //截图
                    Adb.ScreencapNow();
                    Bitmap bitmap = new Bitmap(@"E:\adb\1.png");

                    #region 取棋子坐标
                    Point chess = new Point();
                    //棋子颜色 Color.FromArgb(55, 52, 92))
                    for (int y = 1000; y < 1250; y++)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            if (bitmap.GetPixel(x, y) == Color.FromArgb(57, 58, 102))
                            {
                                chess.X = x;
                                chess.Y = y;
                                break;
                            }
                        }
                        if (chess != new Point())
                        {
                            break;
                        }
                    }
                    if (chess == new Point())
                    {
                        Adb.ScreencapNow();
                        pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
                        button4.Text = "JUMP AUTO";
                        timer1.Enabled = true;
                        MessageBox.Show("找不到棋子！初始化失败！");
                        bitmap.Dispose();
                        return;
                    }
                    #endregion

                    Point rectVertex = new Point(); //顶点坐标
                    Point rectEnd = new Point(); //底部坐标

                    #region 取物体顶点坐标

                    for (int y = 650; y < 1050; y++)
                    {
                        for (int x = 1; x < bitmap.Width; x++)
                        {
                            //Console.WriteLine(bitmap.GetPixel(x, y).ToArgb());
                            //bitmap.SetPixel(x, y, Color.Red);
                            bool isColorAbout;
                            try
                            {
                                isColorAbout = !ColorAbout(bitmap.GetPixel(x - 1, y), bitmap.GetPixel(x, y));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("颜色容差只能为整数");
                                return;
                            }
                            if (bitmap.GetPixel(x, y) == Color.FromArgb(255, 238, 97))
                            {
                                //MessageBox.Show("黄色！手动一波！");
                                
                                rectVertex.X = x;
                                rectVertex.Y = y;
                                rectEnd.X = rectVertex.X;
                                rectEnd.Y = rectVertex.Y + 75;
                                break;
                            }
                            else if ((x < chess.X - 75 || x > chess.X + 75) && isColorAbout)
                            {
                                rectVertex.X = x;
                                rectVertex.Y = y;
                                break;
                            }
                        }
                        if (rectVertex != new Point())
                        {
                            break;
                        }
                    }
                    if (rectVertex == new Point())
                    {
                        Adb.ScreencapNow();
                        pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
                        button4.Text = "JUMP AUTO";
                        timer1.Enabled = true;
                        MessageBox.Show("未知的物体！初始化失败！");
                        bitmap.Dispose();
                        return;
                    }
                    #endregion

                    #region 取物体尾部坐标
                    Color rectColor = bitmap.GetPixel(rectVertex.X, rectVertex.Y + 1);
                    if (rectEnd == new Point())
                    {
                        for (int y = rectVertex.Y; y < 1200; y++)
                        {
                            //bitmap.SetPixel(rectVertex.X, y, Color.Red);
                            bool isColorAbout;
                            try
                            {
                                isColorAbout = ColorAbout(rectColor, bitmap.GetPixel(rectVertex.X, y));
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("颜色容差只能为整数");
                                return;
                            }
                            if (isColorAbout)
                            {
                                rectEnd.X = rectVertex.X;
                                rectEnd.Y = y;
                            }
                        }

                    }
                    #endregion

                    #region 计算距离
                    Point rectCenter = new Point();
                    rectCenter.X = (rectVertex.X + rectEnd.X) / 2;
                    rectCenter.Y = (rectVertex.Y + rectEnd.Y) / 2;

                    length = (int)(Math.Sqrt(Math.Pow(rectCenter.X - chess.X, 2) + Math.Pow(rectCenter.Y - chess.Y, 2)));
                    lastChess = chess;
                    lastRect = rectCenter;
                    #endregion

                    Console.WriteLine(string.Format("length:{0} ; start:[{1},{2}] ; to:[{3},{4}] ; ii = {5}", length, chess.X, chess.Y, rectCenter.X, rectCenter.Y,ii));
                    bitmap.Dispose();

                }
                else
                {
                    MessageBox.Show("请打开微信跳一跳再进行初始化！");
                }
                #region 跳跃
                //1.39
                double i = new int();
                try
                {
                    i = Convert.ToDouble(textBox1.Text);
                }
                catch (Exception)
                {
                    Adb.ScreencapNow();
                    pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
                    button4.Text = "JUMP AUTO";
                    timer1.Enabled = true;
                    MessageBox.Show("微调系数必须为数值！");
                    return;
                }
                Adb.Touch(lastChess.X,lastChess.Y,lastRect.X,lastRect.Y,(int)(length*i));//1.39
                #endregion
                try
                {
                    i = Convert.ToDouble(textBox2.Text);
                }
                catch (Exception)
                {
                    Adb.ScreencapNow();
                    pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
                    button4.Text = "JUMP AUTO";
                    timer1.Enabled = true;
                    MessageBox.Show("延迟时间必须为数值！");
                    return;
                }

                button4.Text = (ii+1).ToString();
                Thread t = new Thread(o => Thread.Sleep((int)i));
                t.Start(this);
                while (t.IsAlive)
                {
                    Application.DoEvents();
                }

                
            }
            #region 复位UI
            Adb.ScreencapNow();
            pictureBox1.Image = Image.FromFile(@"E:\adb\1.png");
            button4.Text = "JUMP AUTO";
            timer1.Enabled = true;

            #endregion
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.微调系数 = textBox1.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.每轮延迟 = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.循环次数 = textBox3.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.颜色容差 = textBox4.Text;
            Properties.Settings.Default.Save();
        }

        private void Form2_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            MessageBox.Show(Resource1.Help, Resource1.Version);
        }
    }

    static class Adb
    {
        static string Cmd(string str,int sleep)
        {
            Process p;
            p = new Process();
            p.StartInfo.FileName = @"E:\adb\adb.exe";
            p.StartInfo.Arguments = str;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Thread.Sleep(sleep);
            return s;
        }
        static string Cmd(string str)
        {
            Process p;
            p = new Process();
            p.StartInfo.FileName = @"E:\adb\adb.exe";
            p.StartInfo.Arguments = str;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            string s = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return s;
        }

        /// <summary>
        /// 触摸屏幕，从点0长按到点1持续time秒
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="time"></param>
        static public void Touch(int x0, int y0, int x1, int y1, int time)
        {
            Cmd(String.Format("shell input swipe {0} {1} {2} {3} {4}", x0, y0, x1, y1, time));
        }

        /// <summary>
        /// 上传手机截图
        /// </summary>
        static public void ScreencapNow()
        {
            Cmd(@"shell screencap -p /sdcard/1.png");
            Cmd(@"pull /sdcard/1.png E:\adb");
        }

        /// <summary>
        /// 上传手机截图
        /// </summary>
        static public void Screencap()
        {
            Cmd(@"shell screencap -p /sdcard/1.png",500);
            Cmd(@"pull /sdcard/1.png E:\adb",500);
        }

        /// <summary>
        /// 获取手机型号
        /// </summary>
        /// <returns></returns>
        static public string GetPhone()
        {
            //adb shell getprop ro.product.model 查询手机型号
            return Cmd("shell getprop ro.product.model",0);
        }


        static public bool IsWeChatApp()
        {
            ///mCurrentFocus = Window{ c514e8f u0 com.tencent.mm / com.tencent.mm.plugin.appbrand.ui.AppBrandUI}
            return Regex.IsMatch(Cmd("shell dumpsys window"), "mCurrentFocus=Window{.+ u0 com.tencent.mm/com.tencent.mm.plugin.appbrand.ui.AppBrandUI1}");
        }
    }

}
