using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Server_Form.GameData;
using Server_Form.GameInse;
using System.Collections.Concurrent;
using Server_Form.Protorol;
using System.Runtime.InteropServices;
using System.Data;
using System.Threading;
using Server_Form.LogicMoudle;

namespace Server_Form
{
    public class Player
    {
        public enum PlayerFlag
        {
            IsRobot = 0x0001,
        }

        public enum PlayerState
        {
            Normal      =   0,
            InTeam      =   1,
            InScence    =   2,
        }

        public enum PlayerRoomState
        {
            no          = 0,
            Normal,
            Ready,
        }

        public enum PlayerScenceState
        {
            no              =   0,
            Stop            =   1,
            Moving          =   2,
            UnMovealbe      =   3,
            Stun            =   4,
            Fear            =   5,
        }

        public TTestSession pSession;

        private int nFlag;
        private int m_nPlayerID;
        private int m_nResourceID;
        private int m_nBody;
        private string m_szName;

        public string SzName
        {
            get { return m_szName; }
            set { m_szName = value; }
        }

        public int m_nLogStageID;
        public int m_nLogStepID;

        public int NBody
        {
            get { return m_nBody; }
            set { m_nBody = value; }
        }

        public int NFlag
        {
            get { return nFlag; }
            set { nFlag = value; }
        }
        public int NPlayerID
        {
            get { return m_nPlayerID; }
            set { m_nPlayerID = value; }
        }
        public int NResourceID
        {
            get { return m_nResourceID; }
            set { m_nResourceID = value; }
        }
        public int m_nTableID;
        //private int m_nSeatIndex;

        //public int NSeatIndex
        //{
        //    get { return m_nSeatIndex; }
        //    set { m_nSeatIndex = value; }
        //}


        /// <summary>
        /// 敲击范围
        /// </summary>
        public int m_nR;

        /// <summary>
        /// 移动速度
        /// </summary>
        public int m_nSpeed;

        public const int m_nTeamListCount = 10;

        private PlayerState m_nPlayerState;
        private PlayerRoomState m_nPlayerRoomState;
        private PlayerScenceState m_nPlayerScenceState;

        public PlayerState NPlayerState
        {
            get { return m_nPlayerState; }
            set { m_nPlayerState = value; }
        }
        public PlayerRoomState NPlayerRoomState
        {
            get { return m_nPlayerRoomState; }
            set { m_nPlayerRoomState = value; }
        }
        public PlayerScenceState NPlayerScenceState
        {
            get { return m_nPlayerScenceState; }
            set { m_nPlayerScenceState = value; }
        }

        public Step m_pCurStep;
        public SubStep m_pCurSubStep;
        private Team m_pCurTeam;

        internal Team CurTeam
        {
            get { return m_pCurTeam; }
            set { m_pCurTeam = value; }
        }

        public int nX_MoveForm
        {
            get;
            set;
        }
        public int nY_MoveForm
        {
            get;
            set;
        }
        public int nX_MoveTo
        {
            get;
            set;
        }
        public int nY_MoveTo
        {
            get;
            set;
        }

        //public static int iID = 1;

        //进入关卡的玩家列表
        public static ConcurrentDictionary<int, Player> AllPlayerList = new ConcurrentDictionary<int, Player>(Define.concurrencyLevel, Define.initialCapacity);

        /// <summary>
        /// 图中不同点的列表
        /// </summary>
        public ConcurrentDictionary<int, Item> ItemList = new ConcurrentDictionary<int, Item>(Define.concurrencyLevel, Define.initialCapacity);

        public Player(int nPlayerID,bool bIsRobort = false)
        {
            m_nPlayerID = nPlayerID;

            //DataTable dt = DAL.Player.GetPlayerInfo(nPlayerID);
            DataTable dt = null;
            m_nResourceID = Convert.ToInt32(dt.Rows[0]["ResourceID"]);
            m_nBody = Convert.ToInt32(dt.Rows[0]["Bodysize"]);
            m_szName = dt.Rows[0]["NickName"].ToString();

            //dt = DAL.Player.GetPlayerLogStep(nPlayerID);
            dt = null;
            if (dt.Rows.Count > 0)
            {
                m_nLogStageID = Convert.ToInt32(dt.Rows[0]["StageID"]);
                m_nLogStepID = Convert.ToInt32(dt.Rows[0]["StepID"]);
            }
            else
            {
                m_nLogStageID = 1;
                m_nLogStepID = 0;
            }

            //dt = DAL.Item.GetPlayerItemInfo(nPlayerID);
            dt = null;
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    Item pItem = new Item();
                    pItem.NID = System.Convert.ToInt32(dt.Rows[i]["ItemDataID"]);
                    pItem.NCount = System.Convert.ToInt32(dt.Rows[i]["Count"]);
                    pItem.NInsID = System.Convert.ToInt32(dt.Rows[i]["InsID"]);

                    ItemList.TryAdd(pItem.NInsID, pItem);
                }
            }



            //foreach (DataRow dr in dt.Rows)
            //{
            //    MonsterData data = new MonsterData();
            //    data.ID = Convert.ToInt32(dr["ID"]);
            //    data.Body = Convert.ToInt32(dr["body"]);
            //    data.Speed = Convert.ToInt32(dr["speed"]);
            //    data.MoveWay = Convert.ToInt32(dr["moveway"]);
            //    data.HitEffect = Convert.ToInt32(dr["hiteffect"]);
            //    data.HitTime = Convert.ToInt32(dr["hittime"]);
            //    MonsterDataList.TryAdd(data.ID, data);
            //}

            NPlayerState = PlayerState.Normal;
            NPlayerRoomState = PlayerRoomState.no;
            NPlayerScenceState = PlayerScenceState.no;

            if (bIsRobort)
            {
                nFlag |= (int)PlayerFlag.IsRobot;
            }
            else
            {
                nFlag = 0;
            }

            m_nTableID = 0;
            //m_nSeatIndex = 0;

            m_nR = 10;

            AllPlayerList.TryAdd(nPlayerID, this);
        }

        public bool IsRobort()
        {
            return (nFlag & (int)PlayerFlag.IsRobot) > 0;
        }
        /// <summary>
        /// 设置玩家移动状态
        /// </summary>
        /// <param nState="PlayerScenceState">需要修改的移动状态</param>
        /// <param stMove="PlayerMove">坐标信息</param>
        /// <returns></returns>
        private void SetMoveState(PlayerScenceState nState, object stMove)
        {
            switch (nState)
            {
                case PlayerScenceState.Stop:
                    {
                        NPlayerScenceState = PlayerScenceState.Stop;
                        nX_MoveForm = 0;
                        nY_MoveForm = 0;
                        nX_MoveTo = 0;
                        nY_MoveTo = 0;
                    }
                    break;
                case PlayerScenceState.Moving:
                    {
                        NPlayerScenceState = PlayerScenceState.Moving;
                        if (null != stMove)
                        {
                            nX_MoveForm = ((PlayerMove)stMove).nFromX;
                            nY_MoveForm = ((PlayerMove)stMove).nFromY;
                            nX_MoveTo = ((PlayerMove)stMove).nToX;
                            nY_MoveTo = ((PlayerMove)stMove).nToY;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public static int OnUserLogin(byte[] datagramBytes,out Player pPlayer)
        {
            pPlayer = null;

            byte[] nLenthName = new byte[4];
            byte[] nLenthPassword = new byte[4];

            //datagramBytes.CopyTo(byte_Name, 0);
            Array.Copy(datagramBytes, 7, nLenthName, 0, 4);
            Array.Copy(datagramBytes, 11, nLenthPassword, 0, 4);

            ////如果有需求，则反序int
            //if (System.BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(nLenthName, 0, 4);
            //    Array.Reverse(nLenthPassword, 0, 4);
            //}

            //int nLenthN = System.BitConverter.ToInt32(nLenthName, 0);
            //int nLenthP = System.BitConverter.ToInt32(nLenthPassword, 0);
            //string sName = System.Text.Encoding.ASCII.GetString(datagramBytes, 15, nLenthN);
            //string sPassword = System.Text.Encoding.ASCII.GetString(datagramBytes, 15 + nLenthN, nLenthP);

            PlayerLogin stLogin;
            stLogin.nLenthName = System.BitConverter.ToInt32(nLenthName, 0);
            stLogin.nLenthPassword = System.BitConverter.ToInt32(nLenthPassword, 0);
            stLogin.szName = System.Text.Encoding.ASCII.GetString(datagramBytes, 15, stLogin.nLenthName);
            stLogin.szPassword = System.Text.Encoding.ASCII.GetString(datagramBytes, 15 + stLogin.nLenthName, stLogin.nLenthPassword);

            //int UserID = DAL.User.VerifyUsers(stLogin.szName, stLogin.szPassword);
            int UserID = 0;
            if (0 <= UserID)
            {
                if (AllPlayerList.ContainsKey(UserID))
                {
                    return -1;
                }
                pPlayer = new Player(UserID);
                return 1;
            }
            return 0;
        }

        public void OnCreateStep(byte[] datagramBytes)
        {
            PlayerCreateStep sCStep = (PlayerCreateStep)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerCreateStep));

            byte[] nResult;
            Step pStep;
            //根据场景创建是否成功，返回不同结构
            if (Step.CreateScence(sCStep.nStageID, sCStep.nStepID, out pStep))
            {
                //成功的话则返回进入的关卡的信息
                m_pCurStep = pStep;
                m_pCurSubStep = m_pCurStep.cdSubList[1];

                byte[] tempByte;
                Step.MakeByte_EnterSubStep(sCStep.nStageID,sCStep.nStepID, 1, pStep, out tempByte);

                byte[] DataByte = new byte[4 + tempByte.Length];
                nResult = System.BitConverter.GetBytes(1);

                nResult.CopyTo(DataByte, 0);
                tempByte.CopyTo(DataByte, 4);

                m_pCurStep.AddPlayer(this);

                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_SingleEnterStep, 
                                       DataByte);

                NPlayerState = PlayerState.InScence;
                NPlayerScenceState = PlayerScenceState.Stop;

                m_pCurStep.nStartTime = DateTime.Now;
            }
            else
            {
                //失败则只返回一个result
                nResult = System.BitConverter.GetBytes(0);
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_SingleEnterStep,
                                       nResult);
            }
        }

        public void OnMove(byte[] datagramBytes)
        {
            PlayerMove stMove = (PlayerMove)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerMove));
            SetMoveState(PlayerScenceState.Moving, stMove);
            
            if (null != m_pCurStep)
            {
                if (m_pCurStep.NStepType == Step.StepType.Battle)
                {
                    PlayerMoveInfo stPMove;
                    stPMove.nResult = 1;
                    stPMove.nPlayerID = m_nPlayerID;
                    stPMove.nFromX = stMove.nFromX;
                    stPMove.nFromY = stMove.nFromY;
                    stPMove.nToX = stMove.nToX;
                    stPMove.nToY = stMove.nToY;
                    byte[] btMove = Common.Method.StructToBytes(stPMove);
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamA,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerMove,
                                 btMove);
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamB,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerMove,
                                 btMove);
                }
                else
                {
                    PlayerMoveResult stResult;
                    stResult.nResult = 1;
                    byte[] btResult = Common.Method.StructToBytes(stResult);
                    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_Move,
                                 btResult);
                }
            }
        }
        public void OnStopMove(byte[] datagramBytes)
        {
            PlayerStopMove stStopMove = (PlayerStopMove)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerStopMove));
            SetMoveState(PlayerScenceState.Stop,null);

            PlayerStopMoveInfo stResult;
            stResult.nPlayerID = m_nPlayerID;
            stResult.nX = stStopMove.nX;
            stResult.nY = stStopMove.nY;

            byte[] btResult = Common.Method.StructToBytes(stResult);

            if (null != m_pCurStep)
            {
                if (m_pCurStep.NStepType == Step.StepType.Battle)
                {
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamA,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerStopMove,
                                 btResult);
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamB,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerStopMove,
                                 btResult);
                }
                else
                {
                    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_StopMove,
                                 btResult);
                }
            }
        }
        public void OnQueryTeamList(byte[] datagramBytes)
        {
            PlayerQueryTeamResult stQueryResult;
            stQueryResult.nPlayerID = m_nPlayerID;
            stQueryResult.nTeamCount = 0;

            byte[] headByte = new byte[Marshal.SizeOf(stQueryResult)];
            byte[] AllByte;

            lock (Team.plock)
            {
                if (Team.cdicWaitTeams.Count > m_nTeamListCount)
                {
                    stQueryResult.nTeamCount = m_nTeamListCount;

                    AllByte = Common.Method.StructToBytes(stQueryResult);

                    List<int> IndexList = new List<int>();

                    foreach (Team t in Team.cdicWaitTeams.Values)
                    {
                        IndexList.Add(t.nTeamIndex);
                    }

                    int index = m_nTeamListCount;
                    while (index > 0)
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        int rIndex = r.Next(0, IndexList.Count);

                        PlayerTeamListInfo stInfo;
                        stInfo.nHasPassword = 0;
                        stInfo.nMaxMemberCount = 0;
                        stInfo.nMember = 0;
                        stInfo.nTeamIndex = 0;
                        stInfo.nBattleType = 0;
                        //stInfo.nNameLenth = 0;
                        //stInfo.szName = null;

                        PlayerTeamNameInfo stNameInfo;
                        stNameInfo.nNameLenth = 0;
                        stNameInfo.btName = null;

                        stInfo.Make(Team.cdicWaitTeams[IndexList[rIndex]]);
                        stNameInfo.Make(Team.cdicWaitTeams[IndexList[rIndex]]);
                        //byte[] NameLByte = System.BitConverter.GetBytes(stNameInfo.nNameLenth);
                        //byte[] NameSByte = stNameInfo.btName;

                        byte[] infoByte = Common.Method.StructToBytes(stInfo);
                        byte[] tempByte = AllByte;

                        AllByte = new byte[tempByte.Length + infoByte.Length + 4 + stNameInfo.btName.Length];
                        tempByte.CopyTo(AllByte, 0);
                        infoByte.CopyTo(AllByte, tempByte.Length);
                        System.BitConverter.GetBytes(stNameInfo.nNameLenth).CopyTo(AllByte, tempByte.Length + infoByte.Length);
                        stNameInfo.btName.CopyTo(AllByte, tempByte.Length + infoByte.Length + 4);
                        --index;
                    }
                }
                else
                {
                    stQueryResult.nTeamCount = Team.cdicWaitTeams.Count;
                    AllByte = Common.Method.StructToBytes(stQueryResult);

                    if (0 < stQueryResult.nTeamCount)
                    {
                        foreach (Team t in Team.cdicWaitTeams.Values)
                        {
                            PlayerTeamListInfo stInfo;
                            stInfo.nHasPassword = 0;
                            stInfo.nMaxMemberCount = 0;
                            stInfo.nMember = 0;
                            stInfo.nTeamIndex = 0;
                            stInfo.nBattleType = 0;
                            //stInfo.nNameLenth = 0;
                            //stInfo.szName = null;

                            PlayerTeamNameInfo stNameInfo;
                            stNameInfo.nNameLenth = 0;
                            stNameInfo.btName = null;

                            //byte[] NameLByte = System.BitConverter.GetBytes(stNameInfo.nNameLenth);
                            //byte[] NameSByte = stNameInfo.btName;

                            stInfo.Make(t);
                            stNameInfo.Make(t);

                            byte[] infoByte = Common.Method.StructToBytes(stInfo);
                            byte[] tempByte = AllByte;
                            //AllByte = new byte[tempByte.Length + infoByte.Length];
                            AllByte = new byte[tempByte.Length + infoByte.Length + 4 + stNameInfo.btName.Length];
                            tempByte.CopyTo(AllByte, 0);
                            infoByte.CopyTo(AllByte, tempByte.Length);
                            System.BitConverter.GetBytes(stNameInfo.nNameLenth).CopyTo(AllByte, tempByte.Length + infoByte.Length);
                            stNameInfo.btName.CopyTo(AllByte, tempByte.Length + infoByte.Length + 4);
                        }
                    } 
                }
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_QueryTeam,
                                     AllByte);
            }
            
        }

        //玩家设置自己进入队伍后的状态
        public void SetJoinTeamState(Team pTeam,bool bIsLeader = false)
        {
            //调用队伍的加入成员函数
            pTeam.AddPlayer(this, bIsLeader);
        }

        public int OnCreateTeam(byte[] datagramBytes)
        {
            PlayerCreateTeamResult stResult;
            byte[] btResult;

            // 为方便测试，暂时开放同一人创建多个队伍，正式版本要增加判断 [11/21/2011 test]
            //if (NPlayerState != PlayerState.Normal)
            //{
            //    stResult.nPlayerID = m_nPlayerID;
            //    stResult.nTeamIndex = 0;
            //    stResult.nMemberCount = 0;
            //    btResult = Common.Method.StructToBytes(stResult);

            //    //SetMoveState(PlayerScenceState.Stop, null);
            //    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
            //                        (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_CreateTeam,
            //                         btResult);

            //    return 0;
            //}
            PlayerCreateTeam stCreateTeam;
            stCreateTeam.nPlayerID = System.BitConverter.ToInt32(datagramBytes, 7);
            stCreateTeam.nBattleType = System.BitConverter.ToInt32(datagramBytes, 11);
            stCreateTeam.nMemberCount = System.BitConverter.ToInt32(datagramBytes, 15);
            stCreateTeam.nNameLenth = System.BitConverter.ToInt32(datagramBytes, 19);
            stCreateTeam.nPasswordLenth = System.BitConverter.ToInt32(datagramBytes, 23);
            stCreateTeam.szName = System.Text.Encoding.UTF8.GetString(datagramBytes, 27, stCreateTeam.nNameLenth);
            stCreateTeam.szPassword = System.Text.Encoding.ASCII.GetString(datagramBytes, 27 + stCreateTeam.nNameLenth, stCreateTeam.nPasswordLenth);

            Team pTeam = new Team(stCreateTeam);

            stResult.nPlayerID = m_nPlayerID;
            stResult.nBattleType = (int)pTeam.NBattleType;
            stResult.nTeamIndex = pTeam.nTeamIndex;
            stResult.nMemberCount = pTeam.nMaxMemberCount;

            btResult = Common.Method.StructToBytes(stResult);

            //SetMoveState(PlayerScenceState.Stop, null);
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_CreateTeam,
                                 btResult);

            SetJoinTeamState(pTeam,true);
            return pTeam.nTeamIndex;
        }

        public void OnJoinTeam(byte[] datagramBytes)
        {
            PlayerJoinTeam stJoin = (PlayerJoinTeam)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerJoinTeam));
            JoinTeam(stJoin);
        }
        public void OnJoinTeamWithPassword(byte[] datagramBytes)
        {
            ////PlayerJoinTeamWithPassword stJoin = (PlayerJoinTeamWithPassword)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerJoinTeamWithPassword));

            PlayerJoinTeamWithPassword stJoin;
            stJoin.nPlayerID = System.BitConverter.ToInt32(datagramBytes, 7);
            stJoin.nTeamIndex = System.BitConverter.ToInt32(datagramBytes, 11);
            stJoin.nLenth = System.BitConverter.ToInt32(datagramBytes, 15);
            stJoin.szPassword = System.Text.Encoding.UTF8.GetString(datagramBytes, 19, stJoin.nLenth);

            JoinTeam(stJoin);
        }
        public void OnJoinTeamRandom(byte[] datagramBytes)
        {
            PlayerJoinTeamRandom stJoin = (PlayerJoinTeamRandom)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerJoinTeamRandom));
            JoinTeam(stJoin);
        }

        //离队
        public void OnQuitTeam(byte[] datagramBytes)
        {
            PlayerQuitTeam stQuit = (PlayerQuitTeam)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerQuitTeam));

            int nResult = 0;

            //检查人物ID是否符合
            if (m_nPlayerID != stQuit.nPlayerID)
            {
                goto Exit0;
            }

            //检查队伍是否存在
            if (!Team.cdicAllTeams.ContainsKey(stQuit.nTeamIndex))
            {
                goto Exit0;
            }
            //检查队伍是否匹配
            if (CurTeam != Team.cdicAllTeams[stQuit.nTeamIndex])
            {
                goto Exit0;
            }
            //检查队伍状态
            if (CurTeam.NFlag != (int)Team.TeamState.Normal)
            {
                goto Exit0;
            }
            ////检查队员是否是本队队长
            //if (CurTeam.Leader != this)
            //{
            //    goto Exit0;
            //}

            QuitTeam();
            nResult = 1;

        Exit0:
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_QuitTeam,
                                 System.BitConverter.GetBytes(nResult));
        }

        //准备
        public void OnSingleReady(byte[] datagramBytes)
        {
            PlayerReady stReady = (PlayerReady)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerReady));

            //检查队伍是否存在
            if (!Team.cdicAllTeams.ContainsKey(stReady.nTeamIndex))
            {
                return;
            }
            //检查队伍是否匹配
            if (CurTeam != Team.cdicAllTeams[stReady.nTeamIndex])
            {
                return;
            }

            // 队伍key现在改用座位号 [11/21/2011 test]
            ////检查队员是否存在
            //if (!CurTeam.cdicMembers.ContainsKey(m_nPlayerID))
            //{
            //    return;
            //}

            byte[] nResult;

            if (NPlayerRoomState == PlayerRoomState.Normal && CurTeam.NTeamState == Team.TeamState.Normal)
            {
                ChangeRoomState(PlayerRoomState.Ready);
                nResult = System.BitConverter.GetBytes(1);
            }
            else
            {
                nResult = System.BitConverter.GetBytes(0);
            }

            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_PlayerReady,
                                     nResult);
   
        }
        //取消准备
        public void OnSingleNotReady(byte[] datagramBytes)
        {
            PlayerReady stReady = (PlayerReady)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerReady));

            //检查队伍是否存在
            if (!Team.cdicAllTeams.ContainsKey(stReady.nTeamIndex))
            {
                return;
            }
            //检查队伍是否匹配
            if (CurTeam != Team.cdicAllTeams[stReady.nTeamIndex])
            {
                return;
            }
            // 队伍Key现在改用座位号 [11/21/2011 test]
            ////检查队员是否存在
            //if (!CurTeam.cdicMembers.ContainsKey(m_nPlayerID))
            //{
            //    return;
            //}

            byte[] nResult;

            if (NPlayerRoomState == PlayerRoomState.Ready && CurTeam.NTeamState == Team.TeamState.Normal)
            {
                ChangeRoomState(PlayerRoomState.Normal);
                nResult = System.BitConverter.GetBytes(1);
            }
            else
            {
                nResult = System.BitConverter.GetBytes(0);
            }

            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_PlayerNotReady,
                                     nResult);
        }
        //改变玩家房间内状态（并广播）
        private void ChangeRoomState(PlayerRoomState nState)
        {
            NPlayerRoomState = nState;

            PlayerRoomStateChange sReady;
            sReady.nPlayerID = m_nPlayerID;
            sReady.nState = (int)NPlayerRoomState;

            byte[] btResult = Common.Method.StructToBytes(sReady);

            Team.TeamMAD(CurTeam,
                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_OtherPlayRoomStateChange,
                            btResult,this);
        }
        //队伍准备（排队）
        public int OnTeamReady(byte[] datagramBytes)
        {
            lock (Team.pReadyTeamlock)
            {
                TeamReady stReady = (TeamReady)Common.Method.BytesToStruct(datagramBytes, typeof(TeamReady));

                int nResult = 0;

                //检查队伍是否存在
                if (!Team.cdicAllTeams.ContainsKey(stReady.nTeamIndex))
                {
                    goto Exit0;
                }
                //检查队伍是否匹配
                if (CurTeam != Team.cdicAllTeams[stReady.nTeamIndex])
                {
                    goto Exit0;
                }
                //检查队员是否是本队队长
                if (CurTeam.Leader != this)
                {
                    goto Exit0;
                }

                //检查队员人数是否足够
                if (CurTeam.nMaxMemberCount != CurTeam.cdicMembers.Count)
                {
                    goto Exit0;
                }

                //  [11/18/2011 test 检查队员状态]
                foreach (Player p in CurTeam.cdicMembers.Values)
                {
                    if (p.NPlayerRoomState != PlayerRoomState.Ready)
                    {
                        goto Exit0;
                    }
                }

                //NPlayerRoomState = PlayerRoomState.Ready;
                CurTeam.NTeamState = Team.TeamState.Ready;
                nResult = 1;

                //发送准备完成消息
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_TeamReady,
                                     System.BitConverter.GetBytes(nResult));

                TeamStateChange stState;
                stState.nState = (int)CurTeam.NTeamState;
                Team.TeamMAD(CurTeam,
                               (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamStateChange,
                                     Common.Method.StructToBytes(stState));

                //寻找匹配队伍
                return Team.FindCouple(CurTeam);
                //Team.cdicReadyTeams.TryAdd(CurTeam.nTeamIndex, CurTeam);

            Exit0:
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_TeamReady,
                                     System.BitConverter.GetBytes(nResult));

            return 0;
            }
        }
        
        //队伍准备（排队）
        public void OnTeamNotReady(byte[] datagramBytes)
        {
            lock (Team.pReadyTeamlock)
            {
                TeamNotReady stReady = (TeamNotReady)Common.Method.BytesToStruct(datagramBytes, typeof(TeamNotReady));

                int nResult = (int)ProtorlEnum.OPResult.OP_FAILURE;
                //检查队伍是否存在
                if (!Team.cdicAllTeams.ContainsKey(stReady.nTeamIndex))
                {
                    goto Exit0;
                }
                //检查队伍是否存在
                if (!Team.cdicReadyTeams.ContainsKey(stReady.nTeamIndex))
                {
                    goto Exit0;
                }
                //检查队伍是否匹配
                if (CurTeam != Team.cdicAllTeams[stReady.nTeamIndex])
                {
                    goto Exit0;
                }
                //检查队员是否是本队队长
                if (CurTeam.Leader != this)
                {
                    goto Exit0;
                }
                if (CurTeam.NTeamState != Team.TeamState.Ready)
                {
                    goto Exit0;
                }

                //  [11/18/2011 test 检查队员状态]
                foreach (Player p in CurTeam.cdicMembers.Values)
                {
                    if (p.NPlayerRoomState != PlayerRoomState.Ready)
                    {
                        goto Exit0;
                    }
                }

                CurTeam.NTeamState = Team.TeamState.Normal;

                //从匹配列表移除
                Team pOutTeam;
                Team.cdicReadyTeams.TryRemove(CurTeam.nTeamIndex, out pOutTeam);
                //重新进入等待队伍
                Team.cdicWaitTeams.TryAdd(CurTeam.nTeamIndex, CurTeam);

                nResult = 1;

                TeamStateChange stState;
                stState.nState = (int)CurTeam.NTeamState;
                Team.TeamMAD(CurTeam,
                               (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamStateChange,
                                     Common.Method.StructToBytes(stState));

            Exit0:
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_TeamNotReady,
                                     System.BitConverter.GetBytes(nResult));
            }
        }

        //打开座位
        public void OnOpenSeat(byte[] datagramBytes)
        {
            OpenSeat stOpen = (OpenSeat)Common.Method.BytesToStruct(datagramBytes, typeof(OpenSeat));

            OpenSeatResult stResult;
            stResult.nResult = (int)ProtorlEnum.OPResult.OP_FAILURE; ;
            //stResult.nSeat = 0;

            //检查队伍是否存在
            if (!Team.cdicAllTeams.ContainsKey(stOpen.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
                goto Exit0;
            }
            //检查队伍是否准备
            if (Team.cdicReadyTeams.ContainsKey(stOpen.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }
            //检查队伍是否匹配
            if (CurTeam != Team.cdicAllTeams[stOpen.nTeamIndex])
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMNOMATCH;
                goto Exit0;
            }
            //检查队员是否是本队队长
            if (CurTeam.Leader != this)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_ISNOTLEADER;
                goto Exit0;
            }
            if (CurTeam.NTeamState != Team.TeamState.Normal)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }

            //CurTeam.SeatOperator(stOpen.nSeat,true);

            if (CurTeam.nMaxMemberCount < Team.m_nMaxPlayer)
            {
                CurTeam.nMaxMemberCount = CurTeam.nMaxMemberCount + 1;
            }
            else
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMMEMBERCOUNTERROE;
                goto Exit0;
            }

            stResult.nResult = (int)ProtorlEnum.OPResult.OP_OK;
            //stResult.nSeat = stOpen.nSeat;

            Team.TeamMAD(CurTeam,
                        (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                        (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamSeatCountChange,
                                System.BitConverter.GetBytes(CurTeam.nMaxMemberCount),this);

        Exit0:
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_OpenSeat,
                                Common.Method.StructToBytes(stResult));
        }
          

        //关闭座位
        public void OnCloseSeat(byte[] datagramBytes)
        {
            CloseSeat stClose = (CloseSeat)Common.Method.BytesToStruct(datagramBytes, typeof(CloseSeat));

            CloseSeatResult stResult;
            stResult.nResult = (int)ProtorlEnum.OPResult.OP_FAILURE; ;
            //stResult.nSeat = 0;

            //检查队伍是否存在
            if (!Team.cdicAllTeams.ContainsKey(stClose.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
                goto Exit0;
            }
            //检查队伍是否准备
            if (Team.cdicReadyTeams.ContainsKey(stClose.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }
            //检查队伍是否匹配
            if (CurTeam != Team.cdicAllTeams[stClose.nTeamIndex])
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMNOMATCH;
                goto Exit0;
            }
            //检查队员是否是本队队长
            if (CurTeam.Leader != this)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_ISNOTLEADER;
                goto Exit0;
            }
            if (CurTeam.NTeamState != Team.TeamState.Normal)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }
            ////该座位上有人
            //if (CurTeam.cdicMembers.ContainsKey(stClose.nSeat))
            //{
            //    goto Exit0;
            //}

            //CurTeam.SeatOperator(stClose.nSeat,false);

            if (CurTeam.nMaxMemberCount > 1)
            {
                CurTeam.nMaxMemberCount = CurTeam.nMaxMemberCount - 1;
            }
            else
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMMEMBERCOUNTERROE;
                goto Exit0;
            }


            stResult.nResult = (int)ProtorlEnum.OPResult.OP_OK; ;
            //stResult.nSeat = stClose.nSeat;

            Team.TeamMAD(CurTeam,
                        (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                        (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamSeatCountChange,
                                System.BitConverter.GetBytes(CurTeam.nMaxMemberCount),this);

        Exit0:
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_CloseSeat,
                                    Common.Method.StructToBytes(stResult));
        }

        //踢人
        public void OnKickPlayer(byte[] datagramBytes)
        {
            KickPlayer stKick = (KickPlayer)Common.Method.BytesToStruct(datagramBytes, typeof(KickPlayer));

            KickPlayerResult stResult;
            stResult.nResult = 0;
            //stResult.nSeat = 0;

            if (null == CurTeam)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
                goto Exit0;
            }
            ////检查队伍是否存在
            //if (!Team.cdicAllTeams.ContainsKey(stKick.nTeamIndex))
            //{
            //    stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
            //    goto Exit0;
            //}
            //检查队伍是否准备
            if (Team.cdicReadyTeams.ContainsKey(CurTeam.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }
            ////检查队伍是否匹配
            //if (CurTeam != Team.cdicAllTeams[stKick.nTeamIndex])
            //{
            //    stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMNOMATCH;
            //    goto Exit0;
            //}
            //检查队员是否是本队队长
            if (CurTeam.Leader != this)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_ISNOTLEADER;
                goto Exit0;
            }
            if (CurTeam.NTeamState != Team.TeamState.Normal)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }

            //stResult.nResult = CurTeam.KickPlayer(stKick.nSeat);
            stResult.nResult = CurTeam.KickPlayer(stKick.nPlayerID);
            //stResult.nSeat = stKick.nSeat;

        Exit0:
            //MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
            //                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_KickPlayer,
            //                        Common.Method.StructToBytes(stResult));
            return;
        }

        //踢人
        public void OnKickAndClose(byte[] datagramBytes)
        {
            KickAndClose stKick = (KickAndClose)Common.Method.BytesToStruct(datagramBytes, typeof(KickAndClose));

            KickAndCloseResult stResult;
            stResult.nResult = 0;
            //stResult.nSeat = 0;

            if (null == CurTeam)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
                goto Exit0;
            }
            ////检查队伍是否存在
            //if (!Team.cdicAllTeams.ContainsKey(stKick.nTeamIndex))
            //{
            //    stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMISNULL;
            //    goto Exit0;
            //}
            //检查队伍是否准备
            if (Team.cdicReadyTeams.ContainsKey(CurTeam.nTeamIndex))
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }
            ////检查队伍是否匹配
            //if (CurTeam != Team.cdicAllTeams[stKick.nTeamIndex])
            //{
            //    stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMNOMATCH;
            //    goto Exit0;
            //}
            //检查队员是否是本队队长
            if (CurTeam.Leader != this)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_ISNOTLEADER;
                goto Exit0;
            }
            if (CurTeam.NTeamState != Team.TeamState.Normal)
            {
                stResult.nResult = (int)ProtorlEnum.OPResult.OP_TEAMSTATEERROE;
                goto Exit0;
            }

            //stResult.nResult = CurTeam.KickPlayer(stKick.nSeat);
            stResult.nResult = CurTeam.KickAndClose(stKick.nPlayerID);
            //stResult.nSeat = stKick.nSeat;

        Exit0:
            //MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
            //                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_KickPlayer,
            //                        Common.Method.StructToBytes(stResult));
            return;
        }
        public void OnKnock(byte[] datagramBytes)
        {
            PlayerKnock sKnock = (PlayerKnock)Common.Method.BytesToStruct(datagramBytes, typeof(PlayerKnock));

            if (null == m_pCurSubStep)
            {
                return;
            }
            
            byte[] nResult;
            int nBeKnockID = CheckKnock(sKnock.nKnockX, sKnock.nKnockY);

            if (nBeKnockID > 0)
            {
                m_pCurSubStep.DiffList[nBeKnockID].bKnocked = true;
            }
            
            if (null != m_pCurStep)
            {
                if (m_pCurStep.NStepType == Step.StepType.Battle)
                {
                    PlayerKnockInfo stInfo;
                    stInfo.nPlayerID = m_nPlayerID;
                    stInfo.nResult = nBeKnockID;

                    if (nBeKnockID > 0)
                    {
                        CurTeam.NBattlePoint += 1;
                    }

                    byte[] btInfo = Common.Method.StructToBytes(stInfo);

                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamA,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerKnock,
                                 btInfo);
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamB,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerKnock,
                                 btInfo);
                }
                else
                {
                    nResult = System.BitConverter.GetBytes(nBeKnockID);
                    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_Knock,
                                 nResult);
                }
            }

            NPlayerScenceState = PlayerScenceState.Stop;

            CheckScenceEnd();
        }

        /// <summary>
        /// 请求下关
        /// </summary>
        public void OnQueryNextSubStep(byte[] datagramBytes)
        {
            QueryNextSubStep sNext = (QueryNextSubStep)Common.Method.BytesToStruct(datagramBytes, typeof(QueryNextSubStep));

            if (null == m_pCurSubStep)
            {
                return;
            }

            if (m_pCurStep.cdSubList.ContainsKey(m_pCurSubStep.ID + 1))
            {
                OnEnterSubStep(m_pCurStep, m_pCurSubStep.ID + 1);
            }
            //else
            //{
            //    //  [11/18/2011 test]关卡模式时，做记录
            //    if (m_pCurStep.NStepType == Step.StepType.Stage)
            //    {
            //        //记录最大开通关卡
            //        bool bUpate = DAL.User.UpdateMaxLogStep(m_nPlayerID.ToString(),
            //                                    m_pCurStep.nStageID.ToString(),
            //                                    m_pCurStep.nID.ToString());
            //        //记录通关时间
            //        System.TimeSpan nFinishTimeSpan = DateTime.Now.Subtract(m_pCurStep.nStartTime);
            //        DAL.User.UpdateStepFinishTimeAndScore(m_nPlayerID.ToString(),
            //                                        m_pCurStep.nStageID.ToString(),
            //                                        m_pCurStep.nID.ToString(),
            //                                        nFinishTimeSpan.Seconds.ToString());

            //        if (bUpate)
            //        {
            //            PlayerStageStepUpdate stUpdate;
            //            stUpdate.nStageID = m_pCurStep.nStageID;
            //            stUpdate.nStepID = m_pCurStep.nID;
            //            stUpdate.nTime = nFinishTimeSpan.Seconds;

            //            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
            //                        (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_StageStepUpdate,
            //                         Common.Method.StructToBytes(stUpdate));
            //        }
            //    }

            //    PlayStepEnd stEndStep;

            //    stEndStep.nResult = (int)ProtorlEnum.ResultEnum.End;
            //    stEndStep.nStageID = m_pCurStep.nStageID;
            //    stEndStep.nStepID = m_pCurStep.nID;

            //    byte[] nEndResult = Common.Method.StructToBytes(stEndStep);
            //    //本关卡结束
            //    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
            //                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayStepEnd,
            //                     nEndResult);



            //    if (m_pCurStep.NStepType == Step.StepType.Battle)
            //    {
            //        BattleEndInfo stInfoA;
            //        BattleEndInfo stInfoB;

            //        Team tA = ((BattleStep)m_pCurStep).PTeamA;
            //        Team tB = ((BattleStep)m_pCurStep).PTeamB;

            //        //结束对战，包括刷新机器人状态，判断队伍是否解散
            //        stInfoA.nPointSelf = tA.NBattlePoint;
            //        stInfoA.nPointEnemy = tB.NBattlePoint;


            //        stInfoB.nPointSelf = tB.NBattlePoint;
            //        stInfoB.nPointEnemy = tA.NBattlePoint;

            //        tA.BattleFinish(stInfoA);
            //        tB.BattleFinish(stInfoB);

            //        if (tA.IsRobortTeam())
            //        {
            //            tA.RobortReady();
            //        }
            //        if (tB.IsRobortTeam())
            //        {
            //            tB.RobortReady();
            //        }
            //    }
            //    else
            //    {
            //        PlayerQuitSence();
            //    }
            //}
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        public void OnUserItem(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));

            switch ((Item.ItemUseType)sFind.nUseType)
            {
                case Item.ItemUseType.Find:
                    {
                        byte[] nResult;
                        int nBeKnockID = FindDiff_NoKnocked();

                        //nResult = System.BitConverter.GetBytes(nBeKnockID);

                        UseItemResultCommon stResult;
                        stResult.nResult = 1;
                        stResult.nUseType = sFind.nUseType;
                        stResult.nParam = nBeKnockID;

                        nResult = Common.Method.StructToBytes(stResult);

                        MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                    (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_UseItem,
                                     nResult);
                    }
                    break;
                case Item.ItemUseType.AOE:
                case Item.ItemUseType.AT:
                case Item.ItemUseType.Hammer:
                case Item.ItemUseType.Invincible:
                case Item.ItemUseType.Speed:
                case Item.ItemUseType.Stun:
                case Item.ItemUseType.TimeSlow:
                case Item.ItemUseType.Trans:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 使用道具 放大镜
        /// </summary>
        public void OnUserItem_Find(byte[] datagramBytes)
        {
            UseItem_Find sFind = (UseItem_Find)Common.Method.BytesToStruct(datagramBytes, typeof(UseItem_Find));

            byte[] nResult;
            int nBeKnockID = FindDiff_NoKnocked();

            if (nBeKnockID > 0)
            {
                m_pCurSubStep.DiffList[nBeKnockID].bKnocked = true;
            }
            
            if (null != m_pCurStep)
            {
                if (m_pCurStep.NStepType == Step.StepType.Battle)
                {
                    PlayerKnockInfo stInfo;
                    stInfo.nPlayerID = m_nPlayerID;
                    stInfo.nResult = nBeKnockID;

                    if (nBeKnockID > 0)
                    {
                        CurTeam.NBattlePoint += 1;
                    }

                    byte[] btInfo = Common.Method.StructToBytes(stInfo);

                    //返回暂时走敲击返回的流程
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamA,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                //(byte)ProtorlEnum.FrameType_ServerToUser.FrameType_OtherUseItem_Find,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerKnock,
                                 btInfo);
                    Team.TeamMAD(((BattleStep)m_pCurStep).PTeamB,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                //(byte)ProtorlEnum.FrameType_ServerToUser.FrameType_OtherUseItem_Find,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerKnock,
                                 btInfo);
                }
                else
                {
                    nResult = System.BitConverter.GetBytes(nBeKnockID);
                    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                //(byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_UseItem_Find,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_Knock,
                                 nResult);
                }
            }

            NPlayerScenceState = PlayerScenceState.Stop;

            CheckScenceEnd();
        }

        /// <summary>
        /// 使用道具 加速
        /// </summary>
        public void OnUserItem_Speed(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 无敌
        /// </summary>
        public void OnUserItem_Invincible(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 导弹
        /// </summary>
        public void OnUserItem_AOE(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 飞弹
        /// </summary>
        public void OnUserItem_Stun(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 减速
        /// </summary>
        public void OnUserItem_TimeSlow(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 保护罩
        /// </summary>
        public void OnUserItem_AT(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 巨锤
        /// </summary>
        public void OnUserItem_Hammer(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        /// <summary>
        /// 使用道具 瞬移
        /// </summary>
        public void OnUserItem_Trans(byte[] datagramBytes)
        {
            UseItemCommon sFind = (UseItemCommon)Common.Method.BytesToStruct(datagramBytes, typeof(UseItemCommon));
        }
        

        private void CheckScenceEnd()
        {
            if (m_pCurSubStep == null)
            {
                return;
            }
            foreach (Diff pDiff in m_pCurSubStep.DiffList.Values)
            {
                if (pDiff.bKnocked != true)
                {
                    return;
                }
            }

            //// 本图清理完成，暂时不用 [12/16/2011 test]
            //SubStepClear stClear;
            //stClear.nResult = 1;

            //MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
            //                        (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_SubStepClear,
            //                         Common.Method.StructToBytes(stClear));


            if (m_pCurStep.cdSubList.ContainsKey(m_pCurSubStep.ID + 1))
            {
                // 目前流程，给客户端一定相应时间，等客户端主动请求后，才进入下一张图 [12/16/2011 test]
                //OnEnterSubStep(m_pCurStep, m_pCurSubStep.ID + 1);
            }
            else
            {
                //if (0 != m_pCurSubStep.m_nBossID)
                //{
                //    m_pCurSubStep.BossTimer.Stop();
                //    m_pCurSubStep.BossTimer.Close();
                //    m_pCurSubStep.BossTimer.Dispose();
                //}

                System.TimeSpan nFinishTimeSpan = DateTime.Now.Subtract(m_pCurStep.nStartTime);

                //  [11/18/2011 test]关卡模式时，做记录
                if (m_pCurStep.NStepType == Step.StepType.Stage)
                {
                    //星级评分
                    PlayerStepGrade stGrade;
                    stGrade.nStageID = m_pCurStep.nStageID;
                    stGrade.nStepID = m_pCurStep.nID;

                    if ((nFinishTimeSpan.Minutes*60 + nFinishTimeSpan.Seconds) <= 60)
                    {
                        stGrade.nGrade = 3;
                    }
                    else if ((nFinishTimeSpan.Minutes * 60 + nFinishTimeSpan.Seconds) <= 90)
                    {
                        stGrade.nGrade = 2;
                    }
                    else if ((nFinishTimeSpan.Minutes * 60 + nFinishTimeSpan.Seconds) <= 120)
                    {
                        stGrade.nGrade = 1;
                    }
                    else
                    {
                        stGrade.nGrade = 0;
                    }

                    MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_StepGrade,
                                     Common.Method.StructToBytes(stGrade));

                    ////记录最大开通关卡
                    //bool bUpate = DAL.User.UpdateMaxLogStep(m_nPlayerID.ToString(),
                    //                            m_pCurStep.nStageID.ToString(),
                    //                            m_pCurStep.nID.ToString());

                    ////记录通关时间及评分（有则更新，无则不变）
                    //DAL.User.UpdateStepFinishTimeAndScore(m_nPlayerID.ToString(),
                    //                                m_pCurStep.nStageID.ToString(),
                    //                                m_pCurStep.nID.ToString(),
                    //                                (nFinishTimeSpan.Minutes * 60 + nFinishTimeSpan.Seconds).ToString(),
                    //                                stGrade.nGrade.ToString());
                    bool bUpate = false;
                    if (bUpate)
                    {
                        m_nLogStageID = m_pCurStep.nStageID;
                        m_nLogStepID = m_pCurStep.nID;

                        PlayerStageStepUpdate stUpdate;
                        stUpdate.nStageID = m_pCurStep.nStageID;
                        stUpdate.nStepID = m_pCurStep.nID;
                        stUpdate.nTime = (nFinishTimeSpan.Minutes * 60 + nFinishTimeSpan.Seconds);

                        MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_StageStepUpdate,
                                     Common.Method.StructToBytes(stUpdate));
                    }
                }

                PlayStepEnd stEndStep;

                stEndStep.nResult = (int)ProtorlEnum.ResultEnum.End;
                stEndStep.nStageID = m_pCurStep.nStageID;
                stEndStep.nStepID = m_pCurStep.nID;

                byte[] nEndResult = Common.Method.StructToBytes(stEndStep);
                //本关卡结束
                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayStepEnd,
                                 nEndResult);



                if (m_pCurStep.NStepType == Step.StepType.Battle)
                {
                    BattleEndInfo stInfoA;
                    BattleEndInfo stInfoB;

                    Team tA = ((BattleStep)m_pCurStep).PTeamA;
                    Team tB = ((BattleStep)m_pCurStep).PTeamB;

                    //结束对战，包括刷新机器人状态，判断队伍是否解散
                    stInfoA.nPointSelf = tA.NBattlePoint;
                    stInfoA.nPointEnemy = tB.NBattlePoint;


                    stInfoB.nPointSelf = tB.NBattlePoint;
                    stInfoB.nPointEnemy = tA.NBattlePoint;

                    tA.BattleFinish(stInfoA);
                    tB.BattleFinish(stInfoB);

                    if (tA.IsRobortTeam())
                    {
                        tA.RobortReady();
                    }
                    if (tB.IsRobortTeam())
                    {
                        tB.RobortReady();
                    }
                }
                else
                {
                    PlayerQuitSence();
                }
            }
        }
        /// <summary>
        /// 玩家单人场景中超时
        /// </summary>
        public void OnSingleTimeOut(byte[] datagramBytes)
        {
            SinglePlayerTimeOut sTimeOut = (SinglePlayerTimeOut)Common.Method.BytesToStruct(datagramBytes, typeof(SinglePlayerTimeOut));

            if (null == m_pCurSubStep)
            {
                return;
            }

            if (sTimeOut.nPlayerID != m_nPlayerID)
            {
                return;
            }

            PlayStepEnd stEndStep;
            stEndStep.nResult = (int)ProtorlEnum.ResultEnum.TimeOut;
            stEndStep.nStageID = m_pCurStep.nStageID;
            stEndStep.nStepID = m_pCurStep.nID;

            byte[] nEndResult = Common.Method.StructToBytes(stEndStep);
            //本关卡结束
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayStepEnd,
                                nEndResult);

            PlayerQuitSence();
            
        }

        /// <summary>
        /// 玩家退出场景（清理状态）
        /// </summary>
        public void PlayerQuitSence()
        {
            NPlayerState = PlayerState.Normal;
            NPlayerScenceState = PlayerScenceState.no;

            //消除当前关卡及子关卡
            m_pCurStep = null;
            m_pCurSubStep = null;
        }
        public void OnCrash(byte[] datagramBytes)
        {
            //Crash sCrash;
            //sCrash.nMonsterIndex = 0;
            //sCrash.nPlayerID = 0;
            //sCrash.nPlayerX = 0;
            //sCrash.nPlayerY = 0;

            Crash sCrash = (Crash)Common.Method.BytesToStruct(datagramBytes, typeof(Crash));
        }

        /// <summary>
        /// 玩家进入场景
        /// </summary>
        public void OnEnterSubStep(Step pStep,int nSubID)
        {
            if (m_pCurStep != pStep && pStep != null)
            {
                m_pCurStep = pStep;
            }

            if (!pStep.cdSubList.ContainsKey(nSubID))
            {
                return;
            }

            byte[] DataByte;
            Step.MakeByte_EnterSubStep(m_pCurStep.nStageID,m_pCurStep.nID, nSubID, pStep, out DataByte);
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser, 
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayEnterSubStep, 
                                DataByte);

            m_nPlayerState = PlayerState.InScence;
            NPlayerScenceState = PlayerScenceState.Stop;

            m_pCurSubStep = pStep.cdSubList[nSubID];

            //子场景启动
            m_pCurSubStep.Start();
        }

        /// <summary>
        /// 玩家进入场景
        /// </summary>
        public void OnEnterScence(Step pStep, int nSubID)
        {
            //if (m_pCurStep != pStep && pStep != null)
            //{
            //    m_pCurStep = pStep;
            //}
            m_pCurStep = pStep;
            m_pCurStep.AddPlayer(this);

            if (!pStep.cdSubList.ContainsKey(nSubID))
            {
                return;
            }

            //byte[] DataByte;
            //Step.MakeByte_EnterSubStep(m_pCurStep.nStageID, m_pCurStep.nID, nSubID, pStep, out DataByte);
            //MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
            //                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayEnterSubStep,
            //                    DataByte);

            m_nPlayerState = PlayerState.InScence;
            NPlayerScenceState = PlayerScenceState.Stop;

            m_pCurSubStep = pStep.cdSubList[nSubID];

            //子场景启动
            m_pCurSubStep.Start();
        }

        ////生成关卡信息
        //private void MakeByte_EnterSubStep(int nSubID, Step pStep, out byte[] tempByte)
        //{
        //    //配置进入场景结构
        //    PlayEnterSubStep stEnterStep;
        //    stEnterStep.nStageID = m_pCurStep.nStageID;
        //    stEnterStep.nStepID = m_pCurStep.nID;
        //    stEnterStep.nSubStepID = nSubID;

        //    //初始化怪物和不同点结构信息
        //    DiffMake sDiffmake;
        //    sDiffmake.nDiffID = 0;
        //    sDiffmake.nRightOrLeft = 0;

        //    MonsterMake sMonstermake;
        //    sMonstermake.nIndex = 0;
        //    sMonstermake.nID = 0;
        //    sMonstermake.nBody = 0;
        //    sMonstermake.nSpeed = 0;
        //    sMonstermake.nMoveWay = 0;
        //    sMonstermake.nHitEffect = 0;
        //    sMonstermake.nHitTime = 0;
        //    sMonstermake.nX = 0;
        //    sMonstermake.nY = 0;
        //    sMonstermake.nDirection = 0;

        //    //取得场景下怪物和不同点的数量
        //    int nDCount = pStep.cdSubList[nSubID].DiffList.Count;
        //    int nMCount = pStep.cdSubList[nSubID].MonsterList.Count;

        //    tempByte = new byte[Marshal.SizeOf(stEnterStep) 
        //        + sizeof(int) + nDCount * Marshal.SizeOf(sDiffmake) 
        //        + sizeof(int) + nMCount * Marshal.SizeOf(sMonstermake)];

        //    byte[] btDCount = System.BitConverter.GetBytes(pStep.cdSubList[nSubID].DiffList.Count);
        //    byte[] btMCount = System.BitConverter.GetBytes(pStep.cdSubList[nSubID].MonsterList.Count);
        //    byte[] byteEnterStep = Common.Method.StructToBytes(stEnterStep);

        //    int offset = 0;
        //    byteEnterStep.CopyTo(tempByte, offset);

        //    //拼接不同点及怪物信息
        //    offset += byteEnterStep.Length;
        //    //拼接不同点信息
        //    btDCount.CopyTo(tempByte, offset);
        //    offset += 4;
        //    foreach (Diff diff in pStep.cdSubList[nSubID].DiffList.Values)
        //    {
        //        sDiffmake.Make(diff);
        //        byte[] byteDiffmake = Common.Method.StructToBytes(sDiffmake);
        //        Array.Copy(byteDiffmake, 0, tempByte, offset, byteDiffmake.Length);
        //        offset += byteDiffmake.Length;
        //    }
        //    //拼接怪物信息
        //    btMCount.CopyTo(tempByte, offset);
        //    offset += 4;
        //    foreach (Monster pMonster in pStep.cdSubList[nSubID].MonsterList.Values)
        //    {
        //        sMonstermake.Make(pMonster);
        //        byte[] bytemonstermake = Common.Method.StructToBytes(sMonstermake);
        //        Array.Copy(bytemonstermake, 0, tempByte, offset, bytemonstermake.Length);
        //        offset += bytemonstermake.Length;
        //    }
        //}

        /// <summary>
        /// 响应请求关卡分数
        /// </summary>
        public void OnQueryStageScore(byte[] datagramBytes)
        {
            QueryStageScore sStage = (QueryStageScore)Common.Method.BytesToStruct(datagramBytes, typeof(QueryStageScore));

            if (StageData.StageList.ContainsKey(sStage.nStageID))
            {
                //DataTable dt = DAL.Player.GetPlayerStageScore(m_nPlayerID, sStage.nStageID);
                DataTable dt = null;
                byte[] allInfo = new byte[4 + 8 * dt.Rows.Count];
                byte[] count = System.BitConverter.GetBytes(dt.Rows.Count);
                int offset = 0;
                count.CopyTo(allInfo, offset);
                offset += count.Length;

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count;++i )
                    {
                        int nQStepID = Convert.ToInt32(dt.Rows[i]["StepID"]);
                        int nQScore = Convert.ToInt32(dt.Rows[i]["Score"]);

                        System.BitConverter.GetBytes(nQStepID).CopyTo(allInfo, offset);
                        offset += 4;
                        System.BitConverter.GetBytes(nQScore).CopyTo(allInfo, offset);
                        offset += 4;
                    }
                }

                MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_StageScore,
                                allInfo);
            }
        }

        /// <summary>
        /// 单人制作并发送数据包
        /// </summary>
        public void MakeAndSendDatagram(byte nGroup,byte nType,byte[] DataByte)
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

            ////发送信息
            //pSession.SendDatagram(tempByte);

            //尝试使用消息队列
            pSession.DatagramQueue_send.Enqueue(tempByte);
        }

        /// <summary>
        /// 单人制作并发送数据包
        /// </summary>
        public void MakeAndSendDatagram_AES(byte nGroup, byte nType, byte[] DataByte)
        {
            byte[] Aes_Byte = new byte[1 + 1 + DataByte.Length];

            byte[] btGroup = { nGroup };
            byte[] btType = { nType };

            btGroup.CopyTo(Aes_Byte, 0);
            btType.CopyTo(Aes_Byte, 1);
            DataByte.CopyTo(Aes_Byte, 2);

            byte[] EncrypByte = LogicMoudle.AESEncryption.AESEncrypt(Aes_Byte, "m7game");
            byte[] tempByte = new byte[1 + 1 + 4 + EncrypByte.Length];
            byte[] btLenth = System.BitConverter.GetBytes(EncrypByte.Length+4);

            Encoding.ASCII.GetBytes("<", 0, 1, tempByte, 0);
            Encoding.ASCII.GetBytes(">", 0, 1, tempByte, tempByte.Length - 1);

            btLenth.CopyTo(tempByte, 1);
            EncrypByte.CopyTo(tempByte, 5);

            ////发送信息
            //pSession.SendDatagram(tempByte);

            //尝试使用消息队列
            pSession.DatagramQueue_send.Enqueue(tempByte);
        }

        //public static void MakeAndSendDatagram(byte nGroup, byte nType, byte[] DataByte,TTestSession tSession)
        //{
        //    byte[] tempByte = new byte[1 + 1 + 4 + 1 + 1 + DataByte.Length];

        //    Encoding.ASCII.GetBytes("<", 0, 1, tempByte, 0);
        //    Encoding.ASCII.GetBytes(">", 0, 1, tempByte, tempByte.Length - 1);

        //    byte[] btGroup = { nGroup };
        //    byte[] btType = { nType };
        //    byte[] btLenth = System.BitConverter.GetBytes(tempByte.Length - 2);

        //    btLenth.CopyTo(tempByte, 1);
        //    btGroup.CopyTo(tempByte, 5);
        //    btType.CopyTo(tempByte, 6);

        //    DataByte.CopyTo(tempByte, 7);

        //    //发送信息
        //    tSession.SendDatagram(tempByte);
        //}

        /// <summary>
        /// 敲击检测，返回0或者不同点ID
        /// </summary>
        public int CheckKnock(int x,int y)
        {
            if (null != m_pCurSubStep)
            {
                foreach (Diff pDiff in m_pCurSubStep.DiffList.Values)
                {
                    if (true == pDiff.bKnocked)
                    {
                        continue;
                    }
                    if (((x + m_nR >= pDiff.PosX + pDiff.LeftX&& x - m_nR <= pDiff.PosX + pDiff.RightX)
                        ||(x + m_nR >= pDiff.PosX + pDiff.LeftX + SubStepData.dis_rtol&& x - m_nR <= pDiff.PosX + pDiff.RightX + SubStepData.dis_rtol))
                        && y + m_nR >= pDiff.PosY + pDiff.LeftY
                        && y - m_nR <= pDiff.PosY + pDiff.RightY)
                    {
                        return pDiff.ID;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 查找未被敲击的点
        /// </summary>
        public int FindDiff_NoKnocked()
        {
            List<int> temp = new List<int>();

            if (null != m_pCurSubStep)
            {
                foreach (Diff pDiff in m_pCurSubStep.DiffList.Values)
                {
                    if (true == pDiff.bKnocked)
                    {
                        continue;
                    }
                    temp.Add(pDiff.ID);
                    //return pDiff.ID;
                }
            }

            int nResult = 0;

            if (temp.Count > 0)
            {
                Random r = new Random();
                int rIndex = r.Next(0, temp.Count);

                nResult = temp[rIndex];

                temp.Clear();
            }

            return nResult;
        }

        /// <summary>
        /// 退队后清理状态
        /// </summary>
        public void ClearTeam()
        {
            CurTeam = null;
            //m_nSeatIndex = 0;
            NPlayerRoomState = Player.PlayerRoomState.no;
            NPlayerState = Player.PlayerState.Normal;
        }

        /// <summary>
        /// 下线
        /// </summary>
        public void Offline()
        {
            if (null != m_pCurStep)
            {
                m_pCurStep.RemovePlayer(this);
            }
            QuitTeam();
        }

        /// <summary>
        /// 退队，队长退队的话，解散队伍
        /// </summary>
        private void QuitTeam()
        {
            lock(Team.plock)
            {
                if (null != CurTeam)
                {
                    if (this != CurTeam.Leader)
                    {
                        Team.TeamMAD(CurTeam,
                                    (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_OtherPlayerQuitTeam,
                                    System.BitConverter.GetBytes(m_nPlayerID), this);
                    }

                    switch (CurTeam.NTeamState)
                    {
                        case Team.TeamState.Normal:
                            {
                                if (this == CurTeam.Leader)
                                {
                                    TeamDisband st;
                                    st.nResult = 1;

                                    Team.TeamMAD(CurTeam,
                                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamDisband,
                                            Common.Method.StructToBytes(st));

                                    int nKey = CurTeam.nTeamIndex;
                                    foreach (Player p in CurTeam.cdicMembers.Values)
                                    {
                                        p.ClearTeam();
                                    }
                                    Team.RemoveTeam(nKey);
                                }
                                else
                                {
                                    Player pOutPlayer;
                                    //CurTeam.cdicMembers.TryRemove(m_nSeatIndex, out pOutPlayer);
                                    CurTeam.cdicMembers.TryRemove(m_nPlayerID, out pOutPlayer);
                                    ClearTeam();
                                }
                            }
                            break;
                        case Team.TeamState.Ready:
                            {
                                if (this == CurTeam.Leader)
                                {
                                    TeamDisband st;
                                    st.nResult = 2;

                                    Team.TeamMAD(CurTeam,
                                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamDisband,
                                            Common.Method.StructToBytes(st));

                                    int nKey = CurTeam.nTeamIndex;
                                    foreach (Player p in CurTeam.cdicMembers.Values)
                                    {
                                        p.ClearTeam();
                                    }
                                    Team.RemoveTeam(nKey);
                                }
                                else
                                {
                                    CurTeam.NTeamState = Team.TeamState.Normal;
                                    //从匹配列表移除
                                    Team pOutTeam;
                                    Team.cdicReadyTeams.TryRemove(CurTeam.nTeamIndex, out pOutTeam);
                                    //重新进入等待队伍
                                    Team.cdicWaitTeams.TryAdd(CurTeam.nTeamIndex, CurTeam);

                                    TeamStateChange stState;
                                    stState.nState = (int)CurTeam.NTeamState;
                                    Team.TeamMAD(CurTeam,
                                                   (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamStateChange,
                                                         Common.Method.StructToBytes(stState));

                                    Player pOutPlayer;
                                    //CurTeam.cdicMembers.TryRemove(m_nSeatIndex, out pOutPlayer);
                                    CurTeam.cdicMembers.TryRemove(m_nPlayerID, out pOutPlayer);
                                    ClearTeam();
                                }
                            }
                            break;
                        case Team.TeamState.Battle:
                            {
                                Player pOutPlayer;
                                //CurTeam.cdicMembers.TryRemove(m_nSeatIndex, out pOutPlayer);
                                CurTeam.cdicMembers.TryRemove(m_nPlayerID, out pOutPlayer);

                                //在战斗场景中退队，人全推光的话

                                if (CurTeam.cdicMembers.Count <= 0)
                                {
                                    int nKey = CurTeam.nTeamIndex;

                                    //对方是机器人对的话，机器人队马上退出，重新排队
                                    if (((BattleStep)m_pCurStep).PTeamA == CurTeam)
                                    {
                                        if (((BattleStep)m_pCurStep).PTeamB.IsRobortTeam())
                                        {
                                            ((BattleStep)m_pCurStep).PTeamB.BattleFinish();
                                            ((BattleStep)m_pCurStep).PTeamB.RobortReady();
                                        }
                                    }
                                    else
                                    {
                                        if (((BattleStep)m_pCurStep).PTeamA.IsRobortTeam())
                                        {
                                            ((BattleStep)m_pCurStep).PTeamA.BattleFinish();
                                            ((BattleStep)m_pCurStep).PTeamA.RobortReady();
                                        }
                                    }
                                    ClearTeam();
                                    Team.RemoveTeam(nKey);
                                }
                                else
                                {
                                    ClearTeam();
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                // 测试版本中，一个人可以创建多个队伍，下线时要全清除，正式版本要去掉 [12/5/2011 test]
                foreach (Team t in new ConcurrentDictionary<int, Team>(Team.cdicAllTeams).Values)
                {
                    if (t.Leader == this)
                        Team.RemoveTeam(t.nTeamIndex);
                }
            }
        }

        /// <summary>
        /// 加入队伍
        /// </summary>
        public void JoinTeam(PlayerJoinTeam stStruct)
        {
            int nResult = 0;

            lock (Team.plock)
            {
                Team pTeam = null;
                bool bAddsuccess = false;
                if (Team.cdicAllTeams.ContainsKey(stStruct.nTeamIndex))
                {
                    pTeam = Team.cdicAllTeams[stStruct.nTeamIndex];
                    //  [11/28/2011 test]房间有密码的话，返回，客户端显示需要密码的界面
                    if (null != pTeam.szPassword)
                    {
                        nResult = -2;
                    }
                    else if (pTeam.cdicMembers.Count >= pTeam.nMaxMemberCount)
                    {
                        nResult = -5;
                    }
                    //else if (pTeam.sdBandPlaylist.ContainsKey(stStruct.nPlayerID))
                    //{
                    //    nResult = -6;
                    //}
                    else if (pTeam.cdicMembers.Count < pTeam.nMaxMemberCount)
                    {
                        bAddsuccess = true;
                        nResult = 1;
                    }
                }
                else
                {
                    nResult = -4;
                }

                if (bAddsuccess)
                {
                    JoinTeamSuccess(pTeam);

                }
                else
                {
                    JoinTeamFailure(nResult);
                }
            }
        }
        public void JoinTeam(PlayerJoinTeamWithPassword stStruct)
        {
            int nResult = 0;

            lock (Team.plock)
            {
                if (!Team.cdicAllTeams.ContainsKey(stStruct.nTeamIndex))
                {
                    nResult = -4;
                    goto Exit0;
                }

                Team pTeam = Team.cdicAllTeams[stStruct.nTeamIndex];
                if (stStruct.szPassword != pTeam.szPassword)
                {
                    nResult = -1;
                    goto Exit0;
                }

                if (Team.cdicAllTeams[stStruct.nTeamIndex].nMaxMemberCount 
                    <= Team.cdicAllTeams[stStruct.nTeamIndex].cdicMembers.Count)
                {
                    nResult = -5;
                    goto Exit0;
                }

                //if (pTeam.sdBandPlaylist.ContainsKey(stStruct.nPlayerID))
                //{
                //    nResult = -6;
                //    goto Exit0;
                //}

                JoinTeamSuccess(Team.cdicAllTeams[stStruct.nTeamIndex]);
                return;
            }
    Exit0:
            JoinTeamFailure(nResult);

        }
        public void JoinTeam(PlayerJoinTeamRandom stStruct)
        {
            bool bSuccess = false;

            lock (Team.plock)
            {
                foreach (Team t in Team.cdicWaitTeams.Values)
                {
                    if (t.nMaxMemberCount > t.cdicMembers.Count)
                    {
                        if (null == t.szPassword)
                        {
                            //if (!t.sdBandPlaylist.ContainsKey(stStruct.nPlayerID))
                            //{
                                JoinTeamSuccess(t);
                                bSuccess = true;
                                break;
                            //}
                        }
                    }
                    continue;
                }
            }

            if (!bSuccess)
            {
                // 找不到合适队伍 [12/2/2011 test]
                JoinTeamFailure(-3);
            }
        }

        private void JoinTeamFailure(int nResult)
        {
            PlayerJoinTeamResult stResult;
            stResult.nResult = nResult;
            stResult.nTeamIndex = 0;
            stResult.nLeaderID = 0;
            stResult.nMemberCount = 0;
            stResult.nCurMaxCount = 0;
            byte[] btResult = Common.Method.StructToBytes(stResult);
            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_JoinTeam,
                                                 btResult);
        }
        private void JoinTeamSuccess(Team t)
        {
            SetJoinTeamState(t);

            //// 测试用，加入房间直接准备,发布版本中要去掉 [11/28/2011 test]
            //NPlayerRoomState = PlayerRoomState.Ready;

            //stResult.nResult = 1;
            PlayerJoinTeamResult stResult;
            stResult.nResult = 1;
            stResult.nTeamIndex = t.nTeamIndex;
            stResult.nLeaderID = t.Leader.m_nPlayerID;
            stResult.nMemberCount = t.cdicMembers.Count;
            stResult.nCurMaxCount = t.nMaxMemberCount;

            int allnamelen = 0;
            byte[] btname;
            byte[] namelen;
            foreach (Player p in t.cdicMembers.Values)
            {
                btname = System.Text.Encoding.UTF8.GetBytes(p.SzName);
                allnamelen += btname.Length;
            }

            byte[] tempByte = new byte[Marshal.SizeOf(stResult)
                                        + (stResult.nMemberCount * 12) + allnamelen];

            byte[] btResult = Common.Method.StructToBytes(stResult);

            int offset = 0;
            btResult.CopyTo(tempByte, offset);
            offset += btResult.Length;

            foreach (Player p in t.cdicMembers.Values)
            {
                byte[] btPlayerID = System.BitConverter.GetBytes(p.m_nPlayerID);
                btPlayerID.CopyTo(tempByte, offset);
                offset += 4;

                byte[] btResourceID = System.BitConverter.GetBytes(p.m_nResourceID);
                btResourceID.CopyTo(tempByte, offset);
                offset += 4;

                btname = System.Text.Encoding.UTF8.GetBytes(p.SzName);
                namelen = System.BitConverter.GetBytes(btname.Length);
                namelen.CopyTo(tempByte, offset);
                offset += 4;

                btname.CopyTo(tempByte, offset);
                offset += btname.Length;
            }

            MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerResult,
                                (byte)ProtorlEnum.FrameType_ServerResult.FrameType_Result_JoinTeam,
                                 tempByte);

            PlayerBaseInfo stJ;
            stJ.nPlayerID = m_nPlayerID;
            stJ.nResourceID = m_nResourceID;
            btResult = Common.Method.StructToBytes(stJ);

            byte[] selfname = System.Text.Encoding.UTF8.GetBytes(SzName);
            byte[] selfnamelen = System.BitConverter.GetBytes(selfname.Length);

            byte[] OtherInfo = new byte[btResult.Length + 4 + selfnamelen.Length];
            btResult.CopyTo(OtherInfo, 0);
            selfnamelen.CopyTo(OtherInfo, btResult.Length);
            selfname.CopyTo(OtherInfo, 4 + btResult.Length);

            Team.TeamMAD(t,(byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_OtherPlayJoinTeam,
                                OtherInfo, this);
        }
    }
}
