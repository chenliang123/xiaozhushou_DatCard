﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Net;
using System.Collections;
using System.Threading;
using RueHelper.util;
using Microsoft.Win32;

namespace RueHelper
{
    public partial class UserControl_Config1 : UserControl
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string hdip = "";
        public int schoolid = 0;
        public int classid = 0;
        public string classname = "";
        public int autorun = 1;
        public int autoupdate = 1;
        public List<Classes> m_classlist = new List<Classes>();
        private RueSqlite m_db = new RueSqlite();

        public UserControl_Config1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e0)
        {
            bool bReloadClass = false;
            string _classname = "";
            if (comboBox_classlist != null && comboBox_classlist.SelectedItem !=null)
            {
                _classname = comboBox_classlist.SelectedItem.ToString();

                try
                {

                    foreach (Classes c in m_classlist)
                    {
                        if (_classname == "公共教室")
                        {
                            classid = -1;
                            break;
                        }
                        if (_classname == c.name)
                        {
                            classid = c.id;
                        }
                    }
                    if (classid != Global.getClassID())
                        bReloadClass = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("教室ID参数设置错误，请重试！", "警告");
                    return;
                }
                Log.Info("Config.2 classid=" + classid);
            }
            

            try
            {
                hdip = textBox_hdip.Text;
                IPAddress ip;
                if (!IPAddress.TryParse(hdip, out ip))
                {
                    MessageBox.Show("采集器IP地址设置错误，请重试！", "警告");
                    return;
                }
            }
            catch (Exception e) {
                Log.Info("Config.3 "+ e.Message);
            }
            Log.Info("Config.3 hdip=" + hdip);


            SetAutoRun(true);
            Global.setAutoUpdate(1);

            button_apply.Enabled = false;

            {
                string strHDIP = textBox_hdip.Text;
                Boolean bAutorun = autorun==1?true:false;
                Global.saveSchoolConfig(strHDIP, bAutorun);
            }

            //TODO:如果教室ID变化，重新获取相关信息
            if (bReloadClass)
            {
                Global.saveClassConfig(classid, _classname);

                if(Global.loadClassInfo())
                {
                    MessageBox.Show("更新成功!", "提示");

                    //更新班级的接收机ID
                    Thread th = new Thread(delegate()
                    {
                        string hdid = Common.getHDID();
                        if (hdid.Length > 0)
                            Common.uploadHDBind();
                    });
                    th.Start();
                }
                else
                {
                    MessageBox.Show("设置失败，请检查网络!", "提示");
                }
            }
            button_apply.Enabled = true;
        }

        private void UserControl_Config1_Load(object sender, EventArgs e0)
        {
            loadCfg();
            button_apply.Enabled = false;
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            //测试采集器
            String url = "http://P1/EduApi/hd.do?action=handon&classid=0";//&callback=CB
            String utctime = getUTC();
            url = url.Replace("P1", textBox_hdip.Text);
            url = url.Replace("P2", utctime);

            String ret = HTTPReq.HttpGet(url, false);
            if(ret.Length==0)
            {
                MessageBox.Show("测试失败，请确认IP地址和网络连接！", "警告");
            }
            else
            {
                MessageBox.Show("测试成功！", "提示");
            }
            //ThreadStart starter = delegate { HTTPReq.HttpGet(url, true); };
            //new Thread(starter).Start();
        }

        private string getUTC()
        {
            string utctime = DateTime.Now.ToUniversalTime().ToString("r");

            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = DateTime.Now.Subtract(dtStart);
            long timeStamp = toNow.Ticks;
            timeStamp = long.Parse(timeStamp.ToString().Substring(0, timeStamp.ToString().Length - 7));

            string strHex = Convert.ToString(timeStamp, 16).ToUpper();
            return strHex;
        }

        private void button_refresh_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
            Form1.updateFormConfig();
            Form1.closeFormConfig();
        }

        public void loadCfg()
        {
            Log.Info(Global.toString());

            textBox_wifi.Text = Global.getWiFi();
            textBox_wifi.Enabled = false;

            textBox_schoolname.Text = Global.getSchoolname();
            classid = Global.getClassID();
            classname = Global.getClassname();

            //get Class list
            {
                m_classlist.Clear();
                comboBox_classlist.Text = "";
                comboBox_classlist.Items.Clear();
                m_classlist = m_db.getClassBySchoolid(Global.getSchoolID());

                if (Global.IsPublicClassroom())
                {
                    comboBox_classlist.Items.Add("公共教室");
                    comboBox_classlist.SelectedIndex = comboBox_classlist.Items.Count - 1;
                    button_reloadClasses.Visible = false;
                }
                else
                {
                    foreach (Classes c in m_classlist)
                    {
                        if (c.name == "" || c.orderid == 0)
                            continue;

                        comboBox_classlist.Items.Add(c.name);
                        if (classid == c.id)
                        {
                            comboBox_classlist.SelectedIndex = comboBox_classlist.Items.Count - 1;
                        }
                    }
                }

            }
            if(Global.getClassname()=="")
            {
                Classes c = m_db.getClassById(Global.getClassID());
                if(c!=null)
                {
                    Global.setClassname(c.name);
                    Global.setClassID(c.id);
                }
            }
            
            textBox_hdip.Text = Global.getHDIP();

            ////////////////////////////////
            ArrayList iplist = Util.GetInternalIPList();
            if (iplist.Count == 1)
            {
                textBox_360ip.Text = (string)iplist[0];
            }
            else
            {
                foreach (string ip in iplist)
                {
                    string ip_4 = ip.Substring(ip.LastIndexOf(".") + 1);
                    if (ip == "172.18.201.3")
                    {
                        textBox_360ip.Text = ip;
                        break;
                    }
                }
            }
        }


        public void SetAutoRun(bool isAutoRun)
        {
            //set APP path
            {
                RegistryKey regApppath = null;
                string exepath = System.Windows.Forms.Application.ExecutablePath;
                regApppath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\"+Global.m_Exe, RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
                if (regApppath == null)
                {
                    regApppath = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\"+Global.m_Exe);
                }
                if (regApppath != null)
                {
                    regApppath.SetValue("", exepath);
                    regApppath.SetValue("path", Application.StartupPath);
                    regApppath.Close();
                }
            }


            string filepath = Application.StartupPath + @"\" + Global.m_Exe;
            RegistryKey reg = null;
            RegistryKey reg1 = null;
            try
            {
                if (!System.IO.File.Exists(filepath))
                    throw new Exception("该文件不存在!");
                reg1 = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl);
                if (reg1 == null)
                {
                    reg1 = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                }
                if (isAutoRun)
                {
                    reg1.SetValue(Global.m_Exe, filepath);
                }
                else
                {
                    reg1.SetValue(Global.m_Exe, false);
                }
            }
            catch(Exception ex)
            {
                //Log.Error(ex.ToString());  
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Form1.closeFormConfig();
        }

        private void comboBox_classlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_apply.Enabled = true;
        }

        private void button_changeSchool_Click(object sender, EventArgs e)
        {
            FormConfigSchool cfgschoolForm = new FormConfigSchool();
            cfgschoolForm.StartPosition = FormStartPosition.CenterScreen;
            cfgschoolForm.ShowDialog();
        }

        private void checkBox_autorun_CheckedChanged(object sender, EventArgs e)
        {
            button_apply.Enabled = true;
        }

        private void checkbox_update_CheckedChanged(object sender, EventArgs e)
        {
            button_apply.Enabled = true;
        }

        private void textBox_hdip_TextChanged(object sender, EventArgs e)
        {
            button_apply.Enabled = true;
        }

        private void button_chgWifi_Click(object sender, EventArgs e)
        {
            if (button_chgWifi.Text =="修改")
            {
                textBox_wifi.Enabled = true;
                button_chgWifi.Text = "确认";
            }
            else
            {
                textBox_wifi.Enabled = false;
                button_chgWifi.Text = "修改";
                Global.setWiFi(textBox_wifi.Text);
            }

        }

        private void button_reloadClasses_Click(object sender, EventArgs e)
        {
            if (Global.loadSchoolInfo() == 1)
            {
                //重置默认第一个班级
                Classes[] classlist = Global.g_szClasses;
                if (classlist.Length > 0)
                {
                    m_classlist.Clear();
                    comboBox_classlist.Text = "";
                    comboBox_classlist.Items.Clear();
                    m_classlist = m_db.getClassBySchoolid(Global.getSchoolID());
                    foreach (Classes c in m_classlist)
                    {
                        if (c.name == "" || c.orderid == 0)
                            continue;

                        comboBox_classlist.Items.Add(c.name);
                        if (classid == c.id)
                        {
                            comboBox_classlist.SelectedIndex = comboBox_classlist.Items.Count - 1;
                        }
                    }
                    if (Global.IsPublicClassroom())
                    {
                        comboBox_classlist.Items.Add("公共教室");
                    }
                    MessageBox.Show("班级更新成功!", "提示");
                }
            }
        }
    }
}
