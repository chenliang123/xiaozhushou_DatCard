﻿using RueHelper.model;
using RueHelper.util;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace RueHelper
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]

    public class EService : IEService
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Form2 f2;//xiti  做题和抢答
        Form3 f3;//pic
        FormPicture4 f3_4;//pic
        FormCaptureScreen fCaptureScreen;
        Form4 f4;//flash
        Form5 f5;//video
        Form6 f6;//web
        Form7 f7;//ppt
        FormStatistics fStatistics;//xiti.result.statistic abcdwr
        Form10 f10;//点名
        FormAward fAward;
        Form11 f11;//Form13,Form2调Form11
        //FormPPTXiTi fPPTXiti;//抢答 数秒
        FormPPTPractise fPPTPractise;//
        FormVote fVote;//
        FormTwoMinute fTMin;//精彩两分钟
        FormInter fInter; //生生互动
        FormCompete fCp;  //分组竞赛
        FormScore fScore; //分组竞赛得分
        private FormHandon fHandon;
        FormQiangDa fQiangDa;
        FormGroup fGroup;
        FormGroupCallname fGroupCallname;
        FormSummary fSummay;
        FormDraw fRobotPen;

        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

        public static MyPPT myppt = null;//全局ppt对象
        public static MyDoc mydoc = null;//全局doc对象
        public static MyPDF mypdf = null;//全局pdf对象
        public static MyTouch mytouch = null;//全局touch对象
        public static bool bShowPicture = false;
		public static FormCamera fCamera;

        string g_answer = "";
        private static RueSqlite m_db = new RueSqlite();
        private static Dictionary<string,string>  filemap = new Dictionary<string, string>();
        private static Dictionary<string,int> PPTPageMap = new Dictionary<string, int>();

        #region Private functions: Close()
        private void Close()
        {
            Httpd.OnPublicMsg("EXIT");
            if (fSummay != null)
            {
                fSummay.Close();
                fSummay = null;
            }
            if (fCp != null)
            {
                fCp.Close();
                fCp = null;
            }
            if (f2 != null)
            {
                f2.Close();
                f2 = null;
            }
            if (f3 != null)
            {
                f3.Close();
                f3 = null;
            }
            if (f3_4 != null)
            {
                f3_4.Close();
                f3_4 = null;
            }
            if (fCaptureScreen != null)
            {
                fCaptureScreen.Close();
                fCaptureScreen = null;
            }
            if (f4 != null)
            {
                f4.Close();
                f4 = null;
            }
            if (f5 != null)
            {
                f5.Close();
                f5 = null;
            }
            if (f6 != null)
            {
                f6.Close();
                f6 = null;
            }
            if (f7 != null)
            {
                f7.Close();
                f7 = null;
            }
            if (fStatistics != null)
            {
                fStatistics.Close();
                fStatistics = null;
            }
            if (f10 != null)
            {
                f10.Close();
                f10 = null;
            }
            if (fAward != null)
            {
                fAward.Close();
                fAward = null;
            }
            if (f11 != null)
            {
                f11.Close();
                f11 = null;
            }

            if (fPPTPractise != null)
            {
                fPPTPractise.Close();
                fPPTPractise = null;
            }

            if (fVote != null)
            {
                fVote.Close();
                fVote = null;
            }
            if (fTMin != null)
            {
                fTMin.Close();
                fTMin = null;
            }
            if (fHandon != null)
            {
                fHandon.shutdown();
                fHandon.Close();
                fHandon = null;
            }
            if (fQiangDa != null)
            {
                fQiangDa.Close();
                fQiangDa = null;
            }
            
            if (fGroup != null)
            {
                fGroup.Close();
                fGroup = null;
            }
            if (fGroupCallname != null)
            {
                fGroupCallname.Close();
                fGroupCallname = null;
            }
            if (myppt != null)
            {
                myppt.PPTClose();
                myppt = null;
            }

            if(fRobotPen!=null)
            {
                fRobotPen.Clear();
                fRobotPen.Hide();
            }
        }

        #endregion

        #region 构造、析构函数
        public EService()
        {
            //InitCamera();

            if (fRobotPen != null)
            {
                fRobotPen.shutdown();
                fRobotPen = null;
            }
            fRobotPen = new FormDraw();
            fRobotPen.Hide();
        }

        ~EService()
        {
            if (m_db != null)
                m_db.Close();

            if (myppt != null)
            {
                myppt.PPTClose();
                myppt = null;
            }
            if (fCamera != null)
            {
                fCamera.Close();
                fCamera = null;
            }
            Exit("","");
        }
        public Response Init()
        {
            Close();

            if(Global.IsPublicClassroom())
            {
                List<Grade> gradelist = new List<Grade>();
                foreach(Classes c in Global.g_szClasses)
                {
                    bool bfound = false;
                    foreach(Grade g in gradelist)
                    {
                        if(g.id == c.grade)
                        {
                            g.classlist.Add(c);
                            bfound = true;
                            break;
                        }
                    }
                    if(!bfound)
                    {
                        Grade g = new Grade();
                        g.id = c.grade;
                        g.classlist = new List<Classes>();
                        g.classlist.Add(c);
                        switch (g.id)
                        {
                            case 1:
                                g.name = "一年级";break;
                            case 2:
                                g.name = "二年级"; break;
                            case 3:
                                g.name = "三年级"; break;
                            case 4:
                                g.name = "四年级"; break;
                            case 5:
                                g.name = "五年级"; break;
                            case 6:
                                g.name = "六年级"; break;
                            case 7:
                                g.name = "初一年级"; break;
                            case 8:
                                g.name = "初二年级"; break;
                            case 9:
                                g.name = "初三年级"; break;
                            default:
                                g.name = g.id+"年级";break;
                        }
                        gradelist.Add(g);
                    }
                }

                gradelist.Sort((x, y) => x.id.CompareTo(y.id));
                foreach(Grade g in gradelist)
                {
                    g.classlist.Sort((x, y) => x.id.CompareTo(y.id));
                }
                SchoolGrade sg = new SchoolGrade();
                sg.name = Global.getSchoolname();
                sg.id = Global.getSchoolID();
                sg.gradelist = gradelist;
                string data = sg.toJson();
                Response resp = new Response(1, "Init ok", data);
                string ret = resp.toJson();
                return resp;
            }
            else
            {
                Response resp = new Response(0, "Init ok", "");
                return resp;
            }
        }
        public Response StartPenTrail()
        {
            fRobotPen.StartExercise();
            Response resp = new Response(0, "StartPenTrail success", "");
            return resp;
        }
        public Response ShowPenTrail(int index)
        {
            fRobotPen.Show();
            fRobotPen.ShowPenTrail(index);
            string data = fRobotPen.GetImages(index);
            Response resp = new Response(0, "ShowPenTrail success", data);
            return resp;
        }
        public Response HidePenTrail()
        {
            fRobotPen.Hide();
            Response resp = new Response(0, "HidePenTrail success", "");
            return resp;
        }
        public Response ClearPenTrail()
        {
            fRobotPen.SaveImages();
            fRobotPen.Clear();
            fRobotPen.Hide();
            Response resp = new Response(0, "ShowPenTrail success", "");
            return resp;
        }
        public Response ClosePenTrail()
        {
            //同步关闭图片播放的form，防止忘了关闭。
            CloseView();
            fRobotPen.SaveImages();
            fRobotPen.Clear();
            fRobotPen.Hide();
            fRobotPen.CloseMs();
            //TODO: 上传数据
            fRobotPen.UpdateImages();

            Response resp = new Response(0, "ShowPenTrail success", "");
            return resp;
        }
        #region 图片播放（本地图片文件）
        public Response ShowViewPen(string filename)
        {
            Log.Info("ShowViewPen()...");
            int index = Util.toInt(filename);
            string path = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + filename;
            if (index > 0)
            {
                path = fRobotPen.SaveImages(index);
            }

            if (f3 == null)
            {
                try
                {
                    f3 = new Form3(path, 2, 0);
                    f3.Show();
                    bShowPicture = true;
                    //f3.Zoom(1.2,0.2,0.3);
                }
                catch (Exception ex)
                {

                }
                string strBase64 = Util.ImgToBase64String(path);
                Response resp = new Response(0, "ShowViewLocal success", strBase64);
                return resp;
            }
            else
            {
                f3.Close();
                f3 = null;
                return ShowViewPen(filename);
            }
        }
        #endregion

        #region 在中间的panel中播放图片（本地图片文件），使用graphic 复制的方式
        public Response ShowViewPenInPanel(string filename, int index)
        {
            Log.Info("ShowViewPenInPanel()...");
            int id = Util.toInt(filename);
            string path = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + filename;
            if (id > 0)
            {
                path = fRobotPen.SaveImages(id);
            }
            
            if(fRobotPen!=null)
            {
                fRobotPen.ShowHistoryImage(path, index);
            }
            string strBase64 = Util.ImgToBase64String(path);
            Response resp = new Response(0, "ShowViewLocal success", strBase64);
            return resp;
        }
        #endregion

        
        public string SetClass(int classid)
        {
             List<Classes> classlist = m_db.getClassBySchoolid(Global.getSchoolID());
             for (int i = 0; i < classlist.Count; i++ )
             {
                 Classes c = classlist[i];
                 if (classid == c.id)
                 {
                     Global.setClassname(c.name);
                     Global.setClassID(c.id);
                     if (Global.loadClassInfo())
                     {
                         return "success";
                     }
                     else
                     {
                         return "error";
                     }
                 }
             }
             return "error";
        }
        public string Exit(string accessValue,string chapter)
        {
            SummaryClose();
            Log.Info("EService.Exit() now... lessonid=" + Global.getLessonID());
            Common.setLessonOff(1,accessValue,chapter);//手动下课
            Close();

            //手动下课清除白名单记录列表，为下节课做准备
            Global.stuKQlist = "";
            Global.stuPadall = "";   //清pad交互数据
            Global.stuPadon = "";

            Form1.DeleteFile("*");
            FormDraw.ClearRecord();

            AnswerCard.LsitClear();
            AnswerCard.CardClose();

            return "success";
        }

        #endregion

        public Stream DownloadFileStream(string filename)
        {
            //if (!filemap.ContainsKey(filename))
            //{
            //    return null;
            //}
            //string filepath = filemap[filename];
            
            string filepath = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + filename;
            if(filename=="music.jpg" || filename == "flash.png")
            {
                filepath = Application.StartupPath + "\\" + filename;
            }

            if (!File.Exists(filepath))
            {
                if (!filemap.ContainsKey(filename))
                {
                    return null;
                }
                else
                {
                    filepath = filemap[filename];
                    if (!File.Exists(filepath))
                    {
                        return null;
                    }
                }
            }

            FileInfo fileinfo = new FileInfo(filepath);
            if (File.Exists(filepath))
            {
                var incomingRequest = WebOperationContext.Current.IncomingRequest;
                var outgoingResponse = WebOperationContext.Current.OutgoingResponse;
                long offset = 0, count = fileinfo.Length;

                if (incomingRequest.Headers.AllKeys.Contains("Range"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(incomingRequest.Headers["Range"], @"(?<=bytes\b*=)(\d*)-(\d*)");
                    if (match.Success)
                    {
                        outgoingResponse.StatusCode = System.Net.HttpStatusCode.PartialContent;
                        string v1 = match.Groups[1].Value;
                        string v2 = match.Groups[2].Value;
                        if (!match.NextMatch().Success)
                        {
                            if (v1 == "" && v2 != "")
                            {
                                var r2 = long.Parse(v2);
                                offset = count - r2;
                                count = r2;
                            }
                            else if (v1 != "" && v2 == "")
                            {
                                var r1 = long.Parse(v1);
                                offset = r1;
                                count -= r1;
                            }
                            else if (v1 != "" && v2 != "")
                            {
                                var r1 = long.Parse(v1);
                                var r2 = long.Parse(v2);
                                offset = r1;
                                count -= r2 - r1 + 1;
                            }
                            else
                            {
                                outgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
                            }
                        }
                    }

                }
                outgoingResponse.ContentType = "application/force-download";
                outgoingResponse.ContentLength = count;

                Log.Info("开始下载：" + filename + "(" + offset + "--" + count + ")");
                CusStreamReader fs = new CusStreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read), offset, count);
                fs.Reading += (t) =>
                {
                    //限速代码,实际使用时可以去掉，或者精确控制
                    Thread.Sleep(10);
                    Console.WriteLine(t);
                };
                return fs;
            }
            else
            {
                throw new FaultException("没找到文件：" + filename);
            }

        }
        #region 右键选择文件并同步给小助手
        public string SelectPPT(string filepath)
        {
            Httpd.OnPublicMsg(filepath);
            Log.Info("SelectPPT， filepath=" + filepath);
            string filepath1 = System.Web.HttpUtility.UrlDecode(filepath);
            Log.Info("SelectPPT， filepath1=" + filepath1);
            string name = Path.GetFileName(filepath);
            Form1.SelectPPT(filepath);
            selectFile(name, filepath);
            return "success";
        }
        #endregion


        public static void selectFile(string filename,string filepath,bool bExport=true)
        {
            if (filemap.ContainsKey(filename))
            {
                filemap.Remove(filename);
            }
            filemap.Add(filename, filepath);

            string filetype = Path.GetExtension(filepath);
            if (filetype.IndexOf("ppt") > 0 && bExport)
            {
                Thread th = new Thread(delegate()
                {
                    MyPPT.exportImg(filepath);
                });
                th.Start();
                return;
            }

            string dir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string dstfilepath = dir + Path.GetFileName(filepath) + "_1.jpg";
            if (File.Exists(dstfilepath))
                return;

            string ext = Path.GetExtension(filepath).ToLower();
            if (ext.IndexOf("doc") >= 0)
            {
                Thread th = new Thread(delegate()
                {
                    string pdfname = Path.GetFileNameWithoutExtension(filepath) + ".pdf";
                    string pdfpath = dir + pdfname;
                    try
                    {
                        Util.ConvertWord2PDF(filepath, dir, pdfname);
                        Util.PDFThumbImage(pdfpath, dir, Path.GetFileName(filepath));//, 0, 1, null, 0
                    }
                    catch (Exception e)
                    {

                    }
                });
                th.Start();
            }
            else if (ext.IndexOf("pdf") >= 0)
            {
                Thread th = new Thread(delegate()
                {
                    Util.PDFThumbImage(filepath, Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd"), Path.GetFileName(filepath));
                });
                th.Start();
            }
            else if (ext.IndexOf("wmv") >= 0 || ext.IndexOf("mp4") >= 0 || ext.IndexOf("mov") >= 0)
            {
                Thread th = new Thread(delegate()
                {
                    string imgpath = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + Path.GetFileName(filepath) + "_1.jpg";
                    Util.MakeThumbNailFromVideo(filepath, imgpath, 200,150);
                });
                th.Start();
            }
        }
        public string DeleteFile(string filename)
        {
            Form1.DeleteFile(filename);
            return "success";
        }
        public static string getFilepath(string filename)
        {
            if (filemap.ContainsKey(filename))
            {
                string path = filemap[filename];
                if (path.IndexOf("#") > 0)
                    path = path.Substring(path.IndexOf("#") + 1);
                //if(path.IndexOf(filename)<0)
                //    path += "\\"+filename;
                return path;
            }else if(filename == "music.jpg"){
                string path = Application.StartupPath + "\\music.jpg";
                return path;
            }
            return "";
        }
        private string getClassInfo()
        {
            return Global.g_ClassInfoStr;
        }

        

        //------------------------举手提问、出题、奖励和批评-------------------------
        #region 提问（举手）的发起
        public Response Handon(int courseid = 0, int lessonid = 0)
        {
            Httpd.cancelHandon();
            Common.ClearHandon();
            Global.checkLesson(courseid, lessonid);

            if (fHandon == null)
            {
                Log.Info("Handon. clear First.");

                fHandon = new FormHandon(getClassInfo());//handon
                fHandon.Hide();
                Global.panelshow = 1;

                AnswerCard.RaiseStart();   //启动答题卡举手

            }
            else
            {
                //“换个问题” pad界面并未返回，直接重新发起举手
                HandonOver("-2", "");
                //Handon(courseid, lessonid);
                AnswerCard.RaiseStart();
            }
            Response resp = new Response(0, "Handon success", "");
            return resp;
        }
        public Response HandonClose()
        {
            if (fHandon != null)
            {
                Log.Info("HandonClose. clear First.");

                string handon = fHandon.StopT();
                //string handon = fHandon.GetResult();
                string callnameStr = fHandon.GetCallname();
                string rewardStr = fHandon.GetRewarded();
                string criticizeStr = fHandon.GetCriticize();
                string handonRepeated = fHandon.getHandonRepeated();
                string createtime = fHandon.tm_create.ToString("yyyyMMddHmmss");
                int timediff = Util.getTimeDiff_Second(fHandon.tm_create);
                string rid = fHandon._xitiId;
                Httpd.setHandon("");
                if (handon.Length > 0 || callnameStr.Length > 0)
                {
                    IntelligentRecommend.AddHandon(handon);//Handon
                    Common.uploadHandon(rid, createtime, handon, callnameStr, rewardStr, criticizeStr, handonRepeated, timediff);//同步到本地服务和云服务器
                }
                if (f10 != null)
                {
                    f10.Close();
                    f10 = null;
                }

                fHandon.Close();
                fHandon = null;
            }
            Response resp = new Response(0, "HandonClose ok", "");
            return resp;
        }
        public Response HandonSwitchView(int index)
        {
            if (fHandon != null)
            {
                Log.Info("HandonSwitchView. index=" + index);

                fHandon.SwitchView(index);
            }
            Response resp = new Response(0, "HandonSwitchView success", "");
            return resp;
        }

        #endregion

        #region [v1.5]获取提问推荐名单
        public string getRecommendStudent()
        {
            string str = IntelligentRecommend.getRecommend();
            Log.Info(str);
            return str;
        }
        #endregion

        #region 提问、出题的点名和提问的关闭
        public string HandonOver(string id, string name)
        {
            //http://localhost:8986/HandonOver?id=0&name=
            Log.Info("HandonOver()...id=" + id + ", name=" + name);
            int nId = Util.toInt(id);
            int _uid = nId;
            if (nId < 100 && _uid > 0)
                _uid = Global.getUidBySeat(nId);

            if (fHandon != null)
            {
                if (nId < 0)
                {
                    string handon = fHandon.StopT();
                    string callnameStr = fHandon.GetCallname();
                    string rewardStr = fHandon.GetRewarded();
                    string criticizeStr = fHandon.GetCriticize();
                    string handonRepeated = fHandon.getHandonRepeated();
                    string createtime = fHandon.tm_create.ToString("yyyyMMddHmmss");
                    int timediff = Util.getTimeDiff_Second(fHandon.tm_create);
                    string rid = fHandon._xitiId;
                    Httpd.setHandon("");
                    if (handon.Length > 0 || callnameStr.Length > 0)
                    {
                        IntelligentRecommend.AddHandon(handon);//Handon
                        Common.uploadHandon(rid, createtime, handon, callnameStr, rewardStr, criticizeStr, handonRepeated, timediff);//同步到本地服务和云服务器
                    }
                    if (f10 != null)
                    {
                        f10.Close();
                        f10 = null;
                    }

                    if (nId == -1)//关闭
                    {
                        fHandon.Close();
                        fHandon = null;
                        Httpd.cancelHandon();

                        AnswerCard.AnswerStop();
                    }
                    else if (nId == -2)//刷新
                    {
                        if (!fHandon.restart())
                        {
                            fHandon = null;
                            Handon();
                        }
                    }

                }//end_if(nId==-1)
                else if (nId == 0)
                {
                    //随机点名
                    if (name.Length == 0)
                    {
                        List<User> ulist = m_db.getStudentlist(Global.getClassID() + "");
                        if (ulist != null && ulist.Count > 0)
                        {
                            Random rd = new Random();
                            User u = ulist[rd.Next(0, ulist.Count)];
                            nId = u.id;
                            name = u.name;
                            _uid = Global.getUidBySeat(nId);
                            fHandon.AppendCallname(_uid);
                            f10 = new Form10(getClassInfo(), "", _uid, name);//callname
                            f10.Show();
                            //fHandon.Hide();
                            //fHandon.Show();
                            //fHandon.BringToFront();
                        }
                    }
                    else
                    {
                        nId = 0;
                        f10 = new Form10(getClassInfo(), "", _uid, name);//callname
                        f10.Show();
                        fHandon.AppendCallname(_uid);
                    }

                }//end_if(nId==0)
                else
                {
                    fHandon.AppendCallname(_uid);
                    f10 = new Form10(getClassInfo(), "", _uid, name);//callname
                    f10.Show();
                }
            }else if(nId==-1)
            {
                Httpd.cancelHandon();
            }
            else
            {
                Log.Info("just callname...");
                if (f10 != null)
                {
                    f10.Close();
                    f10 = null;
                }
                if (nId >= 0 && name.Length > 0)
                {
                    f10 = new Form10(getClassInfo(), "", nId, name);//callname
                    f10.Show();


                    if (f2 != null)
                    {
                        f2.AppendCallname(_uid);
                    }
                    else if (fPPTPractise != null)
                    {
                        fPPTPractise.AppendCallname(_uid);
                    }
                    //Common.uploadCallname(nId + "");//做题窗口
                }

            }
            return "success";
        }

        #endregion

        #region 题库出题（Form2）
        public string Projective(string courseid, string id, string rid,string classid,string lessonid)
        {
            Common.GetXitiResult(true);//clear,Projective

            Log.Info("Projective() courseid=" + courseid + ", id=" + id + ", rid=" + rid + ", classid=" + classid + ", lessonid=" + lessonid);
            if (fStatistics != null)
            {
                fStatistics.StopAnimate();
                fStatistics.Close();
                fStatistics = null;
                AnswerCard.AnswerStart();   //启动答题卡答题
            }
            if (f2 == null)
            {
                //从云端获取习题内容
                string data = "id=" + id + "&rid=" + rid;
                string questionstr = Common.doPost("getXitiBody", data);

                f2 = new Form2(questionstr, rid, getClassInfo(), Global.getClassID()+"", lessonid);//xiti
                f2.Show();

                AnswerCard.AnswerStart();   //启动答题卡答题
            }
            else
            {
                f2.t.Enabled = false;
                f2.t = null;
                f2.Close();
                f2 = null;
                Projective(courseid, id, rid, classid, lessonid);
            }
            return "success";
        }
        
        public string HidePanel(int hide)
        {
            Log.Info("HidePanel()");
            if (fHandon != null)
            {
                if (hide == 1)
                {
                    fHandon.Hide();
                    Global.panelshow = 0;
                }
                else
                {
                    fHandon.Show();
                    Global.panelshow = 1;
                }
            }else if (f2 != null)
            {
                if (hide == 1)
                {
                    f2.Hide();
                    Global.panelshow = 0;
                }
                else
                {
                    f2.Show();
                    Global.panelshow = 1;
                }
            }else if (fPPTPractise != null)
            {
                if (hide == 1)
                {
                    fPPTPractise.Hide();
                    Global.panelshow = 0;
                }
                else
                {
                    fPPTPractise.Show();
                    Global.panelshow = 1;
                }
            }
            return "success";
        }

        public string CloseProjective()
        {
            Log.Info("CloseProjective()");
            AnswerCard.AnswerStop();       //退出答题答题
            string result="",id="",answer="",createtime="";
            string callname = "", reward = "", criticize = "";
            int timeuse=0;
            if (f2 != null)
            {
                f2.t.Enabled = false;
                result = f2.getResult();
                id = f2._id;
                answer = f2.m_answer;
                createtime = f2.tm_create.ToString("yyyyMMddHHmmss");
                timeuse = Util.getTimeDiff_Second(f2.tm_create);

                callname = f2.GetCallname();
                reward = f2.GetRewarded();
                criticize = f2.GetCriticized();
                f2.Close();
                f2 = null;
            }
            if (fPPTPractise != null)
            {
                fPPTPractise.t.Enabled = false;
                Log.Info("CloseProjective()_fPPTPractise.Close....0");

                result = fPPTPractise.getResult();
                id = fPPTPractise._xitiId;
                answer = fPPTPractise._answer;
                createtime = fPPTPractise.tm_create.ToString("yyyyMMddHHmmss");
                timeuse = Util.getTimeDiff_Second(fPPTPractise.tm_create);

                Log.Info("CloseProjective()_fPPTPractise.Close....1");

                callname = fPPTPractise.GetCallname();
                reward = fPPTPractise.GetRewarded();
                criticize = fPPTPractise.GetCriticized();

                fPPTPractise.Close();
                Global.panelshow = 0;
                fPPTPractise = null;
                Log.Info("CloseProjective()_fPPTPractise.Close....2");
            }
            //将习题结果同步到服务器
            if(result.Length>0)
            {
                Log.Info("CloseProjective()_fPPTPractise.Close....3");
                XitiResult xitiResult = new XitiResult(id, answer, result, callname, reward, criticize, createtime, timeuse);
                Log.Info("CloseProjective()_fPPTPractise.Close....4");
                IntelligentRecommend.addXitiResult(id, answer, result);
                Log.Info("CloseProjective()_fPPTPractise.Close....5");
                Common.uploadXitiResult(xitiResult);
            }
            Log.Info("CloseProjective()_fPPTPractise.Close....6");
            Httpd.clearPracticeResult();//清除结果
            Log.Info("return success now...");
            return "success";
        }
        #endregion

        #region 截屏出题（FormPPTPractise）
        public string ProjectiveInPPT(string courseid, string classid, string lessonid)
        {
            Global.panelshow = 1;
            Log.Info("ProjectiveInPPT() courseid=" + courseid + ", classid=" + classid + ", lessonid=" + lessonid);
            if (fPPTPractise == null)
            {
                Common.ClearXiti();

                fPPTPractise = new FormPPTPractise(getClassInfo(), classid, lessonid);
                fPPTPractise.Hide();

                AnswerCard.AnswerStart();   //启动答题卡答题
            }
            else
            {
                CloseProjective();
                //fPPTPractise.t.Enabled = false;
                //fPPTPractise.t = null;
                //fPPTPractise.Close();
                //fPPTPractise = null;
                ProjectiveInPPT(courseid, classid, lessonid);

                AnswerCard.AnswerStart();
            }
            return "success";
        }
        #endregion

        #region 截屏出题判断题型（FormPPTPractise）
        public string JudgeProjectiveInPPT(string courseid, string classid, string lessonid)
        {
            Global.panelshow = 1;
            Log.Info("ProjectiveInPPT() courseid=" + courseid + ", classid=" + classid + ", lessonid=" + lessonid);
            if (fPPTPractise == null)
            {
                Common.ClearXiti();

                fPPTPractise = new FormPPTPractise(getClassInfo(), classid, lessonid);
                fPPTPractise.Hide();

                AnswerCard.JudgeAnswerStart();   //启动答题卡答题
            }
            else
            {
                CloseProjective();
                //fPPTPractise.t.Enabled = false;
                //fPPTPractise.t = null;
                //fPPTPractise.Close();
                //fPPTPractise = null;
                ProjectiveInPPT(courseid, classid, lessonid);

                AnswerCard.JudgeAnswerStart();
            }
            return "success";
        }
        #endregion

        #region 出题 设置答案（Form2，FormPPTPractise）
        public string SetAnswer(string answer)
        {
            Log.Info("setAnswer() answer=" + answer);
            if (f2 != null)
            {
                f2.setAnswer(answer.ToUpper());
            }else if (fPPTPractise != null)
            {
                fPPTPractise.setAnswer(answer.ToUpper());
            }
            return "success";
        }
        #endregion

        #region [v1.5]停止答题
        public string StopXiti()
        {
            string result = "";
            string createtime = "";
            DateTime dt = DateTime.Now;
            Global.panelshow = 1;
            if (f2 != null)
            {
                f2.t.Enabled = false; 
                f2.SwitchView(2);
            }
            if (fPPTPractise != null)
            {
                fPPTPractise.t.Enabled = false; 
                fPPTPractise.SwitchView(2);
                AnswerCard.AnswerStop();
            }
            return "success";
        }
        #endregion
        #region [v1.5]停止答题
        public string StopJugeXiti()
        {
            string result = "";
            string createtime = "";
            DateTime dt = DateTime.Now;
            Global.panelshow = 1;
            if (fPPTPractise != null)
            {
                fPPTPractise.t.Enabled = false;
                fPPTPractise.SwitchView(3);
                AnswerCard.AnswerStop();
            }
            return "success";
        }
        #endregion

        #region  [v2.0]进入生生互动
        public string EnterInter(string courseid, string classid, string lessonid, string stuName)
        {

            if (fInter == null)
            {
                Common.ClearXiti();

                fInter = new FormInter(getClassInfo(), classid, lessonid, stuName);
                fInter.Show();
            }
            else
            {
                ExitInter();
                //fPPTPractise.t.Enabled = false;
                //fPPTPractise.t = null;
                //fPPTPractise.Close();
                //fPPTPractise = null;
                EnterInter(courseid, classid, lessonid, stuName);
            }
            return "success";
        }
        #endregion

        #region [v2.0]退出生生互动
        public string ExitInter()
        {
            string result = "", id = "", answer = "", createtime = "";
            string callname = "", reward = "", criticize = "";
            /// int timeuse = 0;
            if (fInter != null)
            {
                fInter.t.Enabled = false;

                result = fInter.getResult();
                id = fInter._xitiId;
                answer = fInter._answer;
                createtime = fInter.tm_create.ToString("yyyyMMddHHmmss");
               /// timeuse = Util.getTimeDiff_Second(fPPTPractise.tm_create);

                callname = fInter.GetCallname();
                reward = fInter.GetRewarded();
                criticize = fInter.GetCriticized();

                fInter.Close();
                fInter = null;
            }
            return "success";
        }
        #endregion

        #region  [v2.0]进入分组竞赛
        public string EnterCompete(int num, string allScore,string Rank)
        {

            if (fCp == null)
            {
                fCp = new FormCompete(num, allScore,Rank);
                fCp.Show();
            }
            else
            {
                ExitCompete();
                EnterCompete(num,allScore,Rank);
            }
            return "success";
        }
        #endregion

        #region [v2.0]退出分组竞赛
        public string ExitCompete()
        {
            if (fCp != null)
            {
                fCp.Close();
                fCp = null;
            }
            return "success";
        }
        #endregion


        #region  [v2.0]分组竞赛得分
        public string EnterScore(string groupnum, string scorenum)
        {

            if (fScore == null)
            {
                //Common.ClearXiti();

                fScore = new FormScore(groupnum, scorenum);
                //fScore.Show();
            }
            else {
                fScore.SetPanel(groupnum, scorenum);
            }
            return "success";
        }
        #endregion




        #region [v1.6]投票答题
        public string VoteStart(string options)
        {
            Common.ClearXiti();

            if (fVote != null)
            {
                VoteClose();
            }
            fVote = new FormVote(options);
            AnswerCard.AnswerSingleStart();   //启动答题卡答题
            return "success";
        }
        public string VoteStop()
        {
            if (fVote != null)
            {
                fVote.t.Enabled = false;
            }
            return "success";
        }
        public string VoteClose()
        {
            if (fVote != null)
            {
                fVote.t.Enabled = false;
                fVote.Dispose();
                fVote = null;
                AnswerCard.AnswerStop();
            }
            return "success";
        }
        #endregion

        #region [v1.6]精彩两分钟
        public string TwoMinuteStart(string options)
        {
            Common.ClearXiti();

            if (fTMin != null)
            {
                TwoMinuteClose();
            }
            fTMin = new FormTwoMinute(options);
            AnswerCard.AnswerSingleStart();   //启动答题卡答题
            return "success";
        }
        public string TwoMinuteStop()
        {
            if (fTMin != null)
            {
                fTMin.t.Enabled = false;
            }
            return "success";
        }
        public string TwoMinuteUpInfo(string optTea,string optStu)
        {
            if (fTMin != null)
            {
                fTMin.upInfo(optTea, optStu);
            }
            return "success";
        }
        public string TwoMinuteClose()
        {
            if (fTMin != null)
            {
                fTMin.t.Enabled = false;
                fTMin.Dispose();
                fTMin = null;
                AnswerCard.AnswerStop();
            }
            return "success";
        }
        #endregion


        #region [v1.6]抢答
        public string QDStart()
        {
            if (fQiangDa != null)
            {
                 QDClose();
            }
            Common.ClearHandon(false);//QDStart
            fQiangDa = new FormQiangDa();

            AnswerCard.RaiseStart();

            return "success";
        }
        public string QDResult(string answer)
        {
            if (fQiangDa != null)
            {
                //fQiangDa.setResult(answer);
            }
            return "success";
        }
        public string QDClose()
        {
            if (fQiangDa != null)
            {
                if(fQiangDa.t != null){
                    fQiangDa.t.Enabled = false;
                }
                Httpd.cancelHandon();
                fQiangDa.Dispose();
                fQiangDa = null;

                AnswerCard.AnswerStop();
            }
            return "success";
        }

        #endregion

        #region [v1.3-1.4]切换答题结果界面
        public Response XitiSwitchView(int index)
        {
            if (f2 != null)
            {
                Log.Info("HandonSwitchView. index=" + index);
                f2.SwitchView(index);
            }
            else if (fPPTPractise != null)
            {
                Log.Info("HandonSwitchView. index=" + index);
                fPPTPractise.SwitchView(index);
            }
            Response resp = new Response(0, "XitiSwitchView success", "");
            return resp;
        }
#endregion

        #region [v1.0(cancled)]习题结果统计展示

        public string Statistics(string id, string type, string answer)
        {
            Log.Info("Statistics()_1, id=" + id + ", type=" + type + ", answer=" + answer + ", Form2.enable & Form8.open|show");
            g_answer = answer;
            string result = "";
            string createtime = "";
            DateTime dt = DateTime.Now;
            if (f2 != null)
            {
                f2.t.Enabled = false;
                result = f2.getResult();
                dt = f2.tm_create;
                createtime = f2.tm_create.ToString("yyyyMMddHHmmss");
            }
            if (fPPTPractise != null)
            {
                fPPTPractise.t.Enabled = false;
                id = fPPTPractise._xitiId;
                type = "0";
                result = fPPTPractise.getResult();
                dt = fPPTPractise.tm_create;
                createtime = fPPTPractise.tm_create.ToString("yyyyMMddHHmmss");
            }

            if (fStatistics != null)
            {
                Log.Info("Statistics(), FormStatistics.close.");
                fStatistics.Close();
                fStatistics = null;
            }
            fStatistics = new FormStatistics(id, type, answer, getClassInfo(), result);
            Log.Info("Statistics()_2, FormStatistics, result=" + result);
            fStatistics.Show();
            int timediff = Util.getTimeDiff_Second(dt);
            XitiResult xitiResult = new XitiResult(id, answer, result, "","","",createtime, timediff);
            IntelligentRecommend.addXitiResult(id,answer,result);
			return result;
        }

        public string SwitchAnswer(string selectAnswer)
        {
            Log.Info("SwitchAnswer() selectAnswer=" + selectAnswer + ", Form8.SetPanel now...");
            if (fStatistics != null)
            {
                fStatistics.panel2.Controls.Clear();
                fStatistics.SetPanel(getClassInfo(), selectAnswer);
                fStatistics.SetItem(g_answer, selectAnswer);
            }
            return "success";
        }

        /// <summary>
        /// 关闭习题作答的统计结果页面，如果在PPT播放过程中做题，则还需要关闭学号作答窗口
        /// </summary>
        /// <returns></returns>
        public string CloseStatistics()
        {
            Log.Info("CloseStatistics(), Form8.close()");

            if (fStatistics != null)
            {
                fStatistics.Close();
                fStatistics = null;
            }
            if (fPPTPractise != null)
            {
                fPPTPractise.Close();
                fPPTPractise = null;
            }

            return "success";
        }
        #endregion

        //------------------------奖励-------------------------
        #region [v1.4] 奖励
        public string Reward(string reason, string reasonid)
        {
            Log.Info("点名_Reward()_Form10.RewardShow()");
            if (f10 != null && f10.STName != null)
            {
                f10.RewardShow();
                f10.Text = "奖励";
                string strUid = f10.STID;
                if (fHandon != null)
                {
                    fHandon.AppendReward(strUid,0,reason,reasonid);
                }
                else if (fPPTPractise != null)
                {
                    fPPTPractise.AppendReward(strUid, 0, reason,reasonid);
                }
                else if (f2 != null)
                {
                    f2.AppendReward(strUid, 0, reason, reasonid);
                }
                //Common.uploadReward(strUid);//做题窗口
            }
            return "success";
        }
        
#endregion
        #region [v1.5] 奖励
        public string Award(int point, int uid, string name, string reason, string reasonid)
        {
            if (fAward != null)
            {
                fAward.Close();
                fAward = null;
            }
            if(reason==null)
            {
                reason = "表现不错";
                reasonid = "1";
            }

            IntelligentRecommend.addAward(uid, point, reason, reasonid);

            fAward = new FormAward(uid, name, point,reason,reasonid);//callname
            fAward.RewardShow();
            Log.Info("fAward.Show");


            if (fHandon != null)
            {
                fHandon.AppendReward(uid + "", point, reason, reasonid);
            }
            else if (fPPTPractise != null)
            {
                fPPTPractise.AppendReward(uid + "", point, reason, reasonid);
            }
            else if (f2 != null)
            {
                f2.AppendReward(uid + "", point, reason, reasonid);
            }
            //else if(fRobotPen.Visible==true)
            //{
            //    fRobotPen.AppendReward(uid + "", point, reason, reasonid);
            //}
            return "success";
        }
        #endregion

        #region [v1.3-1.4] 批评
        public string Criticize(int id)
        {
            Log.Info("Criticize id=" + id);
            //Common.uploadReward(id+"",-1);//批评

            int uid = id;
            if (id < 100)
                uid = Global.getUidBySeat(id);

            if (fHandon != null)
            {
                fHandon.AppendCriticize(uid);//举手窗口
            }
            else if (fPPTPractise != null)
            {
                fPPTPractise.AppendCriticize(uid);
            }
            else if (f2 != null)
            {
                f2.AppendCriticize(uid);
            }
            //Common.uploadReward(uid);//做题窗口
            return "success";
        }
        #endregion

        //------------------------摄像头拍照-------------------------
        #region 摄像头初始化、拍照（FormCamera）
        public string InitCamera()
        {
            Log.Info("InitCamera()...");
            string path = Application.StartupPath + "\\OpenNetStream.dll";
            if (fCamera == null && Global.isWithCamera() && File.Exists(path))
            {
                try
                {
                    fCamera = new FormCamera();
                    fCamera.Hide();

                    bool bLogin = fCamera.login();
                    if (!bLogin)
                    {
                        fCamera.Close();
                        fCamera = null;
                        return "error";
                    }
                    fCamera.getDeviceList();
                    fCamera.Start_Play(1);
                }
                catch (Exception ex) { }
            }
            return "success";
        }
        public Response TakePicture(string callback)
        {
            Log.Info("TakePicture()...");
            if (fCamera == null)
            {
                Response resp = new Response(0, "TakePicture err", "fCamera is null");
                return resp;
            }
            string ymd = DateTime.Now.ToString("yyyyMMdd");
            string dir = Application.StartupPath + "\\" + ymd;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string path = fCamera.CaptureView();
            if(path.Length > 0)
            {
                if (f3 != null)
                {
                    f3.Close();
                    f3 = null;
                }
                try
                {
                    f3 = new Form3(path, 3, 0);
                    f3.ShowAndHide();

                    string filepath = Application.StartupPath + "\\" + path;
                    string strBase64 = Util.ImgToBase64String(filepath);
                    Response resp = new Response(0, "TakePicture success", strBase64);
                    return resp;
                }
                catch (Exception ex)
                {
                    Response resp = new Response(0, "TakePicture Exception", ex.Message);
                    return resp;
                }
            }
            else
            {
                Response resp = new Response(0, "TakePicture failed", "");
                return resp;
            }

        }

        #endregion

        //------------------------图片、画笔-------------------------
        #region 图片播放（Pad拍照，Form3）
        public string ShowView(Stream stream)
        {
            Log.Info("ShowView()...Form3");
            if (f3 == null)
            {
                try
                {
                    StreamReader sr = new StreamReader(stream);
                    string s = sr.ReadToEnd();
                    string s2 = s;
                    int pos2 = s.IndexOf("name=\"datas\"");
                    if(pos2 > 0)
                    {
                        s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "") ;
                        int pos3 = s2.IndexOf("------WebKitForm");
                        s2 = s2.Substring(0, pos3);
                        s2 = "datas=" + s2;
                    }
                    //sr.Dispose();
                    NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                    f3 = new Form3(nvc["datas"], 1, 1);
                    f3.Show();
                    bShowPicture = true;
                }
                catch (Exception ex) { }
            }
            else
            {
                f3.Close();
                f3 = null;
                ShowView(stream);
            }
            return "success";
        }
        public string ShowViewAgain(Stream stream)
        {
            Log.Info("ShowView()...Form3");
            if (f3 == null)
            {
                try
                {
                    StreamReader sr = new StreamReader(stream);
                    string s = sr.ReadToEnd();
                    string s2 = s;
                    int pos2 = s.IndexOf("name=\"datas\"");
                    if (pos2 > 0)
                    {
                        s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "");
                        int pos3 = s2.IndexOf("------WebKitForm");
                        s2 = s2.Substring(0, pos3);
                        s2 = "datas=" + s2;
                    }
                    //sr.Dispose();
                    NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                    f3 = new Form3(nvc["datas"], 1, 0);
                    f3.Show();
                    bShowPicture = true;
                }
                catch (Exception ex) { }
            }
            else
            {
                f3.Close();
                f3 = null;
                ShowViewAgain(stream);
            }
            return "success";
        }
        #endregion


        #region 图片缩小放大（4张对比，Form3）
        public string Zoom(double ratio,double ratioX,double ratioY)
        {
            Log.Info("Zoom()...");
            if (f3 != null)
            {
                Log.Info("Zoom()...Form3, ratio=" + ratio+" ratioX="+ratioX+", ratioY="+ratioY);
                f3.Zoom(ratio,ratioX,ratioY);
            }else if (fCaptureScreen != null){
                fCaptureScreen.Zoom(ratio,ratioX,ratioY);
            }else if(f3_4!=null){
                Log.Info("Zoom()...Form3_4, ratio=" + ratio);
                f3_4.Zoom(ratio, ratioX, ratioY);
            }
            return "success";
        }
        #endregion

        #region 图片播放（4张对比，Form3）
        public Response ShowView4_1(Stream stream)
        {
            Log.Info("ShowView4_1()");
            if (f3_4 == null)
            {
                f3_4 = new FormPicture4();
            }

            try
            {
                StreamReader sr = new StreamReader(stream);
                string s = sr.ReadToEnd();
                string s2 = s;
                int pos2 = s.IndexOf("name=\"datas\"");
                if (pos2 > 0)
                {
                    s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "");
                    int pos3 = s2.IndexOf("------WebKitForm");
                    s2 = s2.Substring(0, pos3);
                    s2 = "datas=" + s2;
                }
                //sr.Dispose();
                NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                string datas= nvc["datas"];
                f3_4.addPicture(1,datas);
                f3_4.Show();

                bShowPicture = true;
            }
            catch (Exception ex) {

            }
            Response resp = new Response(0, "ShowView4 success", "");
            return resp;
        }
        public Response ShowView4_2(Stream stream)
        {
            Log.Info("ShowView4_2()");
            if (f3_4 == null)
            {
                return new Response(0, "ShowView4 error", "");
            }

            try
            {
                StreamReader sr = new StreamReader(stream);
                string s = sr.ReadToEnd();
                string s2 = s;
                int pos2 = s.IndexOf("name=\"datas\"");
                if (pos2 > 0)
                {
                    s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "");
                    int pos3 = s2.IndexOf("------WebKitForm");
                    s2 = s2.Substring(0, pos3);
                    s2 = "datas=" + s2;
                }
                //sr.Dispose();
                NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                string datas = nvc["datas"];
                f3_4.addPicture(2, datas);
                f3_4.Show();

                bShowPicture = true;
            }
            catch (Exception ex)
            {

            }
            Response resp = new Response(0, "ShowView4 success", "");
            return resp;
        }
        public Response ShowView4_3(Stream stream)
        {
            Log.Info("ShowView4_3()");
            if (f3_4 == null)
            {
                return new Response(0, "ShowView4 error", "");
            }

            try
            {
                StreamReader sr = new StreamReader(stream);
                string s = sr.ReadToEnd();
                string s2 = s;
                int pos2 = s.IndexOf("name=\"datas\"");
                if (pos2 > 0)
                {
                    s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "");
                    int pos3 = s2.IndexOf("------WebKitForm");
                    s2 = s2.Substring(0, pos3);
                    s2 = "datas=" + s2;
                }
                //sr.Dispose();
                NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                string datas = nvc["datas"];
                f3_4.addPicture(3, datas);
                f3_4.Show();

                bShowPicture = true;
            }
            catch (Exception ex)
            {

            }
            Response resp = new Response(0, "ShowView4 success", "");
            return resp;
        }
        public Response ShowView4_4(Stream stream)
        {
            Log.Info("ShowView4_4()");
            if (f3_4 == null)
            {
                return new Response(0, "ShowView4 error", "");
            }

            try
            {
                StreamReader sr = new StreamReader(stream);
                string s = sr.ReadToEnd();
                string s2 = s;
                int pos2 = s.IndexOf("name=\"datas\"");
                if (pos2 > 0)
                {
                    s2 = s.Substring(pos2 + "name=\"datas\"".Length).Replace("\r\n", "");
                    int pos3 = s2.IndexOf("------WebKitForm");
                    s2 = s2.Substring(0, pos3);
                    s2 = "datas=" + s2;
                }
                //sr.Dispose();
                NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                string datas = nvc["datas"];
                f3_4.addPicture(4, datas);
                f3_4.Show();
                bShowPicture = true;
            }
            catch (Exception ex)
            {

            }
            Response resp = new Response(0, "ShowView4 success", "");
            return resp;
        }
        public string ShowView4_switch(int index)
        {
            Log.Info("ShowView4_switch()...");
            if (f3_4 != null)
            {
                f3_4.showPicture(index);
            }

            return "success";
        }
        public string CloseView4()
        {
            Log.Info("CloseView4()...");
            if (f3_4 != null)
            {
                //PhotoCompare
                //TODO: 截屏上传
                if (f3_4.index > 0)
                {
                    //创建习题ID
                    Image img = ScreenCapture.captureScreen(0, 0);
                    string imgName = "C_"+Global.getSchoolID() + "-" + Global.getClassID() + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                    string imgDir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
                    if (!Directory.Exists(imgDir))
                        Directory.CreateDirectory(imgDir);

                    string imgPath = imgDir + "\\" + imgName;
                    img.Save(imgPath);
                    Common.uploadPicture(imgPath);
                    Common.uploadPhotoCompare(imgName);//相机拍照
                }
                f3_4.Close();
                f3_4 = null;
            }
            return "success";
        }
        public string EmptyView4()
        {
            Log.Info("ClearView4()...");
            if (f3_4 != null)
            {
                f3_4.EmptyView();
            }
            return "success";
        }
        #endregion


        #region 图片播放（播放Doc/PPT/PDF截屏，FormCaptureScreen）
        public Response ShowView_Xiti()
        {
            Log.Info("ShowView()...Form3");
            if (fCaptureScreen == null)
            {
                try
                {
                    fCaptureScreen = new FormCaptureScreen();
                    fCaptureScreen.Show();
                    bShowPicture = true;
                }
                catch (Exception ex)
                {

                }
                string strBase64 = fCaptureScreen.GetImageBase64();
                Response resp = new Response(0, "ShowViewLocal success", strBase64);
                return resp;
            }
            else
            {
                fCaptureScreen.Close();
                fCaptureScreen = null;
                return ShowView_Xiti();
            }
        }
        public Response ShowView_PPT()
        {
            Log.Info("ShowView_PPT()...");
            if (fCaptureScreen == null)
            {
                try
                {
                    fCaptureScreen = new FormCaptureScreen();
                    fCaptureScreen.Show();
                    bShowPicture = true;
                }
                catch (Exception ex)
                {

                }
                string strBase64 = fCaptureScreen.GetImageBase64();
                Response resp = new Response(0, "ShowViewLocal success", strBase64);
                return resp;
            }
            else
            {
                fCaptureScreen.Close();
                fCaptureScreen = null;
                return ShowView_PPT();
            }
        }

        #endregion

        #region 图片旋转（Form3）
        public string RotateView(int right)
        {
            Log.Info("RotateView()...");
            if (f3 != null)
            {
                Log.Info("RotateView()...Form3, right=" + right);
                f3.Rotate(right);
            }else if(f3_4!=null){
                Log.Info("RotateView()...Form3_4, right=" + right);
                f3_4.Rotate(right);
            }
            return "success";
        }
        #endregion

        #region 图片播放（本地图片文件）
        public Response ShowViewLocal(string callback,string filename)
        {
            Log.Info("ShowViewLocal()...");

            string path = getFilepath(filename);

            if (f3 == null)
            {
                try
                {
                    f3 = new Form3(path, 2, 0);
                    f3.Show();
                    bShowPicture = true;
                    //f3.Zoom(1.2,0.2,0.3);
                }
                catch (Exception ex) { 

                }
                Common.uploadFileopenEvent(filename);
                string strBase64 = Util.ImgToBase64String(path);
                Response resp = new Response(0, "ShowViewLocal success", strBase64);
                return resp;
            }
            else
            {
                f3.Close();
                f3 = null;
                return ShowViewLocal(callback, filename);
            }

            //return "success";
        }
        #endregion



        #region 微信图片播放
        public Response ShowViewWeixin(string callback, string filename)
        {
            Log.Info("ShowViewWeixin()...");
            FormDraw.DrawWeixinImg(filename);
            string path = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd") + @"\Copy" + filename;
            if (f3 == null)
            {
                try
                {
                    f3 = new Form3(path, 2, 0);
                    f3.Show();
                    bShowPicture = true;
                    //f3.Zoom(1.2,0.2,0.3);
                }
                catch (Exception ex)
                {

                }
                Common.uploadFileopenEvent(filename);
                string strBase64 = Util.ImgToBase64String(path);
                Response resp = new Response(0, "ShowViewLocal success", strBase64);
                return resp;
            }
            else
            {
                f3.Close();
                f3 = null;
                return ShowViewWeixin(callback, filename);
            }

            //return "success";
        }
        #endregion


        #region 画笔
        public string DrawView(string perX, string perY, int mode, string color, int width)
        {
            int w = width;
            //if(width==1)
            //    w = 5;
            //else if(width==2)
            //    w=10;
            //else
            //    w=12;

            if (f3 != null)
            {
                f3.DrawLine(perX, perY, mode, "#" + color, w);
            }
            else if (fCaptureScreen != null)
            {
                fCaptureScreen.DrawLine(perX, perY, mode, "#" + color, w);
            }
            else if(f3_4 != null)
            {
                f3_4.DrawLine(perX, perY, mode, "#" + color, w);
            }
            return "success";
        }

        public string ClearView()
        {
            Log.Info("ClearView()...Form3");
            if (f3 != null)
            {
                f3.ClearView();
            }
            if (f3_4 != null)
            {
                f3_4.ClearView();
            }
            else if (fCaptureScreen != null)
            {
                fCaptureScreen.ClearView();
            }
            return "success";
        }

        public string CloseView()
        {
            Log.Info("CloseView()...Form3");
            if (f3 != null)
            {
                f3.CloseView();
                f3 = null;
                bShowPicture = false;
            }
            else if (fCaptureScreen != null)
            {
                fCaptureScreen.CloseView();
                fCaptureScreen = null;
                bShowPicture = false;
            }
            return "success";
        }
        #endregion

        //------------------------音视频播放-------------------------
        #region 播放flash文件
        public string PlayFlash(string url)
        {
            IntelligentRecommend.m_summary.AddResource(url);//PlayFlash
            Log.Info("PlayFlash()...Form4");
            string cmd = "";
            string lastfilename = "";

            string filepath = "";
            if(url.IndexOf("flv") > 0 || url.IndexOf("swf")>0)
            {
                filepath = filemap[url];
                cmd = "fopen";
            }
            else if (url.StartsWith("http"))
            {
                filepath = url;
            }
            if (cmd=="fopen")
            {
                if (f4 == null)
                {
                    try
                    {
                        f4 = new Form4(filepath);
                        f4.Show();
                    }
                    catch (Exception ex) {
                        string msg = ex.Message;
                        if(msg.IndexOf("没有注册类")>=0)
                        {
                            MessageBox.Show("打开Flash文件失败!\r\n请先安装Adobe Flash Player插件!", "Warning!!!");
                            System.Diagnostics.Process.Start("https://get2.adobe.com/cn/flashplayer/");  
                            f4 = null;
                            return "error";
                        }
                    }
                }else{
                    f4.Close();
                    f4 = null;
                    return PlayFlash(url);
                }
            }else{
                if(f4!=null)
                {
                    if (url == "FastForward")
                    {
                        cmd = "fnext";
                        f4.NextSlide();
                        lastfilename = f4.filename;
                    }
                    else if (url == "FastReverse")
                    {
                        cmd = "fprev";
                        f4.PreviousSlide();
                        lastfilename = f4.filename;
                    }
                    if (url == "pause")
                    {
                        cmd = "pause";
                        f4.Pause();
                        lastfilename = f4.filename;
                    }
                    else if (url == "play")
                    {
                        cmd = "play";
                        f4.Play();
                        lastfilename = f4.filename;
                    }
                }else{
                    return "error";
                }
            }
            return "success";
        }

        public string CloseFlash()
        {
            Log.Info("CloseFlash()...Form4");
            if (f4 != null)
            {
                f4.Close();
                f4 = null;
            }
            return "success";
        }

        #endregion 播放flash文件

        #region 播放音频、视频文件
        public string PlayVideo(string url)
        {
            Log.Info("PlayVideo()...Form5, url=" + url);
            string path = getFilepath(url);
            IntelligentRecommend.m_summary.AddResource(url);//PlayVideo
            if (f5 == null)
            {
                try
                {
                    f5 = new Form5(path);
                    f5.Show();

                    {
                        PPTInfo p = new PPTInfo();
                        p.filename = url;
                        p.filepath = path;
                        p.cmd = "fopen";
                        p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
                        p.pageTotal = 1;
                        p.pageIndex = 1;
                        p.page = "1/1";
                        p.md5 = "";
                        p.szImgData = null;
                        Log.Info(p.toJson());
                        Common.uploadFileEvent(p.toJson());
                    }

                }
                catch (Exception ex) { }
            }
            else
            {
                if (url == "pause")
                {
                    f5.StopVideo();
                }
                else if (url == "play")
                {
                    f5.PlayVideo();
                }
                else if (url == "FastForward")
                {
                    f5.FastForward();
                }
                else if (url == "FastReverse")
                {
                    f5.FastReverse();
                }
                else
                {
                    f5.Close();
                    f5 = null;
                    PlayVideo(url);
                }
            }
            return "success";
        }
        public string PlayAudio(string url)
        {
            Log.Info("PlayVideo()...Form5, url=" + url);
            string path = getFilepath(url);
            if (f5 == null)
            {
                try
                {
                    f5 = new Form5(path);
                    f5.Show();
                    {
                        PPTInfo p = new PPTInfo();
                        p.filename = url;
                        p.filepath = path;
                        p.cmd = "fopen";
                        p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
                        p.pageTotal = 1;
                        p.pageIndex = 1;
                        p.page = "1/1";
                        p.md5 = "";
                        p.szImgData = null;
                        Log.Info(p.toJson());
                        Common.uploadFileEvent(p.toJson());
                    }
                }
                catch (Exception ex) { }
            }
            else
            {
                if (url == "pause")
                {
                    f5.StopVideo();
                }
                else if (url == "play")
                {
                    f5.PlayVideo();
                }
                else if (url == "FastForward")
                {
                    f5.FastForward();
                }
                else if (url == "FastReverse")
                {
                    f5.FastReverse();
                }
                else
                {
                    f5.Close();
                    f5 = null;
                    PlayAudio(url);
                }
            }
            return "success";
        }

        public string CloseVideo()
        {
            Log.Info("CloseVideo()...Form5");
            if (f5 != null)
            {
                f5.Close();
                f5 = null;
            }
            return "success";
        }
        public string CloseAudio()
        {
            Log.Info("CloseAudio()...Form5");
            if (f5 != null)
            {
                f5.Close();
                f5 = null;
            }
            return "success";
        }

        #endregion

        //------------------------Doc,Pdf播放-------------------------

        #region PDF、Doc的播放
        public string OpenPDF(string filename)
        {
            string cmd = "";
            int lessonid = Global.getLessonID();
            string md5 = "";
            string lastfilename = "";
            Log.Info("OpenPDF()..., filename=" + filename);
            string name = filename.ToLower();
            if (name.IndexOf("pdf") > 0)
            {
                cmd = "fopen";
                lastfilename = filename;
                if (mypdf != null)
                {
                    mypdf.Close();
                    mypdf = null;
                }

                try
                {
                    string _path = getFilepath(filename);
                    md5 = Util.GetFileMD5(_path);
                    //Common.uploadPicture(_path);//upload ppt

                    IntelligentRecommend.m_summary.AddResource(filename);//OpenPDF

                    mypdf = new MyPDF();
                    bool bOpen = mypdf.Open(_path);
                    if (!bOpen)
                    {
                        mypdf = null;
                        GC.Collect();
                        return "error";
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("open ppt exception. name=" + filename + ", " + ex.Message);
                    return "error";
                }
            }
            else if (mypdf != null && filename == "next")
            {
                cmd = "fnext";
                if (!mypdf.NextSlide())
                {
                    return "error";
                }
                lastfilename = mypdf.filename;
            }
            else if (mypdf != null && filename == "prev")
            {
                cmd = "fprev";
                if (!mypdf.PreviousSlide())
                {
                    return "error";
                }
                lastfilename = mypdf.filename;
            }
            else if (mypdf != null && filename == "up")
            {
                cmd = "fup";
                mypdf.Up();
                lastfilename = mypdf.filename;
            }
            else if (mypdf != null && filename == "down")
            {
                cmd = "fdown";
                mypdf.Down();
                lastfilename = mypdf.filename;
            }
            else
            {
                return "error";
            }

            #region getPath
            string filepath = EService.getFilepath(lastfilename);
            string path = "";
            if (filepath.Length > 0 && filepath.IndexOf("#") > 0)
            {
                string hdtype = filepath.Split('#')[0].ToLower();
                path = filepath.Split('#')[1];
            }
            else
            {
                path = "usb";
            }
            #endregion


            PPTInfo p = new PPTInfo();
            p.filename = lastfilename;
            p.filepath = path;
            p.cmd = cmd;
            p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (mypdf != null)
            {
                p.pageTotal = 1;
                p.pageIndex = 1;
                p.page = "1/1";
            }

            if (cmd == "fopen")
            {
                p.md5 = md5;
                p.szImgData = null;
                Log.Info(p.toJson());
                Common.uploadFileEvent(p.toJson());
            }
            return "success";
        }

        public Response OpenDoc(string filename)
        {
            string cmd = "";
            int lessonid = Global.getLessonID();
            string md5 = "";
            string lastfilename = "";
            Log.Info("OpenDoc()...Form7, filename=" + filename);
            string name = filename.ToLower();
            if (name.IndexOf("doc") > 0)
            {
                cmd = "fopen";
                lastfilename = filename;
                if (mydoc != null)
                {
                    mydoc.Close();
                    mydoc = null;
                }
                
                try
                {
                    //KILL
                    Process[] myproc = Process.GetProcesses();
                    foreach (Process item in myproc)
                    {
                        if (item.ProcessName == "WINWORD" || item.ProcessName == "WINWORD.EXE")
                        {
                            Log.Info("KillProcess now. " + item.ProcessName);
                            try
                            {
                                item.Kill();
                            }
                            catch (Exception e)
                            {
                                Log.Error("KillProcess exception. " + item.ProcessName);
                            }

                            Log.Info("KillProcess over. " + item.ProcessName);
                        }
                    }

                    string _path = getFilepath(filename);
                    md5 = Util.GetFileMD5(_path);
                    Common.uploadPicture(_path);//upload doc

                    IntelligentRecommend.m_summary.AddResource(filename);//OpenDoc
                    Log.Info("OpenDoc()...mydoc=null, new Mydoc() now..." + _path);
                    mydoc = new MyDoc();
                    bool bOpen = mydoc.Open(_path);
                    if (!bOpen)
                    {
                        mydoc = null;
                        GC.Collect();

                        Response resp = new Response(0, cmd + " failed", "");
                        Log.Info(resp.toJson());
                        return resp;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("open ppt exception. filename=" + filename + ", " + ex.Message);
                }
            }
            else if (mydoc != null && filename == "next")
            {
                cmd = "fnext";
                mydoc.NextSlide();
                lastfilename = mydoc.filename;
            }
            else if (mydoc != null && filename == "prev")
            {
                cmd = "fprev";
                mydoc.PreviousSlide();
                lastfilename = mydoc.filename;
            }
            else if (mydoc != null && filename == "up")
            {
                cmd = "fup";
                mydoc.Up();
                lastfilename = mydoc.filename;
            }
            else if (mydoc != null && filename == "down")
            {
                cmd = "fdown";
                mydoc.Down();
                lastfilename = mydoc.filename;
            }
            else
            {
                Response resp = new Response(0, cmd + " failed", "");
                Log.Info(resp.toJson());
                return resp;
            }

            #region getPath
            string filepath = EService.getFilepath(lastfilename);
            string path = "";
            if (filepath.Length > 0 && filepath.IndexOf("#") > 0)
            {
                string hdtype = filepath.Split('#')[0].ToLower();
                path = filepath.Split('#')[1];
            }
            else
            {
                path = "usb";
            }
            #endregion

            
            PPTInfo p = new PPTInfo();
            p.filename = lastfilename;
            p.filepath = path;
            p.cmd = cmd;
            p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
            if(mydoc!=null)
            {
                p.pageTotal = mydoc.pageTotal;
                p.pageIndex = mydoc.pageCurrent;
                p.page = mydoc.pageCurrent + "/" + mydoc.pageCurrent;
            }

            if (cmd == "fopen")
            {
                p.md5 = md5;
                p.szImgData = null;
                Log.Info(p.toJson());
                Common.uploadFileEvent(p.toJson());
            }
            p.szImgData = new string[1];
            string ret = p.toJson();
            Response _resp = new Response(1, cmd + " ok", ret);
            Log.Info(_resp.toJson());
            return _resp;
        }
        
        public string OpenWeb(string url)
        {
            Log.Info("OpenWeb()...Form6, url=" + url);
            if (f6 == null)
            {
                try
                {
                    f6 = new Form6(url);
                    f6.Show();
                }
                catch (Exception ex) { }
            }
            else
            {
                f5.Close();
                f5 = null;
                OpenWeb(url);
            }
            return "success";
        }

        #endregion

        #region PDF、Doc的关闭
        public string ClosePDF()
        {
            Log.Info("CloseDoc()...");
            if (mypdf != null)
            {
                mypdf.Close();
                mypdf = null;
            }
            return "success";
        }

        public string CloseDoc()
        {
            Log.Info("CloseDoc()...Form7");
            if (mydoc != null)
            {
                mydoc.Close();
                mydoc = null;
            }
            return "success";
        }

        public string CloseWeb()
        {
            Log.Info("CloseWeb()...Form6");
            if (f6 != null)
            {
                f6.Close();
                f6 = null;
            }
            return "success";
        }

#endregion
        //------------------------触摸板操作-------------------------
        #region 触摸板操作
        public string MouseMove(double Rx,double Ry)
        {
            int x = (int)Rx;
            int y = (int)Ry;
            if (mytouch != null)
            {
                //mytouch = null;
            }
            else {
                mytouch = new MyTouch();
            }
            mytouch.Move(x,y);
            return "success";
        }
        public string MouseClick()
        {
            if (mytouch != null)
            {
                //mytouch = null;
            }
            else
            {
                mytouch = new MyTouch();
            }
            mytouch.Click();
            return "success";
        }
        public string MouseRightClick()
        {
            if (mytouch != null)
            {
                //mytouch = null;
            }
            else
            {
                mytouch = new MyTouch();
            }
            if (myppt != null)
            {
                myppt.savePPT();
            }
            mytouch.RightClick();
            return "success";
        }
        #endregion
        //------------------------PPT播放-------------------------
        #region PPT 播放和操作
        public string PPT(string url)
        {
            Log.Info("PPT()...Form7, url=" + url);
            if (f7 == null)
            {
                try
                {
                    f7 = new Form7(url);
                    f7.Show();
                }
                catch (Exception ex) { }
            }
            else
            {
                if (url == "top")
                {
                    f7.ScrollTop();
                }
                else if (url == "bottom")
                {
                    f7.ScrollBottom();
                }
                else if (url.Split('_')[0].ToString() == "scrollto")
                {
                    f7.ScrollTo(Convert.ToInt16(url.Split('_')[1].ToString()), Convert.ToInt16(url.Split('_')[2].ToString()));
                }
                else
                {
                    f7.Close();
                    f7 = null;
                    PPT(url);
                }
            }
            return "success";
        }

        public Response OpenPPT(string filename,string pageIndex)
        {
            if (filename.IndexOf("doc") > 0)
                return OpenDoc(filename);

            string cmd = "";
            int lessonid = Global.getLessonID();
            string pptname = "";
            string md5 = "";

            Log.Info("OpenPPT()...Form7, filename=" + filename);
            string name = filename.ToLower();
            if (name.IndexOf("ppt") > 0)
            {
                cmd = "fopen";
                pptname = filename;
                if (myppt != null)
                {
                    if (filename != myppt.filename)
                    {
                        myppt.pageCurrent = 0;
                    }
                }

                #region 检查缩略图是否就绪
                while (MyPPT.checkImgStatus(filename)==0)
                {
                    Thread.Sleep(500);
                }
                string _path = getFilepath(filename);

                #endregion 检查缩略图是否就绪

                try
                {
                    //KILL
                    Process[] myproc = Process.GetProcesses();
                    foreach (Process item in myproc)
                    {
                        if (item.ProcessName == "POWERPNT" || item.ProcessName == "POWERPNT.EXE")
                        {
                            Log.Info("KillProcess now. " + item.ProcessName);
                            try
                            {
                                item.Kill();
                            }
                            catch (Exception e)
                            {
                                Log.Error("KillProcess exception. " + item.ProcessName);
                            }

                            Log.Info("KillProcess over. " + item.ProcessName);
                        }
                    }

                    Log.Info("LocalOpenPPT()...myppt=null, new MyPPT() now..." + filename);
                    md5 = Util.GetFileMD5(_path);
                    IntelligentRecommend.m_summary.AddResource(filename);//PPT
                    Common.uploadPicture(_path);//upload ppt

                    myppt = new MyPPT();
                    if(PPTPageMap.ContainsKey(filename))
                    {
                        myppt.pageCurrent = PPTPageMap[filename];
                    }
                    bool bOpen = myppt.PPTOpen(_path);
                    if (!bOpen)
                    {
                        myppt = null;
                        GC.Collect();

                        Response resp = new Response(0, cmd + " failed", "");
                        Log.Info(resp.toJson());
                        return resp;
                    }
                    else
                    {
                        if(PPTPageMap.ContainsKey(filename))
                        {
                            Thread.Sleep(1000);
                            int page = PPTPageMap[filename];
                            myppt.GotoPage(page);
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("open ppt exception. name=" + filename + ", " + ex.Message);
                }
            }
            else if (myppt != null && filename == "next")
            {
                cmd = "fnext";
                myppt.NextSlide();
                pptname = myppt.filename;
            }
            else if (myppt != null && filename == "prev")
            {
                cmd = "fprev";
                myppt.PreviousSlide(pageIndex);
                pptname = myppt.filename;
            }
            else if (myppt != null && filename == "noJump")
            {
                myppt.NoSlide(pageIndex);
                pptname = myppt.filename;
            }
            else if (myppt != null)
            {
                cmd = "fgoto";
                try
                {
                    int n = Int32.Parse(name);
                    myppt.GotoPage(n);
                    pptname = myppt.filename;
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                }
            }
            else
            {
                Response resp = new Response(0, cmd + " failed", "");
                Log.Info(resp.toJson());
                return resp;
            }

            int pageTotal = myppt.pageTotal;
            int pageCur = myppt.pageCurrent;

            string filepath = EService.getFilepath(pptname);

            string path = "";
            if (filepath.Length > 0 && filepath.IndexOf("#") > 0)
            {
                string hdtype = filepath.Split('#')[0].ToLower();
                path = filepath.Split('#')[1];
            }
            else
            {
                path = "usb";
            }

            PPTInfo p = new PPTInfo();
            p.filename = pptname;
            p.filepath = path;
            p.cmd = cmd;
            p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
            p.pageTotal = myppt.pageTotal;
            p.pageIndex = myppt.pageCurrent;
            p.page = myppt.pageCurrent + "/" + myppt.pageTotal;
            p.urls = myppt.urls;

            if (cmd == "fopen")
            {
                p.md5 = md5;
                //p.szImgData = new string[myppt.pageTotal];
                for (int i = 1; i <= myppt.pageTotal; i++)
                {
                    //string filedata = myppt.getImgData(i);
                    //p.szImgData[i - 1] = filedata;

                    string _filepath = myppt.getImgPath(i);
                    FileInfo fi = new FileInfo(_filepath);
                    string _filename = fi.Name;//text.txt

                    if (!filemap.ContainsKey(_filename))
                    {
                        filemap.Add(_filename, _filepath);
                    }
                }
            }
            else
            {
                if (PPTPageMap.ContainsKey(myppt.filename))
                {
                    PPTPageMap[myppt.filename] = myppt.pageCurrent;
                }
                else
                {
                    PPTPageMap.Add(myppt.filename, myppt.pageCurrent);
                }
            }


            Response _resp = new Response(1, cmd + " ok", p.toJson());
            Log.Info(_resp.toJson());

            if (myppt.bPageTurning)
            {
                p.szImgData = null;
                p.urls = "";
                p.filepath = "";
                Common.uploadFileEvent(p.toJson());
            }
            return _resp;
        }

        public string LocalOpenPPT(string filename)
        {
            if(filename.IndexOf("ppt")>0)
                OpenPPT(filename,"");
            else if (filename.IndexOf("doc") > 0)
                OpenDoc(filename);
            return "success";
        }

        public string LocalClosePPT()
        {
            Log.Info("LocalClosePPT()");

            if (myppt != null)
            {
                PPTInfo p = new PPTInfo();
                p.filename = myppt.filename;
                p.filepath = "";
                p.cmd = "fclose";
                p.uptime = DateTime.Now.ToString("yyyyMMddHHmmss");
                p.pageTotal = myppt.pageTotal;
                p.pageIndex = myppt.pageCurrent;
                p.page = myppt.pageCurrent + "/" + myppt.pageCurrent;
                p.md5 = "";
                Common.uploadFileEvent(p.toJson());


                myppt.PPTClose();
                //myppt = null;
                //myppt.minisizeProc();
            }
            return "success";
        }
        public string ClosePPT()
        {
            Log.Info("ClosePPT()...Form7");
            if (f7 != null)
            {
                f7.Close();
                f7 = null;
            }
            return "success";
        }
        #endregion

        #region 模拟鼠标单击
        public void PPTMouseClick(double x, double y)
        {
            myppt.PPTMouseClick(x, y);
        }
        public void PPTMouseMove(double x, double y)
        {
            myppt.PPTMouseMove(x, y);
        }
        #endregion

        #region PPT最小化 最大化
        public string MinimizePPT()
        {
            Log.Info("MinimizePPT()...Form7");
            MyPPT.minisizeAllProc();
            Util.ShowDesktop();
            return "success";
        }
        public string MaximizePPT()
        {
            Log.Info("MaxsimizePPT()...Form7");
            if (myppt != null)
            {
                myppt.maximizePPTProc();
            }
            return "success";
        }
        #endregion

        #region 下载PPT缩列图
        public Stream Download(string fileName)
        {
            string _path = getFilepath(fileName);
            if (!File.Exists(_path))//判断文件是否存在
            {
                return null;
            }
            try
            {
                Stream myStream = File.OpenRead(_path);
                return myStream;
            }
            catch { return null; }
        }
        #endregion

        #region 下载文件内容（base64编码）
        public string GetFileBase64(string filename)
        {
            string _path = getFilepath(filename);
            if (!File.Exists(_path))//判断文件是否存在
            {
                return "";
            }
            try
            {
                Bitmap bmp = new Bitmap(_path);
                MemoryStream ms = new MemoryStream();
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                string pic = Convert.ToBase64String(arr);
                return pic;
            }
            catch { return ""; }
        }
        #endregion

        //------------------------录音和录像-------------------------
        #region 录音和录像 的上传和删除
        public Response UploadVideo(Stream stream)
        {
            Log.Info("UploadVideo()...");
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp4";
            string srcDir = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
            string srcFile = Path.Combine(srcDir, fileName);

            string srcDir_Relative = DateTime.Now.ToString("yyyyMMdd");
            string srcFile_Relative = Path.Combine(srcDir_Relative, fileName);

            FileStream targetStream = null;
            try
            {
                if (!stream.CanRead)
                {
                    throw new Exception("数据流不可读!");
                }
                if (!srcDir.EndsWith("\\"))
                    srcDir += "\\";
                Log.Info("UploadVideo()...check dir: " + srcDir);
                if (!Directory.Exists(srcDir))
                {
                    Log.Info("UploadVideo()...create dir: " + srcDir);
                    Directory.CreateDirectory(srcDir);
                }

                string _name = "";
                using (targetStream = new FileStream(srcFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //read from the input stream in 4K chunks
                    //and save to output stream
                    const int bufferLen = 10240;
                    byte[] buffer = new byte[bufferLen];
                    byte[] bufferHead = new byte[bufferLen];
                    int count = 0;
                    bool bRead = false;
                    Log.Info("UploadVideo()...read stream now... dstfile=" + srcFile);
                    while ((count = stream.Read(buffer, 0, bufferLen)) > 0)
                    {
                        //Debug.Write("接收中..............");
                        if(bRead==false)
                        {
                            Array.ConstrainedCopy(buffer, 0, bufferHead, 0, bufferLen);
                            bRead = true;

                            byte[] bufferData = new byte[bufferLen];
                            int pos = Util.bytesIndexOf(buffer,System.Text.Encoding.ASCII.GetBytes("CALLBACK&"));
                            int len = 9;
                            if (pos == -1)
                            {
                                pos = Util.bytesIndexOf(buffer, System.Text.Encoding.ASCII.GetBytes("image/jpeg\r\n\r\n"));
                                len = 14;
                            }
                            Array.ConstrainedCopy(buffer, pos + len, bufferData, 0, count - pos - len);
                            targetStream.Write(bufferData, 0, count - pos - len);

                            string s2 = System.Text.Encoding.UTF8.GetString(bufferHead);
                            NameValueCollection nvc = Util.ParseQueryString(s2);
                            _name = nvc["name"];
                        }
                        else
                        {
                            targetStream.Write(buffer, 0, count);
                        }
                    }
                    targetStream.Close();
                    stream.Close();
                    //Debug.Write("结束流");
                    Log.Info("UploadVideo()...read stream over... dstfile=" + srcFile);
                    selectFile(_name, srcFile);

                    string ffmpegExe = Application.StartupPath + "\\ffmpeg.exe ";
                    if (File.Exists(ffmpegExe))
                    {
                        Thread BackThread = new Thread(delegate()
                        {
                            //var pStartInfo = new ProcessStartInfo
                            //{
                            //    WorkingDirectory = Application.StartupPath,
                            //    FileName = @"ffmpeg.exe",
                            //    Arguments = Argu,
                            //    CreateNoWindow  = true
                            //};
                            //Process p = Process.Start(pStartInfo);

                            string TargetFile = Path.Combine(srcDir_Relative, _name);
                            Log.Info("UploadVideo() recv mp4 ok. ffmpeg now..." + srcFile_Relative + ", TargetFile=" + TargetFile);

                            string Argu = @"-i " + srcFile_Relative + " -ar 22050 -b 700k -s 800x480 " + TargetFile;
                            //string Argu = @"-i " + srcFile_Relative + " -ar 22050 -b 300 -s 800x480 -vcodec mpeg4 -ab 32 -acodec aac -strict experimental -r 23 " + TargetFile;
                            //string Argu = @"-i " + srcFile_Relative + " -ar 22050 -qscale 6 -s 800x450 -vcodec mpeg4 -ab 32 -acodec aac -strict experimental -r 23 " + TargetFile;
                            StringBuilder sbExe = new StringBuilder(255);
                            Util.GetShortPathName(ffmpegExe, sbExe, 255);
                            Log.Info(sbExe.ToString() + Argu);

                            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(sbExe.ToString(), Argu);
                            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            Process p2 = System.Diagnostics.Process.Start(startInfo);
                            while (!p2.HasExited)
                            {
                                Thread.Sleep(500);
                            }

                            if (File.Exists(TargetFile))
                            {
                                string md5 = Util.GetFileMD5(TargetFile);
                                Log.Info("UploadVideo() convert mp4 ok. upload now...");
                                Common.uploadPicture(TargetFile);//upload video
                                Common.uploadRecordEvent(Path.GetFileName(TargetFile), md5);
                            }

                        });
                        BackThread.IsBackground = true;
                        BackThread.Start();
                    }
                    else
                    {

                    }
                    

                }

            }
            catch (Exception ex) {
                Log.Error("视频错误：" + ex.Message);
                Response _resp = new Response(-1, "UploadVideo OK", "");
                return _resp;
            }
            Response resp = new Response(0, "UploadVideo OK", fileName);
            return resp;
        }

        public Response UploadAudio(Stream stream)
        {
            Log.Info("UploadAudio()...");
            string savaPath = Application.StartupPath + "\\" + DateTime.Now.ToString("yyyyMMdd");
            string fileName = Global.getSchoolID()+"_"+DateTime.Now.ToString("yyyyMMddHHmmss") + ".mp3";

            FileStream targetStream = null;
            try
            {
                if (!stream.CanRead)
                {
                    throw new Exception("数据流不可读!");
                }
                if (!savaPath.EndsWith("\\"))
                    savaPath += "\\";

                string uploadFolder = savaPath;
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, fileName);
                string _name = "";
                using (targetStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    //read from the input stream in 4K chunks
                    //and save to output stream
                    const int bufferLen = 4096;
                    byte[] buffer = new byte[bufferLen];
                    int count = 0;
                    bool bRead = false;
                    byte[] bufferHead = new byte[bufferLen];
                    while ((count = stream.Read(buffer, 0, bufferLen)) > 0)
                    {
                        if (bRead == false)
                        {
                            Array.ConstrainedCopy(buffer, 0, bufferHead, 0, bufferLen);
                            bRead = true;

                            byte[] bufferData = new byte[bufferLen];
                            int pos = Util.bytesIndexOf(buffer, System.Text.Encoding.ASCII.GetBytes("CALLBACK&"));
                            Array.ConstrainedCopy(buffer, pos + 9, bufferData, 0, count - pos - 9);
                            targetStream.Write(bufferData, 0, count - pos - 9);

                            string s2 = System.Text.Encoding.UTF8.GetString(bufferHead);
                            NameValueCollection nvc = HttpUtility.ParseQueryString(s2);
                            _name = nvc["name"];
                        }
                        else
                        {
                            targetStream.Write(buffer, 0, count);
                        }
                    }
                    targetStream.Close();
                    stream.Close();

                    {
                        selectFile(_name, filePath);
                        Common.uploadPicture(filePath);//upload audio
                        string md5 = Util.GetFileMD5(filePath);
                        Common.uploadRecordEvent(Path.GetFileName(filePath), md5);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Response _resp = new Response(-1, "UploadAudio Error", "");
                return _resp;
            }
            Response resp = new Response(0, "UploadAudio OK", fileName);
            return resp;
        }

        public string DelMedia(string filename)
        {
            if (filename.IndexOf("mp3") > 0)
            {
                string path = getFilepath(filename);
                string filename2 = Path.GetFileName(path);
                Common.delRecordEvent(filename2);
            }
            else
            {
                Common.delRecordEvent(filename);
            }
            return "success";
        }
        #endregion

        #region 课堂总结
        public string Summary()
        {
            if (fSummay != null)
            {
                fSummay.Close();
                fSummay = null;
            }
            fSummay = new FormSummary();

            string str = fSummay.ShowSummary();
            return str;
        }
        public string SummaryClose()
        {
            if (fSummay != null)
            {
                fSummay.Close();
                fSummay = null;
            }

            return "success";
        }
        public Response getXitiStat()
        {
            Response _resp = new Response(1, "getXitiStat ok", IntelligentRecommend.getXitiStat());
            Log.Info(_resp.toJson());
            return _resp;
        }
        //提问 点名 举手 奖励统计
        public Response getStat()
        {
            Response _resp = new Response(1, "getStat ok", IntelligentRecommend.getStat());
            Log.Info(_resp.toJson());
            return _resp;
        }
        #endregion

        #region 分组教学
        public string showGroup(string group, string name)
        {
            if (group.Length == 0 && name.Length == 0)
            {
                if (fGroup != null)
                {
                    fGroup.StopT();
                    fGroup.Close();
                    fGroup.Dispose();
                    fGroup = null;
                }
                return "success";
            }

            if (fGroup != null)
            {
                fGroup.StopT();
                fGroup.Close();
                fGroup.Dispose();
                fGroup = null;
            }
            fGroup = new FormGroup(12);
            fGroup.ShowGroup(group, name);
            return "success";
        }

        public string startGroupCallname(string group)
        {
            if (fGroupCallname == null)
            {
                fGroupCallname = new FormGroupCallname(12);
            }
            fGroupCallname.Callname(group);
            if (fGroup!=null)
            {
                fGroup.AppendCallname(group);
            }
            return "success";
        }

        public string startGroupReward(string group,int result)
        {
            if (fGroupCallname == null)
            {
                fGroupCallname = new FormGroupCallname(12);
            }
            fGroupCallname.Reward(group, result);
            if (fGroup != null)
            {
                fGroup.AppendReward(group, result+"");
            }
            return "success";
        }

        public string startGroupHandon()
        {
            if (fGroup != null)
            {
                fGroup.StopT();
                fGroup.Close();
                fGroup.Dispose();
                fGroup = null;
            }
            Httpd.clearPracticeResult();
            Common.ClearHandon(false);
            Common.ClearXiti();

            fGroup = new FormGroup(12);
            fGroup.Handon();
            return "success";
        }

        public string showGroupRank()
        {
            if (fGroupCallname != null)
            {
                fGroupCallname.Close();
                fGroupCallname = null;
            }

            try
            {
                fGroupCallname = new FormGroupCallname(12);
                fGroupCallname.ShowRank();
            }
            catch (Exception e)
            {
                Log.Info(e.Message);
            }

            return "success";
        }

        public string closeGroupRank()
        {
            if (fGroupCallname != null)
            {
                fGroupCallname.Close();
                fGroupCallname = null;
            }
            return "success";
        }
        public string startGroupXiti(string rid)//bsj10001381
        {
            if (fGroup != null)
            {
                fGroup.StopT();
                fGroup.Close();
                fGroup.Dispose();
                fGroup = null;
            }
            Httpd.clearPracticeResult();
            Common.ClearHandon(false);
            Common.ClearXiti();

            fGroup = new FormGroup(12);
            string questionstr = null;
            if(rid!=null && rid.Length > 0)
            {
                //从云端获取习题内容
                string data = "rid=" + rid;
                questionstr = Common.doPost("getXitiBody", data);
            }
            fGroup.Xiti(questionstr,rid);
            return "success";
        }

        public string endGroupHandon()
        {
            if (fGroup != null)
            {
                string handon = fGroup.StopT();
                string rid = fGroup._xitiId;
                string answer = fGroup.answer;
                string createtime = fGroup.tm_create.ToString("yyyyMMddHHmmss");
                int timeuse = Util.getTimeDiff_Second(fGroup.tm_create);

                string callnameStr = fGroup.GetCallname();
                string rewardStr = fGroup.GetRewarded();

                fGroup.Close();
                fGroup.Dispose();
                fGroup = null;

                if (handon.Length > 0 || callnameStr.Length > 0)
                {
                    Common.uploadHandon_Group(rid, createtime, handon, callnameStr, rewardStr, Global.getGroupId() , timeuse);//同步到本地服务和云服务器
                }
            }
            return "success";
        }

        public string endGroupXiti()
        {
            if (fGroup != null)
            {
                string result = fGroup.StopT();
                string rid = fGroup._xitiId;
                string answer = fGroup.answer;
                string createtime = fGroup.tm_create.ToString("yyyyMMddHHmmss");
                int timeuse = Util.getTimeDiff_Second(fGroup.tm_create);

                string callnameStr = fGroup.GetCallname();
                string rewardStr = fGroup.GetRewarded();

                fGroup.Close();
                fGroup.Dispose();
                fGroup = null;

                if (result.Length > 0)
                {
                    Common.uploadXiti_Group(rid, createtime, result, callnameStr, rewardStr, Global.getGroupId(), timeuse);//同步到本地服务和云服务器
                }
            }
            return "success";
        }

        public string setGroupXitiAnswer(string answer)
        {
            if (fGroup != null)
            {
                fGroup.answer = answer;
            }
            return "success";
        }
        
        
        #endregion
    }
}


