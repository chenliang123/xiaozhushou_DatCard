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
using RueHelper.model;


namespace RueHelper
{
    public partial class FormHandon : Form
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
        public const int AW_L2R = 0X40001;
        public const int AW_R2L = 0X40002;
        public const int AW_U2D = 0X40004;
        public const int AW_D2U = 0X40008;

        public enum Effect { Roll, Center, Hide, Slide, Blend }
        private static int[] effmap = { 0, 0x10, 0X10000, 0x40000, 0x80000 };
        private static int[] dirmap = { 1, 5, 4, 6, 2, 10, 8, 9 };
        private Hashtable stuPushCount = new Hashtable();

        //0x20010);   // 居中逐渐显示。
        //0xA0000); // 淡入淡出效果。
        //0x60004); // 自上向下。
        //0x20004); // 自上向下。
        //0x10010);    // 居中逐渐隐藏。
        //0x90000); // 淡入淡出效果。
        //0x50008); // 自下而上。
        //0x10008); // 自下而上。
        public static void Animate(Control ctl, Effect effect, int msec, int angle)
        {
            int flags = effmap[(int)effect];
            if (ctl.Visible) { flags |= 0x10000; angle += 180; }
            else
            {
                if (ctl.TopLevelControl == ctl) flags |= 0x20000;
                else if (effect == Effect.Blend) throw new ArgumentException();
            }
            flags |= dirmap[(angle % 360) / 45];
            bool ok = AnimateWindow(ctl.Handle, msec, flags);
            if (!ok) throw new Exception("Animation failed");
            ctl.Visible = !ctl.Visible;
        }



        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void InvokeHide(bool hide);

        AutoResetEvent are = new AutoResetEvent(false);
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;
        private ArrayList al;
        private ArrayList alText;
        private ArrayList clickstate;
        public ArrayList _rightList;
        public Form11 f11;
        System.Media.SoundPlayer sp = new SoundPlayer(RueHelper.Properties.Resources.click1);
        string _classid = "";
        string _lessonid = "";
        private HashSet<int> _resutlSet = new HashSet<int>();
        public string _xitiId = "";
        public System.Timers.Timer t;
        public System.Timers.Timer t1;
        public string _callnamsStr = "";
        public string _rewardStr = "";
        public string _criticizeStr = "";
        private int _querytimes = 0;
        private int _panelX = 0;
        private System.Timers.Timer _panelTimer;
        int inTimer = 0;
        public DateTime tm_create = DateTime.Now;
        private StudentInfo[] si;
        public string RESULT = "";
        public FormHandon(string numberstr)
        {
            Global.panelshow = 1;
            Log.Info("FormHandon.create,  numberstr=" + numberstr);
            al = new ArrayList();
            alText = new ArrayList();
            clickstate = new ArrayList();
            _classid = Global.getClassID()+"";
            _lessonid = Global.getLessonID() + "";
Log.Info("debug. FormHandon._classid=" + _classid + ", _lessonid=" + _lessonid);

            _xitiId = Global.getSchoolID() + "-" +_classid + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            //TODO: 截屏上传
            if ((EService.myppt != null && EService.myppt.isOpen()) || EService.bShowPicture)
            {
                _xitiId = "H_" + _xitiId;

                Image img = ScreenCapture.captureScreen(0, 0);
                string imgName = _xitiId + ".jpg";
                string imgDir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(imgDir))
                    Directory.CreateDirectory(imgDir);

                string imgPath = imgDir + "\\" + imgName;
                img.Save(imgPath);

                Common.uploadPicture(imgPath);
            }

            InitializeComponent();
Log.Info("FormHandon _xitiId=" + _xitiId + ", SetPanel now...");
            int panelH = SetPanel(numberstr);

            Log.Info("FormHandon _xitiId=" + _xitiId + ", SetPanel over...");
            this.Text = "提问[" + _xitiId + "]";

            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;

            this.Height = panelH;
            this.Width = screenWidth;

            StartPosition = FormStartPosition.Manual;
            SetDesktopLocation(0, screenHeight - this.Height);

            this.TopMost = true;
#if DEBUG
            this.TopMost = false;//PPTPractise
#endif
            //this.WindowState = FormWindowState.Maximized;
            this.Hide();
            this.Show();
            this.BringToFront();

            Log.Info("FormHandon Timer_start(Theout) now...");

            //Common.ClearHandon();

            t = new System.Timers.Timer(200);
            t.Elapsed += new System.Timers.ElapsedEventHandler(Theout);
            t.Enabled = true;
            t.AutoReset = true;

            IntelligentRecommend.InitQuestion();
        }
        public void shutdown()
        {
            try
            {
                if (t != null)
                {
                    t.Stop();
                    t.Enabled = false;
                    t = null;
                }
                if (t1 != null)
                {
                    t1.Stop();
                    t1.Enabled = false;
                    t1 = null;
                }
            }
            catch (Exception e)
            {
                Log.Error("Handon.shutdown exception.");
            }
        }
        public bool restart()
        {
            Log.Info("FormHandon.clear");
            RESULT = "";
            for (int i = 0; i < clickstate.Count;i++ )
            {
                PictureBox lb = (PictureBox)al[i];
                PictureBox text = (PictureBox)alText[i];

                LabelStateEventClear(lb, text, i + 1);
                clickstate[i] = 0;
            }


            stuPushCount.Clear();
            _callnamsStr = "";
            _rewardStr = "";
            _criticizeStr = "";
            _querytimes = 0;
            tm_create = DateTime.Now;

            _xitiId = Global.getSchoolID() + "-" + _classid + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");

            //TODO: 截屏上传
            if ((EService.myppt != null && EService.myppt.isOpen()) || EService.bShowPicture)
            {
                _xitiId = "H_" + _xitiId;

                Image img = ScreenCapture.captureScreen(0, 0);
                string imgName = _xitiId + ".jpg";
                string imgDir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
                if (!Directory.Exists(imgDir))
                    Directory.CreateDirectory(imgDir);

                string imgPath = imgDir + "\\" + imgName;
                img.Save(imgPath);

                Common.uploadPicture(imgPath);
            }

            Log.Info("FormHandon _xitiId=" + _xitiId + ", SetPanel now...");

            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            pictureBox4.Visible = false;


            StartPosition = FormStartPosition.Manual;
            SetDesktopLocation(0, screenHeight - this.Height);

            try
            {
                this.TopMost = true;
                this.Show();
                Global.panelshow = 1;
                this.BringToFront();
            }
            catch (Exception e)
            {
                Log.Error("Handon.clear exception.");
                return false;
            }
            
            IntelligentRecommend.InitQuestion();

            //Common.ClearHandon();
            t = new System.Timers.Timer(100);
            t.Elapsed += new System.Timers.ElapsedEventHandler(Theout);
            t.Enabled = true;
            t.AutoReset = true;
            return true;
        }
        public void HideEvent(bool bHide)
        {
            if (this.InvokeRequired)
            {
                try
                {
                    InvokeHide cb = new InvokeHide(HideEvent);
                    this.Invoke(cb, new object[] { bHide });
                    
                }
                finally
                {

                }
                return;
            }
            if (bHide)
            {
               // this.Hide();
            }else{
                _querytimes = 0;
                this.Show();
                this.BringToFront();
            }
                
        }
        public void AppendCallname(int uid)
        {
            IntelligentRecommend.addCallname(uid);

            if (_callnamsStr.IndexOf(uid+":") >= 0)
                return;

            DateTime tm_now = DateTime.Now;
            TimeSpan createtimespan = new TimeSpan(tm_create.Ticks);
            TimeSpan nowtimespan = new TimeSpan(tm_now.Ticks);
            TimeSpan timespan = nowtimespan.Subtract(createtimespan).Duration();
            int timeDiff = timespan.Minutes * 60 + timespan.Seconds;

            string pair = uid + ":" + timeDiff;
            if (_callnamsStr.Length > 0)
                _callnamsStr += ",";
            _callnamsStr += pair;
        }
        public void AppendReward(string uid, int point,string reason,string reasonid)
        {
            if (_rewardStr.IndexOf(uid+":") >= 0)
                return;

            DateTime tm_now = DateTime.Now;
            TimeSpan createtimespan = new TimeSpan(tm_create.Ticks);
            TimeSpan nowtimespan = new TimeSpan(tm_now.Ticks);
            TimeSpan timespan = nowtimespan.Subtract(createtimespan).Duration();
            int timeDiff = timespan.Minutes*60 + timespan.Seconds;

            string pair = uid + ":" + timeDiff + ":" + point + ":" + reason + ":" + reasonid;
            if (_rewardStr.Length > 0)
                _rewardStr += ",";
            _rewardStr += pair;
        }
        public void AppendCriticize(int uid)
        {
            if (_criticizeStr.IndexOf(uid + ":") >= 0)
                return;

            DateTime tm_now = DateTime.Now;
            TimeSpan createtimespan = new TimeSpan(tm_create.Ticks);
            TimeSpan nowtimespan = new TimeSpan(tm_now.Ticks);
            TimeSpan timespan = nowtimespan.Subtract(createtimespan).Duration();
            int timeDiff = timespan.Minutes * 60 + timespan.Seconds;

            if (uid > 0)
            {
                string pair = uid + ":" + timeDiff;
                if (_criticizeStr.Length > 0)
                    _criticizeStr += ",";
                _criticizeStr += pair;
            }
        }

        public string GetCallname()
        {
            return _callnamsStr;
        }
        public string GetRewarded()
        {
            return _rewardStr;
        }
        public string GetCriticize()
        {
            return _criticizeStr;
        }
        public string GetResult()
        {
            return RESULT;
        }

        [DllImport("winmm.dll", EntryPoint = "waveOutSetVolume")]
        public static extern int WaveOutSetVolume(IntPtr hwo, uint dwVolume);
        private void SetVol(double arg)
        {
            double newVolume = ushort.MaxValue * arg / 10.0;

            uint v = ((uint)newVolume) & 0xffff;
            uint vAll = v | (v << 16);

            int retVal = WaveOutSetVolume(IntPtr.Zero, vAll);
        }
        private int SetPanel(string numberstr)
        {
            ClassInfo ci = JsonOper.DeserializeJsonToObject<ClassInfo>(numberstr);
            si = ci.Data.Student;
            int _count = screenWidth / 51;
            int _line = ci.Data.StudentCount % _count > 0 ? ci.Data.StudentCount / _count + 1 : ci.Data.StudentCount / _count;
            int panelHeight = _line * 50 + 10;///////////////////////
            if (_line == 2)
                panelHeight = _line * 50 + 60;
            else if (_line == 1)
                panelHeight = _line * 50 + 110;

            int panelLH = this.screenHeight - panelHeight;

            this.panel1.Location = new System.Drawing.Point(0, 0);//panelLH
            this.panel1.Size = new System.Drawing.Size(this.screenWidth, panelHeight);

            this.panel2.Location = new System.Drawing.Point(0, 0);//panelLH
            this.panel2.Size = new System.Drawing.Size(this.screenWidth, panelHeight);
            this.panel1.BringToFront();
            this.panel2.Hide();
            
            label_top3_1.Text = "";
            label_top3_2.Text = "";
            label_top3_3.Text = "";

            int top3_top = (panelHeight - label_top3_1.Size.Height) / 2 - 10;
            label_top3_1.Text = " ";
            label_top3_2.Text = " ";
            label_top3_3.Text = " ";
            label_top3_1.Top = top3_top;
            label_top3_2.Top = top3_top;
            label_top3_3.Top = top3_top;

            int highInterval = 10;//三行的行间距
            if (_line == 2)
                highInterval = 25;//两行变三行,两行的行间距
            else if (_line == 1)
                highInterval = 55;

            int _lw = 0;
            int _br = 1;
            int _locationWidth = 0;
            int _locationHeight = 40;
            int nStudentCount = ci.Data.StudentCount;
            for (int i = 1; i <= nStudentCount; i++)
            {
                _locationHeight = (_lw * 40) + ((_lw + 1) * highInterval);
                if (i == 1 || _br % 2 == 0)
                {
                    _locationWidth = (screenWidth - _count * 51) / 2 + 5;
                }
                else
                {
                    _locationWidth += 51;
                }
                if (i % _count == 0)
                {
                    _br *= 2;
                    if (_lw <= _line)
                    {
                        _lw++;
                    }
                }
                else
                {
                    _br=1;
                }
                PictureBox pic = new PictureBox();
                AnswerCount ac = new AnswerCount();
                ac.ImageWidth = 40;
                ac.ImagesHeight = 40;
                ac.FontStyle = System.Drawing.FontStyle.Bold;
                ac.AnswerFamily = "微软雅黑";
                ac.AnswerFontSize = 15.75F;
                Image lbimg = ac.DrawingArcFill(1, 1, System.Drawing.Color.FromArgb(254, 80, 79), 0, 41, System.Drawing.Color.FromArgb(254, 80, 79));
                Image textimg = ac.DrawingString(Brushes.White, i + "");
                bool ishasnum = false;

                for (int j = 0; j < nStudentCount; j++)
                {
                    string seat0 = ci.Data.Student[j].SEAT.Replace("-", "");
                    if (seat0 == "")
                        seat0 = "0";
                    int nSeat0 = Util.toInt(seat0);
                    if (i == nSeat0)
                    {
                        ishasnum = true;
                        break;
                    }
                }
                if (ishasnum)
                {
                    textimg = ac.DrawingString(Brushes.DimGray, i + "");
                    lbimg = ac.DrawingArcFill(1, 1, System.Drawing.Color.FromArgb(204, 204, 204), 2, 41, System.Drawing.Color.White);
                }
                pic.Location = new System.Drawing.Point(_locationWidth, _locationHeight);
                pic.Name = "click_" + i;
                pic.Size = new System.Drawing.Size(41, 41);
                pic.TabIndex = 0;
                pic.Image = lbimg;
                PictureBox text = new PictureBox();
                text.BackColor = System.Drawing.Color.Transparent;
                text.Location = new System.Drawing.Point(0, 0);
                text.Name = "text_" + i;
                text.Size = new System.Drawing.Size(41, 41);
                text.TabIndex = 0;
                text.Image = textimg;
                pic.Controls.Add(text);
                al.Add(pic);
                alText.Add(text);
                clickstate.Add(0);
                this.panel1.Controls.Add(pic);
            }
            return panelHeight;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;//最小化
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void updateTop3(int index)
        {
            string name0 = si[index-1].Name;
            string name = Global.getUsernameBySeat(index);
            Log.Info("updateTop3: index=" + index+", name="+Global.getUsernameBySeat(index)+", "+name);
            //①②③
            if (label_top3_1.Text.Length <=1)
                label_top3_1.Text = "① " + name;
            else if (label_top3_2.Text.Length <=1)
                label_top3_2.Text = "② " + name;
            else if (label_top3_3.Text.Length <=1)
                label_top3_3.Text = "③ " + name;

            int left1 = screenWidth / 3 - label_top3_1.Size.Width / 2 -50;
            int left2 = (screenWidth - label_top3_2.Size.Width) / 2;
            int left3 = screenWidth*2/3 - label_top3_3.Size.Width / 2 + 50;

            label_top3_1.Left = left1;
            label_top3_2.Left = left2;
            label_top3_3.Left = left3;
        }
        public void SwitchView(int index)
        {
            this.Show();
            this.BringToFront();
            Log.Info("**************SwitchView************");
            SetDesktopLocation(0, screenHeight - this.Height);
            if(index==1)
            {
                this.panel1.BringToFront();
                AnimateWindow(this.panel1.Handle, 800, AW_U2D);//AW_U2D//AW_L2R
                this.panel2.Hide();   
            }
            else if (index == 2)
            {
                this.panel2.BringToFront();
                this.panel2.Show();
                AnimateWindow(this.panel2.Handle, 800, AW_D2U);//AW_D2U//AW_R2L
                this.panel1.Hide();
            }
        }

        private void Theout(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer, 1) == 0)
            {
                //string data = Common.GetHandon();
               string data = Global.RaiseStu;        
               // Global.RaiseStu = "";               
                if (data != null && data.Length > 0 && t.Enabled)
                {
                    //Console.WriteLine(data);   //data:2|3
                    DateTime tm_now = DateTime.Now;
                    TimeSpan createtimespan = new TimeSpan(tm_create.Ticks);
                    TimeSpan nowtimespan = new TimeSpan(tm_now.Ticks);
                    TimeSpan timespan = nowtimespan.Subtract(createtimespan).Duration();
                    int timeDiff = timespan.Minutes*60 + timespan.Seconds;
                    Log.Info("Theout. handon=" + data + ", timediff=" + timeDiff);

                    //0-3:56AAA549,H|0-4:56AAA54A,H|
                    string context = "";
                    string[] szItem = data.Split('|');
                    for (int i = 0; i < szItem.Length; i++)
                    {
                        string item = szItem[i];
                        item = Global.getSeatByCardid(item);
                        int num = Util.toInt(item);
                        context = num + ":H:" + timeDiff;
                        //updateTop3(num);

                        int _count = 1;
                        if (stuPushCount.Contains(num))
                        {
                            _count = (int)stuPushCount[num]+1;
                            stuPushCount.Remove(num);
                        }
                        else
                        {
                            IntelligentRecommend.addHandon(num);
                        }
                        stuPushCount.Add(num, _count);
                        Log.Info("Theout. num=" + num + ", count=" + _count);
                        for (int j = 0; j < al.Count; j++)
                        {
                            PictureBox lb = (PictureBox)al[j];
                            PictureBox text = (PictureBox)alText[j];
                            if ((num - 1) == j )//&& (int)clickstate[j] == 0
                            {
                                LabelStateEvent(context, lb, text, j + 1);
                                clickstate[j] = 1;
                                Log.Info("Theout.LabelStateEvent id=" + num + ", al.Count=" + al.Count + ", ok.");
                            }
                        }
                    }
                }
                else
                {
                    _querytimes++;
                    if(_querytimes>10*3)
                    {
                        HideEvent(true);
                    }
                }
                Interlocked.Exchange(ref inTimer, 0); 
            }
        }

        public void LabelStateEvent(string context, PictureBox pic, PictureBox text,int i)
        {
            if (pic.InvokeRequired)
            {
                InvokeLabelState labelCallback = new InvokeLabelState(LabelStateEvent);
                pic.Invoke(labelCallback, new object[] { context, pic, text, i });
            }
            else
            {
                AnswerCount ac = new AnswerCount();
                ac.ImageWidth = 40;
                ac.ImagesHeight = 40;
                ac.FontStyle = System.Drawing.FontStyle.Bold;
                ac.AnswerFamily = "微软雅黑";
                ac.AnswerFontSize = 15.75F;
                pic.Image = ac.DrawingArcFill(1, 1, System.Drawing.Color.FromArgb(69, 175, 101), 0, 41, System.Drawing.Color.FromArgb(69, 175, 101));
                text.Image = ac.DrawingString(Brushes.White, i + "");

                int _count = 0;
                if (stuPushCount.Contains(i))
                {
                    _count = (int)stuPushCount[i];
                }

                if(_count == 1)
                {
                    RESULT += (RESULT.Length > 0 ? "," : "") + context;
                    if (Global.Sound() && _count == 1)
                    {
                        System.Media.SystemSounds.Asterisk.Play();
                        //sp.Play();
                    }
                    updateTop3(i);
                }
            }

        }

        public void LabelStateEventClear(PictureBox pic, PictureBox text, int i)
        {
            if (pic.InvokeRequired)
            {
                InvokeLabelStateClear labelCallback = new InvokeLabelStateClear(LabelStateEventClear);
                pic.Invoke(labelCallback, new object[] {pic, text, i });
            }
            else
            {
                AnswerCount ac = new AnswerCount();
                ac.ImageWidth = 40;
                ac.ImagesHeight = 40;
                ac.FontStyle = System.Drawing.FontStyle.Bold;
                ac.AnswerFamily = "微软雅黑";
                ac.AnswerFontSize = 15.75F;

                pic.Image = ac.DrawingArcFill(1, 1, System.Drawing.Color.FromArgb(204, 204, 204), 2, 41, System.Drawing.Color.White);
                text.Image = ac.DrawingString(Brushes.DimGray, i + "");
                //pic.Image = ac.DrawingArcFill(1, 1, System.Drawing.Color.FromArgb(69, 175, 101), 0, 41, System.Drawing.Color.FromArgb(69, 175, 101));
                //text.Image = ac.DrawingString(Brushes.White, i + "");
            }

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            Win32.AnimateWindow(this.Handle, 200, Win32.AW_SLIDE | Win32.AW_HIDE | Win32.AW_BLEND);
        }

        public void Competitive(string req_answer)
        {
            t1 = new System.Timers.Timer(200);
            t1.Elapsed += new System.Timers.ElapsedEventHandler(CompetitiveTheout);
            t1.Enabled = true;
            t1.AutoReset = true;
        }

        private void CompetitiveTheout(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReadFileEvent();
        }

        public void ReadFileEvent()
        {
            if (this.InvokeRequired)
            {
                InvokeReadFile readCallback = new InvokeReadFile(ReadFileEvent);
                this.Invoke(readCallback, new object[] { });
            }
            else
            {
                string answerStr = RESULT;// fo.ReadFile();
            }
        }

        public string StopT()
        {
            t.Enabled = false;
            string answerStr = RESULT.Replace(":H","");// fo.ReadFile();
            Log.Info("StopT() answerStr=" + answerStr);
            return answerStr;
        }

        public string getHandonRepeated()
        {
            string result = "";
            ArrayList akeys = new ArrayList(stuPushCount.Keys);//Hashtable
            akeys.Sort(); //按字母顺序进行排序
            foreach (int seat in akeys)
            {
                StudentInfo si = Global.getUserInfoBySeat(seat);
                if (si == null)
                    continue;

                string uid = si.ID;
                int count = (int)stuPushCount[seat];
                string pair = uid + ":" + count;
                if(count <= 3)
                    continue;

                result += (result.Length > 0 ? "," : "") + pair;
            }
            return result;
        }

        private void panel2_DoubleClick(object sender, EventArgs e)
        {
            //this.Hide();
            //Global.panelshow = 0;
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            this.Hide();
            Global.panelshow = 0;
        }
    }
}
