using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SuperStar.Scripts.Network.model
{
    #region ��½��ط��ؽṹ
    #region �汾��֤
    public class ResultCheck : ResultBase
    {
        public int code { get; set; }
        public string version_update_url { get; set; }
        public string version_url { get; set; }
    }
    #endregion

    #region ��½
    public class ResultLogin : ResultBase
    {
        public data_server data { get; set; }
    }
    public class data_server
    {
        public login_info login_info { get; set; }
        public List<server_node> server_list { get; set; }
    }
    public class login_info
    {
        public int uin { get; set; }
        public string token { get; set; }
    }
    public class server_node
    {
        public display_info display_info { get; set; }
        public server_info server_info { get; set; }
    }
    public class display_info
    {
        public string name { get; set; }
        public string type { get; set; }
        public int state { get; set; }
    }
    public class server_info
    {
        public string ip { get; set; }
        public int port { get; set; }
    }
    #endregion

    #region ѡ�������
    public class ResultLoginServer : ResultBase
    {
        public data_session data { get; set; }
    }
    public class data_session
    {
        public string session { get; set; }
    }
    #endregion
    #endregion
}
