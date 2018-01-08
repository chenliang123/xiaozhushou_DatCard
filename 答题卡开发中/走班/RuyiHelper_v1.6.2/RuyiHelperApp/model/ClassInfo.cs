﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace RueHelper
{
    [DataContract]
    public class Resp
    {
        [DataMember]
        public int ret { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public string data { get; set; }
        [DataMember]
        public int count { get; set; }
    }

    [DataContract]
    public class DataInfo
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Grade { get; set; }
        [DataMember]
        public int StudentCount { get; set; }
        [DataMember]
        public StudentInfo[] Student { get; set; }
        [DataMember]
        public User[] Teacher { get; set; }
        //[DataMember]
        //public CoursetableDay[] coursetable { get; set; }
        [DataMember]
        public int UploadInvalidData { get; set; }
    }
    [DataContract]
    public class ClassInfo
    {
        [DataMember]
        public int ret { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public DataInfo Data { get; set; }
    }
    [DataContract]
    public class Xiti
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string rid { get; set; }
        [DataMember]
        public string answer { get; set; }
        [DataMember]
        public string content { get; set; }
        [DataMember]
        public string difficulty { get; set; }
        [DataMember]
        public string source { get; set; }
    }
    [DataContract]
    public class CBInfo
    {
        [DataMember]
        public string data { get; set; }
    }
    [DataContract]
    public class AnswerInfo
    {
        [DataMember]
        public int CBID { get; set; }
        [DataMember]
        public string CBAnswer { get; set; }
    }
    [DataContract]
    public class StudentInfo
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string cardid { get; set; }
        [DataMember]
        public string pinyin { get; set; }
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string SEAT { get; set; }
        [DataMember]
        public string imageurl { get; set; }
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string pinying { get; set; }
        [DataMember]
        public string phone { get; set; }
        [DataMember]
        public string cardid { get; set; }
        [DataMember]
        public string account { get; set; }
        [DataMember]
        public string pwd { get; set; }
        [DataMember]
        public int classid { get; set; }
        [DataMember]
        public int schoolid { get; set; }
        [DataMember]
        public int type { get; set; }
        [DataMember]
        public string seat { get; set; }
        [DataMember]
        public int lessonid { get; set; }
        [DataMember]
        public int courseid { get; set; }
        [DataMember]
        public string coursename { get; set; }
        [DataMember]
        public string imageurl { get; set; }

        public void valid()
        {
            if (seat == null)
                seat = "";
            if (account == null)
                account = "";
            if (pwd == null)
                pwd = "";
            if (phone == null)
                phone = "";
            if (cardid == null)
                cardid = "";
            if (name == null)
                name = "";
        }

        public string toJson()
        {
            string ret= new JavaScriptSerializer().Serialize(this);
            return ret;
        }
    }

    [DataContract]
    public class PYGroup
    {
        [DataMember]
        public string ch { get; set; }
        [DataMember]
        public List<User> userlist { get; set; }

        public string toJson()
        {
            string ret = new JavaScriptSerializer().Serialize(this);
            return ret;
        }
    }


    [DataContract]
    public class SchoolInfo
    {
        [DataMember]
        public int schoolid { get; set; }
        [DataMember]
        public string schoolname { get; set; }
        [DataMember]
        public Classes[] classlist { get; set; }
        [DataMember]
        public User[] teacherlist { get; set; }
        [DataMember]
        public AwardType[] awardtypelist { get; set; }
        [DataMember]
        public string coursetime { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
    [DataContract]
    public class SchoolStuInfo
    {
        [DataMember]
        public int schoolid { get; set; }
        [DataMember]
        public string schoolname { get; set; }
        [DataMember]
        public Classes[] classlist { get; set; }
        [DataMember]
        public User[] teacherlist { get; set; }
        [DataMember]
        public StudentInfo[] studentlist { get; set; }
        [DataMember]
        public AwardType[] awardtypelist { get; set; }
        [DataMember]
        public string coursetime { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
    [DataContract]
    public class CoursetableDay
    {
        [DataMember]
        public string week { get; set; }
        [DataMember]
        public string courses { get; set; }
    }

    [DataContract]
    public class CourseTime
    {
        [DataMember]
        public int week { get; set; }
        [DataMember]
        public int index { get; set; }
        [DataMember]
        public string timeOn { get; set; }
        [DataMember]
        public string timeOff { get; set; }
    }

    [DataContract]
    public class AwardType
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int uid { get; set; }
        [DataMember]
        public int type { get; set; }
        [DataMember]
        public int point { get; set; }
        [DataMember]
        public string reason { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class SchoolGrade
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public List<Grade> gradelist { get; set; }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class Grade
    {
        //{"id":"1638","schoolid":"33","grade":"1","name":"一(1)班","seatxy":"1,1","orderid":"1",
        //"roomid":"1360","roomname":"一年级一班","building":"","hdid":"192.168.253.201","hdip":"1","hdport":"80",
        //"appip":"172.18.2.104","courseid":"12","coursename":"语文"}
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public List<Classes> classlist { get; set; }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class Classes
    {
        //{"id":"1638","schoolid":"33","grade":"1","name":"一(1)班","seatxy":"1,1","orderid":"1",
        //"roomid":"1360","roomname":"一年级一班","building":"","hdid":"192.168.253.201","hdip":"1","hdport":"80",
        //"appip":"172.18.2.104","courseid":"12","coursename":"语文"}
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int schoolid { get; set; }
        [DataMember]
        public int grade { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string seatxy { get; set; }
        [DataMember]
        public int orderid { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember]
        public int ret { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public Object data { get; set; }

        public Response(int ret, string msg, Object data)
        {
            this.ret = ret;
            this.msg = msg;
            this.data = data;
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class PPTInfo
    {
        [DataMember]
        public string filename { get; set; }
        [DataMember]
        public string filepath { get; set; }
        [DataMember]
        public string cmd { get; set; }
        [DataMember]
        public string uptime { get; set; }
        [DataMember]
        public string page { get; set; }
        [DataMember]
        public int pageTotal { get; set; }
        [DataMember]
        public int pageIndex { get; set; }
        [DataMember]
        public string md5 { get; set; }
        [DataMember]
        public string urls { get; set; }
        [DataMember]
        public string[] szImgData { get; set; }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class XitiResult
    {
        [DataMember]
        public string rid { get; set; }
        [DataMember]
        public string answer { get; set; }
        [DataMember]
        public string ctime { get; set; }
        [DataMember]
        public int duration { get; set; }
        [DataMember]
        public int count { get; set; }
        [DataMember]
        public int countok { get; set; }
        [DataMember]
        public int timeMin { get; set; }
        [DataMember]
        public int timeMax { get; set; }
        [DataMember]
        public int timeAverage { get; set; }
        [DataMember]
        public string result { get; set; }
        [DataMember]
        public string callname { get; set; }
        [DataMember]
        public string reward { get; set; }
        [DataMember]
        public string criticize { get; set; }

        public XitiResult(string id,string answer,string result,string callname,string reward, string criticize,string ctime,int duration)
        {
            this.rid = id;
            this.answer = answer.ToUpper();
            this.result = result;
            this.ctime = ctime;
            this.duration = duration;
            this.callname = callname;
            this.reward = reward;
            this.criticize = criticize;

            timeMin = 0;
            timeMax = 0;
            timeAverage = 0;
            int timeSum = 0;

            if (result.Length == 0)
                return;

            //TODO: calc right,wrong,timeMin,timeMax,timeAverage
            string[] szResult = result.Split(',');
            count = szResult.Length;

            for (int i = 0; i < szResult.Length; i++)
            {
                string item = szResult[i];
                string[] szItem = item.Split(':');
                string seat = szItem[0];
                string keys = szItem[1];
                string time = "0";
                if(szItem.Length==3)
                    time = szItem[2];
                int nTime = Int32.Parse(time);
                timeSum += nTime; 
                if (i == 0)
                {
                    timeMin = nTime;
                    timeMax = nTime;
                }
                else
                {
                    timeMin = timeMin > nTime ? nTime : timeMin;
                    timeMax = timeMax < nTime ? nTime : timeMax;
                }

                if (keys == answer)
                {
                    countok++;
                }
            }
            if (count == 0)
                timeAverage = 0;
            else
                timeAverage = timeSum / count;
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }


    [DataContract]
    class UpdateInfo
    {
        [DataMember]
        public UpdateItem[] updateinfolist { get; set; }
    }

    [DataContract]
    class UpdateItem
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int type { get; set; }
        [DataMember]
        public string softname { get; set; }
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string path { get; set; }
        [DataMember]
        public string date { get; set; }
        [DataMember]
        public string content { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class CusFileInfo
    {
        public string filepath;

        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public long FileLength { get; set; }
        [DataMember]
        public string MapPath { get; set; }
    }

    [DataContract]
    public class Grouplist
    {
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public int classid { get; set; }
        [DataMember]
        public int courseid { get; set; }
        [DataMember]
        public Group[] grouplist { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class Group
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int point { get; set; }
        [DataMember]
        public string uids { get; set; }

        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }

    [DataContract]
    public class RobotPenImageItem
    {
        [DataMember]
        public string imgName { get; set; }
        [DataMember]
        public string time { get; set; }
        public RobotPenImageItem(string t,string n)
        {
            time = t;
            imgName = n;
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
    [DataContract]
    public class RobotPenImageGroup
    {
        [DataMember]
        public List<RobotPenImageItem> imglist { get; set; }
        public int status = 0;//update status
        public RobotPenImageGroup()
        {
            imglist = new List<RobotPenImageItem>();
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
    [DataContract]
    public class RobotPenImages
    {
        [DataMember]
        public List<RobotPenImageGroup> grouplist { get; set; }
        [DataMember]
        public int seat = 0;
        [DataMember]
        public int uid = 0;
        
        public RobotPenImages()
        {
            grouplist = new List<RobotPenImageGroup>();
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
    [DataContract]
    public class RobotPenRecord
    {
        [DataMember]
        public List<RobotPenImages> recordlist { get; set; }
        public RobotPenRecord()
        {
            recordlist = new List<RobotPenImages>();
        }
        public string toJson()
        {
            string str = new JavaScriptSerializer().Serialize(this);
            return str;
        }
    }
}
