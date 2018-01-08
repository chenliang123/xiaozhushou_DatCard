﻿using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using RueHelper.util;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace RueHelper
{
    public partial class FormTwoMinute : Form
    {
        [DllImport("user32.dll", EntryPoint = "AnimateWindow")]
        private static extern bool AnimateWindow(IntPtr handle, int ms, int flags);
        public const int AW_HOR_POSITIVE = 0X1;//左->右
        public const int AW_HOR_NEGATIVE = 0X2;//右->左
        public const int AW_VER_POSITIVE = 0X4;//上->下
        public const int AW_VER_NEGATIVE = 0X8;//下->上
        public const int AW_CENTER = 0X10;
        public const int AW_HIDE = 0X10000;
        public const int AW_ACTIVATE = 0X20000;//逐渐显示
        public const int AW_SLIDE = 0X40000;
        public const int AW_BLEND = 0X80000;
        public const int AW_L2R = 0X80001;
        public const int AW_R2L = 0X80002;
        public const int AW_U2D = 0X80004;
        public const int AW_D2U = 0X80008;

        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        AutoResetEvent are = new AutoResetEvent(false);
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        ArrayList al;
        ArrayList alText;
        private ArrayList clickstate;
        public ArrayList _rightList;
        public Form11 f11;
        System.Media.SoundPlayer sp = new SoundPlayer(RueHelper.Properties.Resources.click1);   
        public string _xitiId = "";
        
        public System.Timers.Timer t;
        public System.Timers.Timer t1;
        int inTimer = 0;
        public DateTime tm_create = DateTime.Now;

        private Hashtable m_hashtable = new Hashtable(); //保存学生按键数据
        private Color CircleBackgroundColor = System.Drawing.Color.FromArgb(254, 232, 211);
        private Graphics gColA;
        private Graphics gColB;
        private Graphics gColC;
        private Graphics gColD;

        Label lbStu = new Label();//创建一个label
        Label lbTea = new Label();//创建一个label
        private int countA0;
        private int countB0;
        private int countC0;
        private int countD0;
        private int countA;
        private int countB;
        private int countC;
        private int countD;
        private int nCol = 0;
        public delegate void InvokeVoteState(string context, PictureBox pic, PictureBox text, int i);
        public delegate void InvokeColumnState(string context, PictureBox pic);
        private List<VoteItem> votelist = new List<VoteItem>();
        public static string RESULT = "0,0,0,0";
        public FormTwoMinute(string options)
        {
            RESULT = "0,0,0,0";  
            InitializeComponent();

            Log.Info("FormVote.create,  options=" + options);
            tm_create = DateTime.Now;
            al = new ArrayList();
            alText = new ArrayList();
            clickstate = new ArrayList();

            //No xiti.id
            _xitiId = Global.getSchoolID() + "-" + Global.getClassID() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            SetPanel(options); //设置面板

            this.Text = "精彩两分钟，评价";
            
            this.Height = screenHeight;
            this.Width = screenWidth;

            //StartPosition = FormStartPosition.Manual;
            //SetDesktopLocation(0, screenHeight - this.Height);

            this.TopMost = true;
#if DEBUG
            this.TopMost = false;//PPTPractise
#endif
            this.Hide();
            this.Show();
            this.BringToFront();

            t = new System.Timers.Timer(200);
            t.Elapsed += new System.Timers.ElapsedEventHandler(Theout);
            t.Enabled = true;
            t.AutoReset = true;

            //for (int i = 1; i <= 40; i++)
            //{
            //    statisticABCD(i, "A");
            //}
            //VoteColumnEvent("A", pictureBox_A);
        }
        public void upInfo(string optTea,string optStu)
        {
            {

                lbStu.Text = optTea;
                lbStu.Parent = pictureBox_Stu;//指定父级
                lbStu.Size = pictureBox_Stu.Size;
                lbStu.BackColor = Color.Transparent;
                lbStu.ForeColor = System.Drawing.Color.Transparent;
                lbStu.Font = new System.Drawing.Font("隶书", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                lbStu.Location = new Point(100, 0);//在pictureBox1中的坐标
                lbStu.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }
            {
                lbTea.Text = optStu;
                lbTea.Parent = pictureBox_Tea;//指定父级
                lbTea.Size = pictureBox_Tea.Size;
                lbTea.BackColor = Color.Transparent;
                lbTea.ForeColor = System.Drawing.Color.Transparent;
                lbTea.Font = new System.Drawing.Font("隶书", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                lbTea.Location = new Point(100, 0);//在pictureBox1中的坐标
                lbTea.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }
        }
        private void SetPanel(string options = "1,2,4,3")
        {
            #region 选项
                label_optionA.Text = "非常优秀";
                label_optionB.Text = "表现不错";
                label_optionC.Text = "继续努力";
                label_optionD.Text = "再接再厉";
                label_optionA.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                label_optionB.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                label_optionC.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                label_optionD.Font = new System.Drawing.Font("微软雅黑", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                nCol = 4;
            #endregion

            #region 标题栏
            pictureBox_Title.Top = 50;
            pictureBox_Title.Left = (screenWidth - pictureBox_Title.Width) / 2;
            Label lb = new Label();//创建一个label
            lb.Text = "精彩两分钟";
            lb.Parent = pictureBox_Title;//指定父级
            lb.Size = pictureBox_Title.Size;
            lb.BackColor = Color.Transparent;
            lb.ForeColor = System.Drawing.Color.White;
            lb.Font = new System.Drawing.Font("微软雅黑", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            lb.Location = new Point(0, 0);//在pictureBox1中的坐标
            lb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            #endregion

            //"✔", "✘"
            #region 计数行
            double picCountRatio = 0.20;
            pictureBox_CountA.Visible = false;
            pictureBox_CountB.Visible = false;
            pictureBox_CountC.Visible = false;
            pictureBox_CountD.Visible = false;
            label_countA.Text = "0";
            label_countB.Text = "0";
            label_countC.Text = "0";
            label_countD.Text = "0";

            int txtLeft = 0;
            int txtTop = -5;
            label_countA.Parent = pictureBox_CountA;//指定父级
            label_countA.Font = new System.Drawing.Font("黑体", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            label_countA.Location = new System.Drawing.Point(txtLeft, txtTop);

            label_countB.Parent = pictureBox_CountB;//指定父级
            label_countB.Font = new System.Drawing.Font("黑体", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            label_countB.Location = new System.Drawing.Point(txtLeft, txtTop);

            label_countC.Parent = pictureBox_CountC;//指定父级
            label_countC.Font = new System.Drawing.Font("黑体", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            label_countC.Location = new System.Drawing.Point(txtLeft, txtTop);

            label_countD.Parent = pictureBox_CountD;//指定父级
            label_countD.Font = new System.Drawing.Font("黑体", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            label_countD.Location = new System.Drawing.Point(txtLeft, txtTop);

            for (int i = 0; i < nCol; i++ )
            {
                if(i==0){
                    pictureBox_CountA.Left = (int)(screenWidth * 1 / 8) - pictureBox_CountA.Width / 2;
                    pictureBox_CountA.Top = (int)(screenHeight * picCountRatio);
                    pictureBox_CountA.Visible = true;
                }else if (i == 1){
                    pictureBox_CountB.Left = (int)(screenWidth * 3 /8) - pictureBox_CountA.Width / 2;
                    pictureBox_CountB.Top = (int)(screenHeight * picCountRatio);
                    pictureBox_CountB.Visible = true;
                }else if(i==2){
                    pictureBox_CountC.Left = (int)(screenWidth * 5/ 8) - pictureBox_CountA.Width / 2;
                    pictureBox_CountC.Top = (int)(screenHeight * picCountRatio);
                    pictureBox_CountC.Visible = true;
                }else{
                    pictureBox_CountD.Left = (int)(screenWidth * 7/ 8) - pictureBox_CountA.Width / 2;
                    pictureBox_CountD.Top = (int)(screenHeight * picCountRatio);
                    pictureBox_CountD.Visible = true;
                }
            }
            #endregion

            #region 柱状图行
            int colH = 350;

            pictureBox_A.Visible = true;
            pictureBox_B.Visible = true;
            pictureBox_C.Visible = true;
            pictureBox_D.Visible = true;
            label_optionA.Visible = true;
            label_optionB.Visible = true;
            label_optionC.Visible = true;
            label_optionD.Visible = true;

            gColA = pictureBox_A.CreateGraphics();
            gColB = pictureBox_B.CreateGraphics();
            gColC = pictureBox_C.CreateGraphics();
            gColD = pictureBox_D.CreateGraphics();

            pictureBox_A.Size = new System.Drawing.Size(62, colH);
            pictureBox_B.Size = new System.Drawing.Size(62, colH);
            pictureBox_C.Size = new System.Drawing.Size(62, colH);
            pictureBox_D.Size = new System.Drawing.Size(62, colH);
            double picBottomRatio = 0.8;
            pictureBox_A.Top = (int)(screenHeight * picBottomRatio) - colH;
            pictureBox_B.Top = (int)(screenHeight * picBottomRatio) - colH;
            pictureBox_C.Top = (int)(screenHeight * picBottomRatio) - colH;
            pictureBox_D.Top = (int)(screenHeight * picBottomRatio) - colH;

            double optionRatio = 0.8;
            label_optionA.Top = (int)(screenHeight * optionRatio);
            label_optionB.Top = (int)(screenHeight * optionRatio);
            label_optionC.Top = (int)(screenHeight * optionRatio);
            label_optionD.Top = (int)(screenHeight * optionRatio);

            for (int i = 0; i < nCol; i++)
            {
                if (i == 0)
                {
                    pictureBox_A.Left = (int)(screenWidth * 1 /8) - pictureBox_A.Width / 2;
                    label_optionA.Left = (int)(screenWidth * 1 /8) - label_optionA.Width / 2;
                }
                else if (i == 1)
                {
                    pictureBox_B.Left = (int)(screenWidth * 3 / 8) - pictureBox_A.Width / 2;
                    label_optionB.Left = (int)(screenWidth * 3 /8) - label_optionA.Width / 2;
                }
                else if (i == 2)
                {
                    pictureBox_C.Left = (int)(screenWidth * 5 / 8) - pictureBox_A.Width / 2;
                    label_optionC.Left = (int)(screenWidth * 5 /8) - label_optionA.Width / 2;
                }
                else
                {
                    pictureBox_D.Left = (int)(screenWidth * 7 / 8) - pictureBox_A.Width / 2;
                    label_optionD.Left = (int)(screenWidth * 7 /8) - label_optionA.Width / 2;
                }
            }
            #endregion

            #region 填充底座
            Image imgA_Bg = global::RueHelper.Properties.Resources.voteA_bg;
            Image imgA_Top = global::RueHelper.Properties.Resources.voteA_top;

            Image imgB_Bg = global::RueHelper.Properties.Resources.voteB_bg;
            Image imgB_Top = global::RueHelper.Properties.Resources.voteB_top;

            Image imgC_Bg = global::RueHelper.Properties.Resources.voteC_bg;
            Image imgC_Top = global::RueHelper.Properties.Resources.voteC_top;

            Image imgD_Bg = global::RueHelper.Properties.Resources.voteD_bg;
            Image imgD_Top = global::RueHelper.Properties.Resources.voteD_top;

            int bgY = pictureBox_A.Height - imgA_Bg.Height;

            Rectangle bgRect = new Rectangle(0, 0, imgA_Bg.Width, imgA_Bg.Height);
            Rectangle topRect = new Rectangle(0, 0, imgA_Top.Width, imgA_Top.Height);
            GraphicsUnit units = GraphicsUnit.Pixel;
            int topH = imgA_Top.Height - 2;
            int topX = (pictureBox_A.Width - imgA_Top.Width) / 2;
            {
                Bitmap _bmp = new Bitmap(62, 350, PixelFormat.Format32bppArgb);
                using (Graphics _g = Graphics.FromImage(_bmp))
                {
                    _g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    _g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    _g.DrawImage(imgA_Bg, 0, bgY, bgRect, units);
                    _g.DrawImage(imgA_Top, topX, bgY - topH, bgRect, units);
                }
                pictureBox_A.Image = _bmp;
             }
             {
                Bitmap _bmp = new Bitmap(62, 350, PixelFormat.Format32bppArgb);
                using (Graphics _g = Graphics.FromImage(_bmp))
                {
                    _g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    _g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    _g.DrawImage(imgB_Bg, 0, bgY, bgRect, units);
                    _g.DrawImage(imgB_Top, topX, bgY - topH, bgRect, units);
                }
                pictureBox_B.Image = _bmp;
            }

            {
                Bitmap _bmp = new Bitmap(62, 350, PixelFormat.Format32bppArgb);
                using (Graphics _g = Graphics.FromImage(_bmp))
                {
                    _g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    _g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    _g.DrawImage(imgC_Bg, 0, bgY, bgRect, units);
                    _g.DrawImage(imgC_Top, topX, bgY - topH, bgRect, units);
                }
                pictureBox_C.Image = _bmp;
            }

            {
                Bitmap _bmp = new Bitmap(62, 350, PixelFormat.Format32bppArgb);
                using (Graphics _g = Graphics.FromImage(_bmp))
                {
                    _g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    _g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    _g.DrawImage(imgD_Bg, 0, bgY, bgRect, units);
                    _g.DrawImage(imgD_Top, topX, bgY - topH, bgRect, units);
                }
                pictureBox_D.Image = _bmp;
            }
            #endregion

            #region 评价
          ///  double picEval = 0.8;

           /// pictureBox_Stu.Left = (int)(screenWidth * 2 / 9) - pictureBox_Stu.Width / 2;
            pictureBox_Stu.Left = 0;
            pictureBox_Stu.Top = (int)(screenHeight - 110);
            pictureBox_Stu.Width = (int)(screenWidth / 2);
            pictureBox_Stu.Height = 110;

            pictureBox_Tea.Left = (int)(screenWidth / 2);
            pictureBox_Tea.Top = (int)(screenHeight -110);
            pictureBox_Tea.Width = (int)(screenWidth / 2);
            pictureBox_Tea.Height = 110;

            picture_sel.Parent = pictureBox_Stu;//指定父级
            picture_sel.Location = new Point(55, 5);//在pictureBox1中的坐标
            picture_tea.Parent = pictureBox_Tea;//指定父级
            picture_tea.Location = new Point(55, 5);//在pictureBox1中的坐标
            ///picture_sel.BackgroundImageLayout = "Stretch";
            ///upInfo("非常优秀","表现不错");


            #endregion
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;//最小化
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Theout(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer, 1) == 0)
            {
                //string data = Common.GetXitiResult();//FormPPTPractise
                string data = Global.AnswerStu;
                if (data.Length>0)
                {
                    Log.Info("xiti.get=" + data);
                    DateTime tm_now = DateTime.Now;
                    TimeSpan createtimespan = new TimeSpan(tm_create.Ticks);
                    TimeSpan nowtimespan = new TimeSpan(tm_now.Ticks);
                    TimeSpan timespan = nowtimespan.Subtract(createtimespan).Duration();
                    int timeDiff = timespan.Minutes * 60 + timespan.Seconds;

                    for (int i = 0; i < data.Split('|').Length; i++)
                    {
                        string temp = data.Split('|')[i].Split(':')[0].ToString().Replace("-", "");
                        int num = Convert.ToInt16(Global.getSeatByCardid(temp));
                        //int num = Convert.ToInt16(data.Split('|')[i].Split(':')[0].ToString().Replace("-", ""));
                        string answer = data.Split('|')[i].Split(':')[1];
                        string context = num + ":" + answer + ":" + timeDiff;
                        Log.Info("vote.item=" + context);
                        if (answer.Length > 1)
                            answer = answer.Substring(answer.Length - 1);
                        statisticABCD(num, answer);
                        VoteColumnEvent(answer, pictureBox_A);
                    }
                    Httpd.NotifyVoteEvent();
                }
                Interlocked.Exchange(ref inTimer, 0); 
            }
        }
        private void statisticABCD(int seat, string answer)
        {
            bool bfound = false;
            foreach(VoteItem item in votelist)
            {
                if(item.seat == seat)
                {
                    bfound = true;
                    item.answer = answer;
                }
            }
            if(!bfound)
            {
                VoteItem item = new VoteItem();
                item.seat = seat;
                item.answer = answer;
                votelist.Add(item);
            }

            int nA = 0, nB = 0, nC = 0, nD = 0;
            foreach (VoteItem item in votelist)
            {
                if (item.answer == "A")
                {
                    nA++;
                }else if (item.answer == "B"){
                    nB++;
                }else if (item.answer == "C"){
                    nC++;
                }
                else if (item.answer == "D")
                {
                    nD++;
                }
            }

            countA = nA;
            countB = nB;
            countC = nC;
            countD = nD;
            
        }
        public void VoteColumnEvent(string name, PictureBox pic)
        {
            if (pic.InvokeRequired)
            {
                InvokeColumnState callback = new InvokeColumnState(VoteColumnEvent);
                pic.Invoke(callback, new object[] { name, pic});
            }
            else
            {
                label_countA.Text = "" + countA;
                label_countB.Text = "" + countB;
                label_countC.Text = "" + countC;
                label_countD.Text = "" + countD;

                Image imgA_Bg = Properties.Resources.voteA_bg;
                Image imgA_Top = Properties.Resources.voteA_top;
                Image imgA_Count = Properties.Resources.voteA_count;
                Image imgA_Line = Properties.Resources.voteA_line;
                Image imgB_Bg = Properties.Resources.voteB_bg;
                Image imgB_Top = Properties.Resources.voteB_top;
                Image imgB_Count = Properties.Resources.voteB_count;
                Image imgB_Line = Properties.Resources.voteB_line;
                Image imgC_Bg = Properties.Resources.voteC_bg;
                Image imgC_Top = Properties.Resources.voteC_top;
                Image imgC_Count = Properties.Resources.voteC_count;
                Image imgC_Line = Properties.Resources.voteC_line;
                Image imgD_Bg = Properties.Resources.voteD_bg;
                Image imgD_Top = Properties.Resources.voteD_top;
                Image imgD_Count = Properties.Resources.voteD_count;
                Image imgD_Line = Properties.Resources.voteD_line;

                int bgY = pictureBox_A.Height - imgA_Bg.Height;
                int lineX = (pictureBox_A.Width - imgA_Line.Width) / 2;

                Rectangle bgRect = new Rectangle(0, 0, imgA_Bg.Width, imgA_Bg.Height);
                Rectangle lineRect = new Rectangle(0, 0, imgA_Line.Width, imgA_Line.Height);
                Rectangle topRect = new Rectangle(0, 0, imgA_Top.Width, imgA_Top.Height);
                Rectangle countRect = new Rectangle(0, 0, imgA_Count.Width, imgA_Count.Height);
                GraphicsUnit units = GraphicsUnit.Pixel;

                int lineH = imgA_Line.Height - 4;
                int topH = imgA_Top.Height - 4;
                int lineY = pictureBox_A.Height - 74;

                int count = 1;
                {
                    if(countA0 != countA)
                    {
                        gColA = pictureBox_A.CreateGraphics();
                        if (countA <= 20)
                            count = countA * 2;
                        else
                            count = 40 + (countA - 20);
                        gColA.Clear(System.Drawing.Color.White);
                        gColA.DrawImage(imgA_Bg, 0, bgY, bgRect, units);
                        for (int i = 1; i <= count; i++)
                            gColA.DrawImage(imgA_Line, lineX, lineY - lineH * i, lineRect, units);
                        gColA.DrawImage(imgA_Top, lineX, lineY - lineH * count - topH, topRect, units);
                    }

                    if (countB0 != countB)
                    {
                        gColB = pictureBox_B.CreateGraphics();
                        if (countB <= 20)
                            count = countB * 2;
                        else
                            count = 40 + (countB - 20);
                        gColB.Clear(System.Drawing.Color.White);
                        gColB.DrawImage(imgB_Bg, 0, bgY, bgRect, units);
                        for (int i = 1; i <= count; i++)
                            gColB.DrawImage(imgB_Line, lineX, lineY - lineH * i, lineRect, units);
                        gColB.DrawImage(imgB_Top, lineX, lineY - lineH * count - topH, topRect, units);
                    }

                    if (countC0 != countC)
                    {
                        gColC = pictureBox_C.CreateGraphics();
                        if (countC <= 20)
                            count = countC * 2;
                        else
                            count = 40 + (countC - 20);
                        gColC.Clear(System.Drawing.Color.White);
                        gColC.DrawImage(imgC_Bg, 0, bgY, bgRect, units);
                        for (int i = 1; i <= count; i++)
                            gColC.DrawImage(imgC_Line, lineX, lineY - lineH * i, lineRect, units);
                        gColC.DrawImage(imgC_Top, lineX, lineY - lineH * count - topH, topRect, units);
                    }
                    if (countD0 != countD)
                    {
                        gColD = pictureBox_D.CreateGraphics();
                        if (countD <= 20)
                            count = countD * 2;
                        else
                            count = 40 + (countD - 20);
                        gColD.Clear(System.Drawing.Color.White);
                        gColD.DrawImage(imgD_Bg, 0, bgY, bgRect, units);
                        for (int i = 1; i <= count; i++)
                            gColD.DrawImage(imgD_Line, lineX, lineY - lineH * i, lineRect, units);
                        gColD.DrawImage(imgD_Top, lineX, lineY - lineH * count - topH, topRect, units);
                    }
                    countA0 = countA;
                    countB0 = countB;
                    countC0 = countC;
                    countD0 = countD;

                    RESULT = countA + "," + countB + "," + countC + "," + countD;
                }
            }
        }
        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Win32.AnimateWindow(this.Handle, 200, Win32.AW_SLIDE | Win32.AW_HIDE | Win32.AW_BLEND);
        }
        string _req_answer;

        public void StopT(string req_answer)
        {
            t1.Enabled = false;
            t.Enabled = false;
        }

        public string getResult()
        {
            string result = "";
            ArrayList akeys = new ArrayList(m_hashtable.Keys);
            akeys.Sort();
            foreach (int skey in akeys)
            {
                int uid = skey;
                if (skey < 100)
                    uid = Global.getUidBySeat(skey);
                string pair = uid + ":" + m_hashtable[skey];
                result += (result.Length > 0 ? "," : "") + pair;
            }

            string dir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            string filename = _xitiId + ".txt";
            FileOper fo = new FileOper(dir, filename);
            fo.WriteFile(result);
            return result;
        }
                        
        //public void DrawingArcs(Label label, string KEY, int n, int Total)
        //{
        //    if (label.InvokeRequired)
        //    {
        //        InvokeDrawingArcs labelCallback = new InvokeDrawingArcs(DrawingArcs);
        //        label.Invoke(labelCallback, new object[] { label, KEY, n, Total });
        //    }
        //    else
        //    {
        //        //Log.Info("DrawingArcs Key=" + KEY + ", n=" + n + ", Total=" + Total);
        //        int r = 120;
        //        double ratio = (double)n / Total;
        //        if (ratio > 100)
        //            ratio = 100.00;

        //        string strRatio = string.Format("{0:0.0%}", ratio);//得到5.88%

        //        AnswerCount ac = new AnswerCount();
        //        ac.AnswerBackColor = System.Drawing.Color.FromArgb(212, 214, 213);
        //        ac.AnswerBorderColor = System.Drawing.Color.FromArgb(0, 0, 0);
        //        ac.AnswerBorderWidth = 10;
        //        ac.AnswerW = r + 20;
        //        ac.AnswerH = r + 20;
        //        ac.AnswerX = 4;
        //        ac.AnswerY = 4;
        //        float nArc = (float)n * 360 / Total;
        //        ac.AnswerSweep = (int)nArc;

        //        Color colorBackground = CircleBackgroundColor;
        //        Color colorText = System.Drawing.Color.FromArgb(249, 142, 56);

        //        string text1 = "" + n;
        //        //if (n == 0)
        //        //    text1 = KEY;

        //        Image img = ac.DrawingArcWithText(r, colorBackground, colorText, text1, strRatio);
        //        if (KEY == "A")
        //            pictureBox_A.Image = img;
        //        else if (KEY == "B")
        //            pictureBox_B.Image = img;
        //        else if (KEY == "C")
        //            pictureBox_C.Image = img;
        //        else if (KEY == "D")
        //            pictureBox_D.Image = img;
        //    }
        //}
        

    }

    public class VoteItem2
    {
        public int uid;
        public int seat;
        public string answer="";
    }
}
