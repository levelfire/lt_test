using Assets.SuperStar.Scripts.Network.model;
using FrameProtoBuf;
using ProtoBuf.Meta;
using SSTest.Comm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSTest
{
    public partial class Form1 : Form
    {
        //public static string urlroot = @"http://192.168.66.77:8888/";
        //public static string signkey = @"!1AQaq#3";
        public static ResultLogin loginresult = null;
        public static string sessionid = null;
        public static server_info si = null;
        //public string PostTemp(string postData, string url)
        //{
        //    string responseFromServer = string.Empty;
        //    if (string.IsNullOrWhiteSpace(url))
        //    {
        //        return null;
        //    }

        //    //string uri = string.Format("{0}?who={1}", url, "joey");

        //    string uri = url;

        //    WebRequest request = WebRequest.Create(uri);
        //    request.Method = "POST";

        //    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

        //    request.ContentType = "application/x-www-form-urlencoded";
        //    //request.ContentType = "application/json";
        //    // Set the ContentLength property of the WebRequest.
        //    request.ContentLength = byteArray.Length;

        //    // Get the request stream.
        //    using (Stream dataStream = request.GetRequestStream())
        //    {
        //        // Write the data to the request stream.
        //        dataStream.Write(byteArray, 0, byteArray.Length);
        //    }

        //    // Get the response.      
        //    using (WebResponse response = request.GetResponse())
        //    {
        //        using (Stream dataStream = response.GetResponseStream())
        //        {
        //            using (StreamReader reader = new StreamReader(dataStream))
        //            {
        //                responseFromServer = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    return responseFromServer;
        //}

        //public string check_device(string dev, string plat, string version)
        //{
        //    //string url = string.Format("http://192.168.66.88:8888/check_device?dev={0}&plat={1}&version={2}",dev,plat,version);
        //    string url = urlroot + "check_device";
        //    string data = string.Format("dev={0}&plat={1}&version={2}", dev, plat, version);
        //    string result = PostTemp(data, url);

        //    return result;
        //}

        //public string login(string tp, string Dev, string Plat, string version, string Id, string Name)
        //{
        //    string url = urlroot + "login";
        //    string data = string.Format("tp={0}&dev={1}&plat={2}&version={3}&Id={4}&Name={5}", tp, Dev, Plat, version, Id, Name);
        //    string result = SSTest.Comm.ManagerHttp.HttpPost(data, url);

        //    System.Web.Script.Serialization.JavaScriptSerializer js = new System.Web.Script.Serialization.JavaScriptSerializer();
        //    loginresult = js.Deserialize<Result_Login>(result);

        //    return result;
        //}

        //public string login_server()
        //{
        //    if (loginresult == null || loginresult.data == null || loginresult.data.server_list == null || loginresult.data.server_list.Count == 0)
        //    {
        //        return null;
        //    }

        //    server_info si = loginresult.data.server_list[0].server_info;

        //    string url = @"http://" + si.ip + ":" + si.port + "/login";

        //    string mdstr = string.Format(@"uin={0}&token={1}{2}",loginresult.data.login_info.uin, loginresult.data.login_info.token,signkey);
        //    string encode = CommonLib.MD5.Encrypt(mdstr, 32);
        //    string data = string.Format("uin={0}&token={1}&sign={2}", loginresult.data.login_info.uin, loginresult.data.login_info.token, encode);
        //    string result = PostTemp(data, url);

        //    return result;
        //}

        //public static string urlex = @"http://192.168.66.88:8888/check_device";
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// check_device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = string.Empty;
            string result = SSTest.Comm.Query.check_device("1", "fackbook", "1");

            ResultCheck resultmodel = CommMeth.JsonDeserialize<ResultCheck>(result);
            richTextBox1.Text = result;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = string.Empty;
            string result = SSTest.Comm.Query.login("0", "1", "fackbook", "1", null, null);

            loginresult = CommMeth.JsonDeserialize<ResultLogin>(result);
            richTextBox1.Text = result;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = string.Empty;
            si =  SSTest.Comm.Query.SelectServer(loginresult, 2);
            string result = SSTest.Comm.Query.login_server(loginresult.data.login_info.uin, loginresult.data.login_info.token, si);

            ResultLoginServer r = CommMeth.JsonDeserialize<ResultLoginServer>(result);
            sessionid = r.data.session;
            richTextBox1.Text = result;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                //server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);
                List<string> ls = new List<string>();
                ls.Add("backpack");

                //Typelist tl = new Typelist();
                //tl.type = new List<string>();
                //tl.type.Add("backpack");

                string tlist = CommMeth.JsonSerialize<List<string>>(ls);
                string tll = "{" + string.Format("\"type\":{0}", tlist) + "}";
                string aaa = SSTest.Comm.Query.getuserinfo(si.ip, si.port.ToString(), sessionid, tll);
                //string aaa = SSTest.Comm.Query.getuserinfo(si.ip, si.port.ToString(), sessionid, "{\"type\": [\"backpack\"]}");
                richTextBox1.Text = aaa;
                MUserInfo resultmodel = CommMeth.JsonDeserialize<MUserInfo>(aaa);
            }
            catch(Exception ex)
            {

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);
            string querydata = "&op=get&param=aaa";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid,"op_bag", querydata);
            richTextBox1.Text = aaa;
            MBag resultmodel = CommMeth.JsonDeserialize<MBag>(aaa);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //SSTest.Comm.server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);
            //string querydata = "&op=add&param=20000&addcount=10";
            //string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_bag", querydata);
            //richTextBox1.Text = aaa;

            //server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);

            List<object> param = new List<object>();
            param.Add(20000);
            param.Add(1);
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=add&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";

            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_bag", tl);
            richTextBox1.Text = aaa;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //SSTest.Comm.server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);
            //string querydata = "&op=del&param=20000&delcount=1";
            //string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_bag", querydata);
            //richTextBox1.Text = aaa;

            //server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);

            List<object> param = new List<object>();
            param.Add(20000);
            param.Add(1);
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=del&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";

            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_bag", tl);
            richTextBox1.Text = aaa;
        }

        /// <summary>
        /// 测试param
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            //server_info si = SSTest.Comm.Query.SelectServer(loginresult, 0);

            List<object> param = new List<object>();
            param.Add(20000);
            param.Add(1);
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=test&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";

            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_bag", tl);
            richTextBox1.Text = aaa;
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        //enter
        private void button10_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            int level = 0;
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                level = Convert.ToInt32(textBox1.Text);
            }
            param.Add(level);
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=enter&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }

        //leave
        private void button11_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=leave&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }

        //select
        private void button12_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            int level = 0;
            if (!string.IsNullOrWhiteSpace(textBox2.Text))
            {
                level = Convert.ToInt32(textBox2.Text);
            }
            param.Add(level);
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=select&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }
        //win
        private void button13_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            param.Add(1);//1.win,0.lose
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=battle_result&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }
        //lose
        private void button14_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            param.Add(0);//1.win,0.lose
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=battle_result&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            //param.Add(0);//1.win,0.lose
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=add_ticket&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            //param.Add(0);//1.win,0.lose
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=get_info&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_rewardmatch", tl);
            richTextBox1.Text = aaa;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            List<object> param = new List<object>();
            //param.Add(0);//1.win,0.lose
            string paramstr = CommMeth.JsonSerialize<List<object>>(param);
            string tl = "&op=get_info&param={" + string.Format("\"paramlist\":{0}", paramstr) + "}";
            string aaa = SSTest.Comm.Query.querygateway(si.ip, si.port.ToString(), sessionid, "op_ladder", tl);

            aaa = CommMeth.FormatJsonStr(aaa);
            MBaseProtobuf resultmodel = CommMeth.JsonDeserialize<MBaseProtobuf>(aaa);

            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(resultmodel.data);
            writer.Flush();

            var model = TypeModel.Create();
            stream.Position = 0;
            F_T_CurTime fct = (F_T_CurTime)model.Deserialize(stream, null, typeof(F_T_CurTime));

            richTextBox1.Text = aaa + " time:" + fct.time;
        }

        //private void 上传ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    openFileDialog1.Filter = "文件(*.*)|*.*";
        //    if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //    {
        //        string[] files = openFileDialog1.FileNames;//用户选中的全部文件，如果没有设置多选的话可以用FileName查找文件
        //        foreach (string filepath in files)
        //        {
        //        }
        //    }
        //    MessageBox.Show("成功");
        //}

        //public static bool UploadFile(string localFilePath, string serverFolder,bool reName)
        //{
        //    string fileNameExt, newFileName, uriString;
        //    if (reName)
        //    {
        //        fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf(".") + 1);
        //        newFileName = DateTime.Now.ToString("yyMMddhhmmss") + fileNameExt;
        //    }
        //    else
        //    {
        //        newFileName = localFilePath.Substring(localFilePath.LastIndexOf("\\")+1);
        //    } 
        //    if (!serverFolder.EndsWith("/") && !serverFolder.EndsWith("\\"))
        //    {
        //        serverFolder = serverFolder + "/";
        //    }
 
        //    uriString = serverFolder + newFileName;   //服务器保存路径
        //    /**//// 创建WebClient实例
        //    WebClient myWebClient = new WebClient();
        //    myWebClient.Credentials = CredentialCache.DefaultCredentials;
 
        //    // 要上传的文件
        //    FileStream fs = new FileStream(newFileName, FileMode.Open, FileAccess.Read);
        //    BinaryReader r = new BinaryReader(fs);
        //    try
        //    {
        //        //使用UploadFile方法可以用下面的格式
        //        //myWebClient.UploadFile(uriString,"PUT",localFilePath);
        //        byte[] postArray = r.ReadBytes((int)fs.Length);
        //        Stream postStream = myWebClient.OpenWrite(uriString, "PUT");
        //        if (postStream.CanWrite)
        //        {
        //            postStream.Write(postArray, 0, postArray.Length);
        //        }
        //        else
        //        {
        //            MessageBox.Show("文件目前不可写！");
        //        }
        //        postStream.Close();
        //    }
        //    catch
        //    {
        //        //MessageBox.Show("文件上传失败，请稍候重试~");
        //        return false;
        //    }
 
        //    return true;
        //}
    }


}
