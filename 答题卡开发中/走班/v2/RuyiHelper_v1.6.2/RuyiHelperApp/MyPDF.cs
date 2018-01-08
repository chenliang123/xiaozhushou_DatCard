﻿using Microsoft.Office.Core;
using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using RueHelper.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Windows.Forms;
namespace RueHelper
{

    public class MyPDF
    {
        [DllImport("user32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessage(IntPtr HWnd, uint Msg, int WParam, int LParam);

        [DllImport("User32.dll ", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);//关键方法  

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);//0 关闭窗口; 1 正常大小显示窗口; 2 最小化窗口; 3 最大化窗口

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);  //导入为windows窗体设置焦点的方法

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(double idAttach, double idAttachTo, bool fAttach);

        [DllImport("USER32.DLL")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);  //导入模拟键盘的方法

        [DllImport("USER32.DLL")]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        const int MOUSEEVENTF_MOVE = 0x1;
        const int MOUSEEVENTF_LEFTDOWN = 0x2;
        const int MOUSEEVENTF_LEFTUP = 0x4;
        const int MOUSEEVENTF_RIGHTDOWN = 0x8;
        const int MOUSEEVENTF_RIGHTUP = 0x10;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        const int MOUSEEVENTF_MIDDLEUP = 0x40;
        const int MOUSEEVENTF_WHEEL = 0x800;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public const int WM_SYSCOMMAND = 0x112;
        public const int SC_MINIMIZE = 0xF020;
        public const int SC_MAXIMIZE = 0xF030;

        public const int SW_HIDE = 0;
        public const int SW_NORMAL = 1;
        public const int SW_ShowMaximized = 3;
        public const int SW_ShowNOACTIVATE = 4;
        public const int SW_Show = 5;
        public const int SW_Minimize = 6;
        public const int SW_ShowNA = 8;
        public const int SW_Restore = 9;
        public const int SW_ShowDEFAULT = 10;

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint AssocQueryString(AssocF flags, AssocStr str, string pszAssoc, string pszExtra, [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// /////////////////////////////////////////////////////////////////////
        public bool bPageTurning = true;
        public int pageTotal = 0;
        public int pageCurrent = 0;
        public int pageLast = 0;
        public string filename = "";
        public string filepath = "";
        public static Hashtable g_fileImgStatus = new Hashtable();//缩略图生成

        public object missing = System.Reflection.Missing.Value;
        private Process p = null;
        private string exepath = "";
        public MyPDF()
        {

        }
        [Flags]
        public enum AssocF
        {
            Init_NoRemapCLSID = 0x1,
            Init_ByExeName = 0x2,
            Open_ByExeName = 0x2,
            Init_DefaultToStar = 0x4,
            Init_DefaultToFolder = 0x8,
            NoUserSettings = 0x10,
            NoTruncate = 0x20,
            Verify = 0x40,
            RemapRunDll = 0x80,
            NoFixUps = 0x100,
            IgnoreBaseClass = 0x200
        }

        public enum AssocStr
        {
            Command = 1,
            Executable,
            FriendlyDocName,
            FriendlyAppName,
            NoOpen,
            ShellNewValue,
            DDECommand,
            DDEIfExec,
            DDEApplication,
            DDETopic
        }
        public static string FileExtentionInfo(AssocStr assocStr, string doctype)
        {
            uint pcchOut = 0;
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, null, ref pcchOut);

            StringBuilder pszOut = new StringBuilder((int)pcchOut);
            AssocQueryString(AssocF.Verify, assocStr, doctype, null, pszOut, ref pcchOut);
            return pszOut.ToString();
        }

        public bool Open(string filepath)
        {
            minisizeProc();

            filename = Path.GetFileName(filepath);
            exepath = FileExtentionInfo(AssocStr.Command, ".pdf");
            Log.Info("openPDF exePath=" + exepath);
            if (exepath.Length <= 0)
                return false;

            try
            {

                if (exepath.IndexOf("QQ") > 0)
                {
                    System.Diagnostics.Process.Start(filepath);
                    return false;
                }
                else if (exepath.IndexOf("rundll32.exe") > 0)
                {
                    //MessageBox.Show("请安装PDF阅读器!", "Warning!!!");
                    string content = HttpUtility.UrlEncode("Foxit Reader");
                    string searchUrl = "https://www.baidu.com/s?ie=utf-8&wd="+ content;
                    string downloadUrl = "https://get.adobe.com/cn/reader/";
                    System.Diagnostics.Process.Start(downloadUrl);
                    return false;
                }
                else
                {
                    

                    System.Diagnostics.Process.Start(filepath);
                    maxisizeProc();
                }
                return true;
            }
            catch (Exception ex)
            {
                int errorcode = 0;
                var w32ex = ex as Win32Exception;
                if (w32ex == null)
                {
                    w32ex = ex.InnerException as Win32Exception;
                }
                if (w32ex != null)
                {
                    errorcode = w32ex.ErrorCode;
                }
                Log.Error("DocOpen exception. " + ex.Message + ", errorcode=" + errorcode);//Error HRESULT E_FAIL has been returned from a call to a COM component.
                Close();
            }
            finally
            {
            }
            return false;
        }
        public void Close()
        {
            try
            {
                //if (p != null)
                //    p.Kill();
                Process[] myproc = Process.GetProcesses();
                foreach (Process item in myproc)
                {
                    if (item.MainWindowTitle.IndexOf(filename) >= 0)
                    {
                        item.Kill();
                    }
                }
            }
            catch (Exception e1)
            {
                Log.Error(e1.Message);
            }
            finally
            {
                p = null;
                GC.Collect();
            }
        }

        public bool NextSlide()
        {
            try
            {
                if (maxisizeProc())
                {
                    keybd_event(0x22, 0, 0, 0);
                    keybd_event(0x22, 0, 2, 0);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("PDF.Next() exception. " + ex.Message);
                Close();
            }
            return false;
        }
        public bool PreviousSlide()
        {
            try
            {
                if(maxisizeProc())
                {
                    keybd_event(0x21, 0, 0, 0);
                    keybd_event(0x21, 0, 2, 0);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("PDF.Last() exception. " + ex.Message);
                Close();
            }
            return false;
        }

        public  bool Up()
        {
            try
            {
                maxisizeProc();
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 100, 0);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Doc.Next() exception. " + ex.Message);
                Close();
                return false;
            }
        }
        public bool Down()
        {
            try
            {
                maxisizeProc();
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -100, 0);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Doc.Next() exception. " + ex.Message);
                Close();
                return false;
            }
        }

        public void minisizeProc()
        {
            //https://msdn.microsoft.com/en-us/library/dd375731(v=vs.85).aspx
            //VK_LWIN 0x5B
            //VK_RWIN 0x5C
            //D key 0x44
            //M key 0x4D
            keybd_event(0x5B, 0, 0, 0);
            keybd_event(0x4D, 0, 0, 0);
            keybd_event(0x4D, 0, 2, 0);
            keybd_event(0x5B, 0, 2, 0);
        }
        public bool maxisizeProc()
        {
            bool ret = false;
            try
            {
                Process[] myproc = Process.GetProcesses();
                foreach (Process item in myproc)
                {
                    //Log.Info("checkPDF process1. " + item.MainWindowTitle);
                    //Log.Info("checkPDF process2. " + item.ProcessName);
                    if (item.MainWindowTitle.IndexOf(filename) >= 0)
                    {
                        p = item;
                        IntPtr hWnd = item.MainWindowHandle;
                        SendMessage(hWnd, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
                        BringWindowToTop(hWnd);
                        ShowWindow(hWnd, SW_ShowMaximized);
                        SetForegroundWindow(hWnd);
                        ret = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("maxisizeProc exception. " + e.Message);
            }
            return ret;
        }
    }
}
