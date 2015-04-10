using Assets.SuperStar.Scripts.Network.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace SSTest.Comm
{
    /// <summary>
    /// 公用方法类
    /// </summary>
    public class CommMeth
    {
        /// <summary>
        /// post方法
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="url"></param>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        public static string HttpPost(string postData, string url, string ContentType = "application/x-www-form-urlencoded")
        {
            string responseFromServer = string.Empty;
            if (string.IsNullOrWhiteSpace(url))
            {
                return responseFromServer;
            }

            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                request.ContentType = ContentType;
                request.ContentLength = byteArray.Length;
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }   
                using (WebResponse response = request.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return responseFromServer;
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string MD5Encrypt(string data)
        {
            byte[] result = Encoding.Default.GetBytes(data);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "").ToLower();
        }

        /// <summary>
        /// 格式化json字串（处理转义符号）
        /// </summary>
        /// <param name="jsonstr"></param>
        /// <returns></returns>
        public static string FormatJsonStr(string jsonstr)
        {
            jsonstr = jsonstr.Replace("\"{", "{");
            jsonstr = jsonstr.Replace("}\"", "}");
            jsonstr = jsonstr.Replace("\\\"", "\"");

            return jsonstr;
        }

        #region Json序列化与反序列化
        public static string JsonSerialize<T>(T jsonobj)
        {
            return LitJson.JsonMapper.ToJson(jsonobj);
        }
        public static T JsonDeserialize<T>(string jsonstr)
        {
            return LitJson.JsonMapper.ToObject<T>(jsonstr);
        } 
        #endregion
    }

    //请求逻辑类
    public class Query
    {
        /// <summary>
        /// 验签MD5秘钥
        /// </summary>
        public static string signkey = @"!1AQaq#3";
        /// <summary>
        /// 登陆入口（目前是测试地址）
        /// </summary>
        public static string urlroot = @"http://192.168.66.180:8888/";
        //public static string urlroot = @"http://192.168.66.137:8888/";
        //public static string urlroot = @"http://192.168.66.88:8888/";
        /// <summary>
        /// 版本验证
        /// </summary>
        /// <param name="dev">设备号</param>
        /// <param name="plat">平台</param>
        /// <param name="version">版本号</param>
        /// <returns></returns>
        public static string check_device(string dev, string plat, string version)
        {
            string url = urlroot + "check_device";
            string data = string.Format("dev={0}&plat={1}&version={2}", dev, plat, version);
            return  CommMeth.HttpPost(data, url);
        }

        //public static void GetTTYpe<T>(T aa)
        //{
        //    var aaa = typeof(T).GetGenericArguments();
        //    switch(aaa.ToString())
        //    {
        //        case (aa.GetType()).ToString():
        //            {

        //            }
        //            break;
        //    }
        //}

        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="tp">登陆模式 0.游客登陆 1.平台登陆</param>
        /// <param name="Dev">设备号</param>
        /// <param name="Plat">平台</param>
        /// <param name="version">版本号</param>
        /// <param name="Id">玩家ID（游客登陆时可为空）</param>
        /// <param name="Name">玩家名（游客登陆时可为空）</param>
        /// <returns></returns>
        public static string login(string tp, string Dev, string Plat, string version, string Id, string Name)
        {
            string url = urlroot + "login";
            string data = string.Format("tp={0}&dev={1}&plat={2}&version={3}&Id={4}&Name={5}", tp, Dev, Plat, version, Id, Name);

            try
            {
                return CommMeth.HttpPost(data, url);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 选择服务器
        /// </summary>
        /// <param name="loginresult">登陆请求返回内容</param>
        /// <param name="servername">选择的服务器名（display_info.name）</param>
        /// <returns></returns>
        public static server_info SelectServer(ResultLogin loginresult, string servername)
        {
            if (loginresult == null || loginresult.data == null || loginresult.data.server_list == null || loginresult.data.server_list.Count == 0)
            {
                return null;
            }

            //server_info si = loginresult.data.server_list[0].server_info;
            server_node si = loginresult.data.server_list.Find(s => s.display_info.name == servername);

            if (si == null || si.server_info == null)
            {
                return null;
            }

            return si.server_info;
        }
        public static server_info SelectServer(ResultLogin loginresult, int index)
        {
            if (loginresult == null || loginresult.data == null || loginresult.data.server_list == null || loginresult.data.server_list.Count == 0)
            {
                return null;
            }

            //if (loginresult.data.server_list[index] == null)
            //{
            //    return null;
            //}

            server_node si = loginresult.data.server_list.Find(s => s.display_info.name == "server137");

            //server_node si = loginresult.data.server_list[index];
            if (si == null || si.server_info == null)
            {
                return null;
            }

            return si.server_info;
        }
        
        /// <summary>
        /// 登陆选择服务器
        /// </summary>
        /// <param name="uin">玩家id</param>
        /// <param name="token">验证码</param>
        /// <param name="si">服务器信息</param>
        /// <returns></returns>
        public static string login_server(int uin,string token, server_info si)
        {
            if (si == null)
            {
                return null;
            }
            string url = string.Format(@"http://{0}:{1}/login", si.ip, si.port);
            string mdstr = string.Format(@"uin={0}&token={1}{2}", uin, token, signkey);
            string encode = CommMeth.MD5Encrypt(mdstr).ToLower();
            string data = string.Format("uin={0}&token={1}&sign={2}", uin, token, encode);
            string result = CommMeth.HttpPost(data, url);
            return CommMeth.FormatJsonStr(result);
        }

        public static string getuserinfo(string ip,string port, string session, string type)
        {
            string url = string.Format(@"http://{0}:{1}/get_user_info", ip, port);
            string data = string.Format(@"session_id={0}&info_type={1}", session, type);
            string result = CommMeth.HttpPost(data, url);
            result = CommMeth.FormatJsonStr(result);
            //ResultLoginServer resultmodel = CommMeth.JsonDeserialize<ResultLoginServer>(result);
            return result;
        }

        public static string querygateway(string ip, string port, string session,string modename, string querystr)
        {
            string url = string.Format(@"http://{0}:{1}/{2}", ip, port, modename);
            string data = string.Format(@"session_id={0}{1}", session, querystr);
            string result = CommMeth.HttpPost(data, url);
            result = CommMeth.FormatJsonStr(result);
            //ResultLoginServer resultmodel = CommMeth.JsonDeserialize<ResultLoginServer>(result);
            return result;
        }

        /// <summary>
        /// 登陆后的后续操作，具体结构待定
        /// </summary>
        /// <param name="url"></param>
        /// <param name="session_id"></param>
        /// <param name="paramstr"></param>
        /// <returns></returns>
        public static string OP(string url,string session_id,string paramstr)
        {
            if(string.IsNullOrWhiteSpace(url)
                || string.IsNullOrWhiteSpace(session_id))
            {
                return null;
            }

            string data = string.Format("session_id={0}&{1}", session_id, paramstr);
            return CommMeth.HttpPost(data, url);
        }
    }
}
