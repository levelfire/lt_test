using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using servercore;
using Server_Form.Protorol;

namespace Server_Form
{
    /// <summary>
    /// 测试用会话Session类
    /// </summary>
    public class TTestSession : TSessionBase
    {
        //public Table table;
        //public int seat;
        //public int nPlayerID;
        public Player SessionPlayer;
        private bool m_bFirstRequest = true;

        //初始化回话数据（连接时）
        public override void SubInit(object socket)
        {
            //Player pPlayer = new Player();
            //nPlayerID = pPlayer.m_nPlayerID;

            BFirstRe = this.m_bFirstRequest;
        }

        public override void Close()
        {
            base.Close();

            //if (nPlayerID > 0)
            //{
            //    Player pOutplayer;
            //    Player.AllPlayerList.TryRemove(nPlayerID, out pOutplayer);
            //}
            if (null != SessionPlayer)
            {
                // 下线，清理队伍 [12/5/2011 test]
                SessionPlayer.Offline();
                Player pOutplayer;
                Player.AllPlayerList.TryRemove(SessionPlayer.NPlayerID, out pOutplayer);
            }
        }

        /// <summary>
        /// 重写错误处理方法, 返回消息给客户端
        /// </summary>
        protected override void OnDatagramDelimiterError()
        {
            base.OnDatagramDelimiterError();

            base.SendDatagram("datagram delimiter error");
        }

        /// <summary>
        /// 重写错误处理方法, 返回消息给客户端
        /// </summary>
        protected override void OnDatagramOversizeError()
        {
            base.OnDatagramOversizeError();

            base.SendDatagram("datagram over size");
        }

        public void OnCheckFrameType_UTS(byte[] datagramBytes)
        {
            ProtorlEnum.FrameInfo data_info;
            data_info.nFrameType = datagramBytes[6];
            switch (data_info.nFrameType)
            {
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_Keep:
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_Login:
                    {
                        Player pPlayer;
                        int nLoginResult = Player.OnUserLogin(datagramBytes, out pPlayer);

                        PlayerLoginResult stLoginResult;
                        stLoginResult.nResult = nLoginResult;

                        if (nLoginResult > 0)
                        {
                            stLoginResult.nPlayerID = pPlayer.NPlayerID;
                            stLoginResult.nResourceID = pPlayer.NResourceID;
                            stLoginResult.nBody = pPlayer.NBody;
                            stLoginResult.nLogStageID = pPlayer.m_nLogStageID;
                            stLoginResult.nLogStepID = pPlayer.m_nLogStepID;
                        }
                        else
                        {
                            stLoginResult.nPlayerID = 0;
                            stLoginResult.nResourceID = 0;
                            stLoginResult.nBody = 0;
                            stLoginResult.nLogStageID = 0;
                            stLoginResult.nLogStepID = 0;
                        }

                        byte[] btLoginResult = Common.Method.StructToBytes(stLoginResult);
                        byte[] allInfo;

                        if (null != pPlayer)
                        {
                            

                            byte[] btname = System.Text.Encoding.UTF8.GetBytes(pPlayer.SzName);
                            allInfo = new byte[btLoginResult.Length + 4 + btname.Length];
                            byte[] namelen = System.BitConverter.GetBytes(btname.Length);


                            //byte[] tempByte = new byte[4 + 4];
                            //byte[] nResult = System.BitConverter.GetBytes(nLoginResult);
                            //byte[] nID;
                            //nResult.CopyTo(tempByte, 0);
                            btLoginResult.CopyTo(allInfo, 0);
                            namelen.CopyTo(allInfo, btLoginResult.Length);
                            btname.CopyTo(allInfo, 4 + btLoginResult.Length);

                            SessionPlayer = pPlayer;
                            pPlayer.pSession = this;


                            //nID = System.BitConverter.GetBytes(SessionPlayer.m_nPlayerID);
                            //nID.CopyTo(tempByte, 4);

                            pPlayer.MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                                        (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_Login,
                                                        allInfo);
                        }
                        else
                        {
                            //nID = System.BitConverter.GetBytes(0);
                            //nID.CopyTo(tempByte, 4);
                            TTestSession.MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                                        (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_Login,
                                                        btLoginResult,
                                                        this);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_SingleEnterStep:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnCreateStep(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_PlayerMove:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnMove(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_PlayerKnock:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnKnock(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_PlayerStopMove:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnStopMove(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_QueryTeam:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnQueryTeamList(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_CreateTeam:
                    {
                        if (null != SessionPlayer)
                        {
                            int nTeamIndex = SessionPlayer.OnCreateTeam(datagramBytes);
                            //base.OnDatagramHandled(); 

                            base.Logout = "Player " + SessionPlayer.NPlayerID.ToString() + " CreateTeam " + nTeamIndex.ToString();
                            base.OnDatagramlogOut();  // 输出
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_JoinTeam:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnJoinTeam(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_JoinTeamWithPassword:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnJoinTeamWithPassword(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_JoinTeamRandom:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnJoinTeamRandom(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_PlayerReady:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnSingleReady(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_PlayerNotReady:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnSingleNotReady(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_TeamReady:
                    {
                        if (null != SessionPlayer)
                        {
                            int nResult = SessionPlayer.OnTeamReady(datagramBytes);

                            if (nResult > 0)
                            {
                                base.Logout = "Battle Start " + SessionPlayer.CurTeam.nTeamIndex.ToString() + " vs " + nResult.ToString();
                                base.OnDatagramlogOut();  // 输出
                            }
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_TeamNotReady:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnTeamNotReady(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_OpenSeat:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnOpenSeat(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_CloseSeat:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnCloseSeat(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_KickPlayer:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnKickPlayer(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_KickAndClose:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnKickAndClose(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_QuitTeam:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnQuitTeam(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_SingleTimeOut:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnSingleTimeOut(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_QueryNextSubStep:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnQueryNextSubStep(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Find(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Find:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Find(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Speed:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Speed(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Invincible:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Invincible(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_AOE:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_AOE(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Stun:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Stun(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_TimeSlow:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_TimeSlow(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_AT:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_AT(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Trans:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Trans(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_UseItem_Hammer:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnUserItem_Hammer(datagramBytes);
                        }
                    }
                    break;
                case (byte)ProtorlEnum.FrameType_UserToServer.FrameType_QueryStageScore:
                    {
                        if (null != SessionPlayer)
                        {
                            SessionPlayer.OnQueryStageScore(datagramBytes);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public static void MakeAndSendDatagram(byte nGroup, byte nType, byte[] DataByte, TTestSession tSession)
        {
            byte[] tempByte = new byte[1 + 1 + 4 + 1 + 1 + DataByte.Length];

            Encoding.ASCII.GetBytes("<", 0, 1, tempByte, 0);
            Encoding.ASCII.GetBytes(">", 0, 1, tempByte, tempByte.Length - 1);

            byte[] btGroup = { nGroup };
            byte[] btType = { nType };
            byte[] btLenth = System.BitConverter.GetBytes(tempByte.Length - 2);

            btLenth.CopyTo(tempByte, 1);
            btGroup.CopyTo(tempByte, 5);
            btType.CopyTo(tempByte, 6);

            DataByte.CopyTo(tempByte, 7);

            //发送信息
            tSession.SendDatagram(tempByte);
        }

        /// <summary>
        /// 重写 AnalyzeDatagram 方法, 调用数据存储方法
        /// </summary>
        protected override void AnalyzeDatagram(byte[] datagramBytes)
        {

            int nFrameLenth;
            ProtorlEnum.FrameInfo data_info;

            // AS3在初次连接时会默认发送一个请求，需要特殊处理 [12/13/2011 test]
            if (22 == datagramBytes.Length)
            {
                string sztemp = System.Text.Encoding.ASCII.GetString(datagramBytes, 0, datagramBytes.Length);
                if (sztemp == "<policy-file-request/>")
                {
                    /**
                        注意此处xml文件的内容，为纯字符串，没有xml文档的版本号
                    */
                    string xmlS = "<cross-domain-policy>" +
                            "<allow-access-from domain=\"*\" to-ports=\"*\" />" +
                            "</cross-domain-policy>\0";

                    //回复
                    byte[] packetSend = System.Text.Encoding.ASCII.GetBytes(xmlS);
                    base.SendDatagram(packetSend);
                    return;
                }
            }

            nFrameLenth = System.BitConverter.ToInt32(datagramBytes, 0);
            if (nFrameLenth <= 6)
            {
                base.OnDatagramError();  // 错误包
                return;
            }
            data_info.nFrameGroup = datagramBytes[5];
            base.OnDatagramAccepted();  // 模拟接收到一个完整的数据包

            switch (data_info.nFrameGroup)
            {
                case (byte)ProtorlEnum.FrameGroup.FrameGroup_UserToServer:
                    {
                        OnCheckFrameType_UTS(datagramBytes);
                        base.OnDatagramHandled();  // 模拟已经处理（存储）了数据包
                    }
                    break;
                case (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser:
                    {
                    }
                    break;
                default:
                    base.OnDatagramError();  // 错误包
                    break;
            }

            //string datagramText = Encoding.ASCII.GetString(datagramBytes);

            //string clientName = string.Empty;
            //int datagramTextLength = 0;

            //int n = datagramText.IndexOf(',');  // 格式为 <C12345,0000000000,****>
            //if (n >= 1)
            //{
            //    clientName = datagramText.Substring(1, n - 1);
            //    try
            //    {
            //        datagramTextLength = int.Parse(datagramText.Substring(n + 1, 10));
            //    }
            //    catch
            //    {
            //        datagramTextLength = 0;
            //    }
            //}

            //base.OnDatagramAccepted();  // 模拟接收到一个完整的数据包

            //if (!string.IsNullOrEmpty(clientName) && datagramTextLength > 0)
            //{

            //    if (datagramTextLength == datagramBytes.Length)
            //    {
            //        base.SendDatagram("<OK: " + clientName + ", datagram length = " + datagramTextLength.ToString() + ">");

            //        this.Store(datagramBytes);
            //        base.OnDatagramHandled();  // 模拟已经处理（存储）了数据包
            //    }
            //    else
            //    {
            //        base.SendDatagram("<ERROR: " + clientName + ", error length, datagram length = " + datagramTextLength.ToString() + ">");
            //        base.OnDatagramError();  // 错误包
            //    }
            //}
            //else if (string.IsNullOrEmpty(clientName))
            //{
            //    base.SendDatagram("client: no name, datagram length = " + datagramTextLength.ToString());
            //    base.OnDatagramError();
            //}
            //else if (datagramTextLength == 0)
            //{
            //    base.SendDatagram("client: " + clientName + ", datagram length = " + datagramTextLength.ToString());
            //    base.OnDatagramError();  // 错误包
            //}
        }
        /// <summary>
        /// 自定义的数据存储方法
        /// </summary>
        private void Store(byte[] datagramBytes)
        {
            if (this.DatabaseObj == null)
            {
                return;
            }

            TTestAccessDatabase db = this.DatabaseObj as TTestAccessDatabase;
            db.Store(datagramBytes, this);
        }
    }
}
