
using Newtonsoft.Json;
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

namespace RueHelper
{
    public partial class FormKaoQin : Form
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
        private ArrayList al;
        private ArrayList bl;

        AutoResetEvent are = new AutoResetEvent(false);
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

        public FormKaoQin()
        {
            InitializeComponent();
            this.Height = screenHeight;
            this.Width = screenWidth;
            StartPosition = FormStartPosition.Manual;
            SetDesktopLocation(0, 0);
            al = new ArrayList();
            bl = new ArrayList();
            this.TopMost = true;
            this.Show();
            this.BringToFront();

            //根据传过来的组数，动态创建控件
            AddGroupBox();
        }

        public void SetPanel(string strCard, string numData, string wrongName)
        {
            for (int j = 0; j < al.Count; j++)
            {
                PictureBox pic = (PictureBox)al[j];
                Label lb = (Label)bl[j];
                if (strCard.IndexOf(pic.Name) >= 0)
                {
                    pic.BackColor = Color.FromArgb(0, 230, 117);
                    //pic.txt.ForeColor = Color.White;                   
                }
            }
            for (int i = 0; i < bl.Count; i++)
            {
                Label lb = (Label)bl[i];
                if (strCard.IndexOf(lb.Name) >= 0)
                {
                    lb.ForeColor = Color.White;                   
                }
            }

            Label lab = new Label();
            lab.Width = 800;
            lab.Height = 40;
            lab.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lab.BackColor = Color.Transparent;
            lab.Font = new System.Drawing.Font("微软雅黑", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            lab.Text = "教室: " + Global.roomname + "                    教学班: " + Global.classname + "                        教师:  " + Global.teachername;
            lab.ForeColor = Color.Black;
            lab.Location = new Point((screenWidth - 800) / 2, 70);
            this.Controls.Add(lab);

            //下划线
            PictureBox line = new PictureBox();
            line.Height = 2;
            line.Width = 1100;
            line.BackColor = Color.FromArgb(201, 201, 201);
            line.Location = new Point((screenWidth - 1100) / 2, 130);
            this.Controls.Add(line);


            if(numData.Length > 0)
            {
                string[] sNum = numData.Split(',');              
                Label la = new Label();
                la.Width = 350;
                la.Height = 40;
                la.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                la.BackColor = Color.Transparent;
                la.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                la.Text = "应到:" + sNum[0] + "   出勤:" + sNum[1] + "   缺勤:" + sNum[2] + "   走错教室:" + sNum[3];
                la.ForeColor = Color.Black;
                la.Location = new Point((screenWidth - 350) / 2, 150);
                this.Controls.Add(la);
            }

            if (wrongName.Length > 0)
            {
                Label txt = new Label();
                txt.Width = 140;
                txt.Height = 40;
                txt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                txt.BackColor = Color.Transparent;
                txt.Font = new System.Drawing.Font("微软雅黑", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
                txt.Text = "非本教室学生";
                int iL = (screenWidth - 140) / 2;
                int iT = screenHeight - 180; 
                txt.Location = new Point(iL, iT);
                this.Controls.Add(txt);
                int iW = (screenWidth - 300) / 8;
                int iLeft = (iW - 80) / 2 + 150;

                string[] sItem = wrongName.Split('，');

                for (int i = 0; i < sItem.Length; i++)
                {
                    PictureBox gbox = new PictureBox();
                    gbox.Width = 80;
                    gbox.Height = 40;
                    gbox.Location = new Point(iLeft + i % 8 * iW, iT + 40);
                    gbox.BackColor = Color.FromArgb(75,168,243);
                    this.Controls.Add(gbox);
                    AddTxt(gbox, sItem[i], false);
                }
            }
        }
      
        public void AddGroupBox()
        {
            int iW = (screenWidth -300) / 8;
            int iL = (iW - 80) / 2 + 150;
            //string name = "gbox";
            //int index = 0;
            //string[] RankArr = Rank.Split(',');
            //string[] strArray = str.Split(',');
            for (int i = 0; i < Global.g_Studentlist.Count; i++)
            {
                PictureBox gbox = new PictureBox();
                gbox.Name = Global.g_Studentlist[i].cardid;
                gbox.Width = 80;
                gbox.Height = 40;
                al.Add(gbox);
                if (i / 8 <= 0)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 200);
                }
                else if (i / 8 <= 1)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 250);
                }
                else if (i / 8 <= 2)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 300);
                }
                else if (i / 8 <= 3)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 350);
                }
                else if (i / 8 <= 4)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 400);
                }
                else if (i / 8 <= 5)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 450);
                }
                else if (i / 8 <= 6)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 500);
                }
                else if (i / 8 <= 7)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 550);
                }
                else if (i / 8 <= 8)
                {
                    gbox.Location = new Point(iL + i % 8 * iW, 600);
                }
                
                gbox.BackColor = Color.FromArgb(232, 232, 232);
                this.Controls.Add(gbox);               
               //添加文本控件
                AddTxt(gbox, Global.g_Studentlist[i].name, true);            
            }
        }
        public void AddTxt(PictureBox gb, string name, bool isBlack)
        {        
            Label txt = new Label();
            txt.Name = gb.Name;
            bl.Add(txt);
            txt.Width = 80;
            txt.Height = 40;
            txt.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            txt.BackColor = Color.Transparent;
            txt.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            txt.Text = name;
            if (isBlack)
            {
                txt.ForeColor = Color.Black;
            }
            else
            {
                txt.ForeColor = Color.White;
            }           
            txt.Location = new Point(0, 0);
            gb.Controls.Add(txt);           
        }
    }
}