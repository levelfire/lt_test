using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Server_Form.Protorol;
using System.Threading;
using Server_Form.GameInse;
using System.Runtime.InteropServices;
using System.Timers;
using Server_Form.GameData;

namespace Server_Form
{
    public class Team
    {
        public enum TeamFlag
        {
            IsRobot = 0x0001,
        }

        public enum TeamState
        {
            Normal  =   0,
            Ready   =   1,
            Battle  =   2,
        }
        public enum SeatState
        {
            Seat_1 = 1,
            Seat_2 = 1 << 1,
            Seat_3 = 1 << 2,
        }
        public enum BattleType
        {
            Opposition  = 1,//对抗
            Cooperation = 2,//合作
            Racing      = 3,//竞速
        }

        public const int m_nMaxPlayer = 3;

        private int nFlag;
        public int NFlag
        {
            get { return nFlag; }
            set { nFlag = value; }
        }

        private int nSeatFlag;
        public int NSeatFlag
        {
            get { return nSeatFlag; }
            set { nSeatFlag = value; }
        }

        private static Int32 numCreateTeam;

        public static object plock = new object();
        public static object pReadyTeamlock = new object();

        public object pSelfTeamlock = new object();

        public Player Leader;

        private TeamState m_nTeamState;

        public TeamState NTeamState
        {
            get { return m_nTeamState; }
            set { m_nTeamState = value; }
        }

        private BattleType nBattleType;
        public BattleType NBattleType
        {
            get { return nBattleType; }
            set { nBattleType = value; }
        }

        private int nBattlePoint = 0;
        public int NBattlePoint
        {
            get { return nBattlePoint; }
            set { nBattlePoint = value; }
        }

        //所有队伍列表
        public static ConcurrentDictionary<int, Team> cdicAllTeams = new ConcurrentDictionary<int, Team>(Define.concurrencyLevel, Define.initialCapacity);
        //准备队伍列表
        public static ConcurrentDictionary<int, Team> cdicReadyTeams = new ConcurrentDictionary<int, Team>(Define.concurrencyLevel, Define.initialCapacity);
        //等待队伍列表
        public static ConcurrentDictionary<int, Team> cdicWaitTeams = new ConcurrentDictionary<int, Team>(Define.concurrencyLevel, Define.initialCapacity);

        public ConcurrentDictionary<int, Player> cdicMembers = new ConcurrentDictionary<int, Player>(Define.concurrencyLevel, Define.initialCapacity);
        public ConcurrentDictionary<int, Player> sdBandPlaylist = new ConcurrentDictionary<int, Player>(Define.concurrencyLevel, Define.initialCapacity);

        public int nTeamIndex
        {
            get;
            set;
        }

        public string szName
        {
            get;
            set;
        }

        public string szPassword
        {
            get;
            set;
        }

        public int nMaxMemberCount
        {
            get;
            set;
        }

        /// <summary>
        /// 通过指定的机器人，创建一个机器人队伍 
        /// </summary>
        public Team(Player p1,Player p2,Player p3)
        {
            lock (plock)
            {
                if (p1.IsRobort()&&p2.IsRobort()&&p3.IsRobort())
                {
                    Interlocked.Increment(ref numCreateTeam);
                    nTeamIndex = numCreateTeam;

                    Leader = p1;

                    szName = "三个火枪手";

                    szPassword = null;

                    nMaxMemberCount = 3;

                    cdicMembers.TryAdd(0, p1);
                    cdicMembers.TryAdd(1, p2);
                    cdicMembers.TryAdd(2, p3);

                    NTeamState = TeamState.Normal;
                    nFlag = (int)TeamFlag.IsRobot;
                    //InitSeat();

                    // 目前比赛只有对抗一种，之后加 [12/2/2011 test]
                    nBattleType = BattleType.Opposition;

                    cdicAllTeams.TryAdd(nTeamIndex, this);
                }

                nBattleType = BattleType.Opposition;
            }
        }

        /// <summary>
        /// 通过指定的机器人列表，创建一个机器人队伍
        /// </summary>
        public Team(List<Player> PlayerList)
        {
            lock (plock)
            {
                if (PlayerList.Count > 0 && PlayerList.Count <= 3)
                {
                    Interlocked.Increment(ref numCreateTeam);
                    nTeamIndex = numCreateTeam;

                    Leader = PlayerList[0];
                    szName = "盖亚";
                    szPassword = null;
                    nMaxMemberCount = PlayerList.Count;

                    int index = 0;
                    foreach (Player p in PlayerList)
                    {
                        if (p.IsRobort())
                        {
                            cdicMembers.TryAdd(index, p);
                            p.NPlayerRoomState = Player.PlayerRoomState.Ready;
                            ++index;
                        }  
                    }

                    NTeamState = TeamState.Normal;
                    nFlag = (int)TeamFlag.IsRobot;
                    //InitSeat();

                    // 目前比赛只有对抗一种，之后加 [12/2/2011 test]
                    nBattleType = BattleType.Opposition;

                    cdicAllTeams.TryAdd(nTeamIndex, this);
                }
            }
        }

        /// <summary>
        /// 通过玩家的创建信息，创建一个队伍
        /// </summary>
        public Team(PlayerCreateTeam stCreateTeam)
        {
            lock(plock)
            {
                Interlocked.Increment(ref numCreateTeam);
                nTeamIndex = numCreateTeam;

                Leader = Player.AllPlayerList[stCreateTeam.nPlayerID];
                szName = stCreateTeam.szName;

                if (stCreateTeam.nPasswordLenth > 0)
                {
                    szPassword = stCreateTeam.szPassword;
                }
                else
                {
                    szPassword = null;
                }

                // 目前比赛只有对抗一种，之后加 [12/2/2011 test]
                nBattleType = (BattleType)stCreateTeam.nBattleType;

                nMaxMemberCount = stCreateTeam.nMemberCount;

                //入队操作由player.JoinTeam完成
                NTeamState = TeamState.Normal;
                nFlag = 0;
                //InitSeat();
                cdicAllTeams.TryAdd(nTeamIndex, this);
            }
        }

        ///// <summary>
        ///// 座位初始化（全打开）
        ///// </summary>
        //private void InitSeat()
        //{
        //    nSeatFlag = 0;
        //    nSeatFlag |= (int)SeatState.Seat_1;
        //    nSeatFlag |= (int)SeatState.Seat_2;
        //    nSeatFlag |= (int)SeatState.Seat_3;
        //}


        /// <summary>
        /// 移除队伍
        /// </summary>
        public static void RemoveTeam(int nIndex)
        {
            if (cdicReadyTeams.ContainsKey(nIndex))
            {
                cdicReadyTeams[nIndex].cdicMembers.Clear();
                Team pOutTeam;
                Team.cdicReadyTeams.TryRemove(nIndex, out pOutTeam);
            }
            if (cdicWaitTeams.ContainsKey(nIndex))
            {
                cdicWaitTeams[nIndex].cdicMembers.Clear();
                Team pOutTeam;
                Team.cdicWaitTeams.TryRemove(nIndex, out pOutTeam);
            } 
            if (cdicAllTeams.ContainsKey(nIndex))
            {
                cdicAllTeams[nIndex].cdicMembers.Clear();
                Team pOutTeam;
                Team.cdicAllTeams.TryRemove(nIndex, out pOutTeam);
                pOutTeam = null;
            }  
        }

        /// <summary>
        /// 操作座位
        /// </summary>
        public void SeatOperator(int nSeat,bool bOpenOrClose)
        {
            lock(pSelfTeamlock)
            {
                if (bOpenOrClose)
                {
                    OpenSeat(nSeat);
                }
                else
                {
                    CloseSeat(nSeat);
                }
            }
        }

        /// <summary>
        /// 开启指定座位
        /// </summary>
        private void OpenSeat(int nSeat)
        {
            nSeatFlag |= (1 << nSeat);
        }
        /// <summary>
        /// 关闭指定座位
        /// </summary>
        private void CloseSeat(int nSeat)
        {
            int nTemp = ~(1 << nSeat);
            nSeatFlag &= nTemp;
        }

        /// <summary>
        /// 检测是否是机器人队伍
        /// </summary>
        public bool IsRobortTeam()
        {
            return (nFlag & (int)TeamFlag.IsRobot) > 0;
        }

        /// <summary>
        /// 匹配队伍
        /// </summary>
        public static int FindCouple(Team pTeam)
        {
            lock (pReadyTeamlock)
            {
                Team pFindTeam;
                int nKey = -1;
                foreach (Team t in cdicReadyTeams.Values)
                {
                    if (pTeam.IsRobortTeam() && t.IsRobortTeam())
                    {
                        continue;
                    }
                    if (t.nMaxMemberCount == pTeam.nMaxMemberCount && t.nTeamIndex != pTeam.nTeamIndex)
                    {
                        pFindTeam = t;
                        nKey = t.nTeamIndex;
                        break;
                    }
                }

                if (nKey >= 0)
                {
                    cdicReadyTeams.TryRemove(nKey, out pFindTeam);
                    //两队开始战斗

                    BattleStep pStep;

                    //根据场景创建是否成功，返回不同结构
                    if (Step.CreateBattleScence(1, 1, 1, pTeam, pFindTeam, out pStep))
                    {
                        byte[] byteEnterStep;
                        Step.MakeByte_EnterSubStep(1, 1, 1, pStep, out byteEnterStep);

                        PlayerMake pM;
                        pM.nID = 0;
                        pM.nResourceID = 0;
                        pM.nTeamIndex = 0;
                        pM.nX = 0;
                        pM.nY = 0;

                        int nPlayerCount = pTeam.nMaxMemberCount * 2;

                        byte[] tempByte = new byte[byteEnterStep.Length
                                        + sizeof(int) + nPlayerCount * Marshal.SizeOf(pM)];

                        int offset = 0;
                        byteEnterStep.CopyTo(tempByte, offset);

                        //拼接玩家信息
                        offset += byteEnterStep.Length;

                        byte[] btCount = System.BitConverter.GetBytes(nPlayerCount);
                        btCount.CopyTo(tempByte, offset);
                        offset += 4;

                        foreach (Player p in pTeam.cdicMembers.Values)
                        {
                            //p.m_pCurStep = pStep;
                            p.OnEnterScence(pStep, 1);

                            pM.Make(p);

                            byte[] btPlayMake = Common.Method.StructToBytes(pM);
                            Array.Copy(btPlayMake, 0, tempByte, offset, btPlayMake.Length);
                            offset += btPlayMake.Length;
                        }
                        foreach (Player p in pFindTeam.cdicMembers.Values)
                        {
                            //p.m_pCurStep = pStep;
                            p.OnEnterScence(pStep, 1);
                            pM.Make(p);

                            byte[] btPlayMake = Common.Method.StructToBytes(pM);
                            Array.Copy(btPlayMake, 0, tempByte, offset, btPlayMake.Length);
                            offset += btPlayMake.Length;
                        }

                        //给两队发进入场景消息
                        if (pTeam.IsRobortTeam())
                        {
                            pTeam.ClearRobortTime();
                            pTeam.RobotMove();
                        }
                        else
                        {
                            TeamMAD(pTeam,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamEnterStep,
                                tempByte);
                        }

                        if (pFindTeam.IsRobortTeam())
                        {
                            pFindTeam.ClearRobortTime();
                            pFindTeam.RobotMove();
                        } 
                        else
                        {
                            TeamMAD(pFindTeam,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamEnterStep,
                                tempByte);
                        }
                        return nKey;
                    }             
                }
                else
                {
                    cdicReadyTeams.TryAdd(pTeam.nTeamIndex, pTeam);
                }
            }
            return 0;
        }

        /// <summary>
        /// 清楚机器人计时器
        /// </summary>
        public void ClearRobortTime()
        {
            if (null != RobortTimer)
            {
                RobortTimer.Close();
            }
        }

        /// <summary>
        /// 战斗结束（清理计时器，清理状态）
        /// </summary>
        public void BattleFinish()
        {
            NTeamState = TeamState.Normal;
            ClearRobortTime();
            foreach (Player p in cdicMembers.Values)
            {
                p.PlayerQuitSence();
            }
            //如果队长掉线不在，解散队伍
            if (null == Leader)
            {
                TeamDisband st;
                st.nResult = 3;

                TeamMAD(this,
                        (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                        (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_TeamDisband,
                        Common.Method.StructToBytes(st));

                int nKey = nTeamIndex;
                foreach (Player p in cdicMembers.Values)
                {
                    p.ClearTeam();
                }
                Team.RemoveTeam(nKey);
            }
        }
        public void BattleFinish(BattleEndInfo stInfo)
        {
            if (!IsRobortTeam())
            {
                byte[] btResult = Common.Method.StructToBytes(stInfo);
                //本关卡结束
                //发送结束信息
                TeamMAD(this,
                    (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                    (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayBattleEnd,
                        btResult);
            }

            BattleFinish();
        }

        /// <summary>
        /// 向队伍成员广播信息
        /// </summary>
        public static void TeamMAD( Team pTeam,byte btGroup,byte btType, byte[] tempByte,Player pWithout = null)
        {
            if (null == pTeam || pTeam.IsRobortTeam())
            {
                return;
            }
	        //向两队发送信息
            foreach (Player p in pTeam.cdicMembers.Values)
            {
                if (!p.IsRobort())
                {
                    if (p != pWithout)
                    {
                        p.MakeAndSendDatagram(btGroup, btType, tempByte);
                    }
                }
            }
        }

        /// <summary>
        /// 加入玩家
        /// </summary>
        public bool AddPlayer(Player pPlayer,bool bIsLeader = false)
        {
            
            lock (pSelfTeamlock)
            {
                bool nResult = false;
                if (null == pPlayer)
                {
                    return nResult;
                }

                cdicMembers.TryAdd(pPlayer.NPlayerID, pPlayer);
                //pPlayer.NSeatIndex = i;
                pPlayer.CurTeam = this;
                pPlayer.NPlayerState = Player.PlayerState.InTeam;

                if (bIsLeader)
                {
                    Leader = pPlayer;
                    pPlayer.NPlayerRoomState = Player.PlayerRoomState.Ready;
                    //创建队伍后，进入等待列表
                    Team.cdicWaitTeams.TryAdd(this.nTeamIndex, this);
                }
                else
                {
                    pPlayer.NPlayerRoomState = Player.PlayerRoomState.Normal;
                }

                nResult = true;

                //// 考虑到踢人功能，Index序号不连续，可能要修改 [12/2/2011 test]
                //for (int i = 0; i < m_nMaxPlayer; ++i)
                //{
                //    if (cdicMembers.ContainsKey(i))
                //    {
                //        continue;
                //    }
                //    else
                //    {
                //        cdicMembers.TryAdd(i, pPlayer);
                //        pPlayer.NSeatIndex = i;
                //        pPlayer.CurTeam = this;
                //        pPlayer.NPlayerState = Player.PlayerState.InTeam;

                //        if (bIsLeader)
                //        {
                //            Leader = pPlayer;
                //            pPlayer.NPlayerRoomState = Player.PlayerRoomState.Ready;
                //            //创建队伍后，进入等待列表
                //            Team.cdicWaitTeams.TryAdd(this.nTeamIndex, this);
                //        }
                //        else
                //        {
                //            pPlayer.NPlayerRoomState = Player.PlayerRoomState.Normal;
                //        }
                        
                //        nResult = true;
                //        break;
                //    }
                //}
                return nResult;
            }
        }

        /// <summary>
        /// 踢出玩家
        /// </summary>
        public int KickPlayer(int nPlayerID)
        {
            lock (pSelfTeamlock)
            {
                int nResult = (int)ProtorlEnum.OPResult.OP_FAILURE;
                if (!cdicMembers.ContainsKey(nPlayerID))
                {
                    return nResult;
                }
                //加入踢人列表
                sdBandPlaylist.TryAdd(nPlayerID, cdicMembers[nPlayerID]);
                //广播
                Team.TeamMAD(cdicMembers[nPlayerID].CurTeam,
                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerBeKicked,
                            System.BitConverter.GetBytes(nPlayerID));

                Player pOutPlayer;
                cdicMembers.TryRemove(nPlayerID, out pOutPlayer);    

                // 设置玩家队伍state [11/21/2011 test]
                pOutPlayer.ClearTeam();  

                nResult = (int)ProtorlEnum.OPResult.OP_OK;
                return nResult;
            }
        }

        /// <summary>
        /// 踢出玩家并关闭座位
        /// </summary>
        public int KickAndClose(int nPlayerID)
        {
            lock (pSelfTeamlock)
            {
                int nResult = (int)ProtorlEnum.OPResult.OP_FAILURE;
                if (!cdicMembers.ContainsKey(nPlayerID))
                {
                    return nResult;
                }

                if (nMaxMemberCount > 1)
                {
                    nMaxMemberCount = nMaxMemberCount - 1;
                }
                else
                {
                    nResult = (int)ProtorlEnum.OPResult.OP_TEAMMEMBERCOUNTERROE;
                    return nResult;
                }

                //加入踢人列表
                sdBandPlaylist.TryAdd(nPlayerID, cdicMembers[nPlayerID]);

                KickAndCloseBroadCast st;
                st.nPlayerID = nPlayerID;
                st.nSeatCount = nMaxMemberCount;

                //广播
                Team.TeamMAD(cdicMembers[nPlayerID].CurTeam,
                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_PlayerBeKickAndClose,
                            Common.Method.StructToBytes(st));

                Player pOutPlayer;
                cdicMembers.TryRemove(nPlayerID, out pOutPlayer);

                // 设置玩家队伍state [11/21/2011 test]
                pOutPlayer.ClearTeam();

                nResult = (int)ProtorlEnum.OPResult.OP_OK;
                return nResult;
            }
        }

        /// <summary>
        /// 队伍准备
        /// </summary>
        public void RobortReady()
        {
            NTeamState = Team.TeamState.Ready;
            Team.FindCouple(this);
        }

        /// <summary>
        /// 生成人数为1的机器人的队伍
        /// </summary>
        public static void CreateSingleRobortTeam(int ID)
        {
            Player r1 = new Player(ID, true);
            List<Player> rl1 = new List<Player>();
            rl1.Add(r1);
            Team t1 = new Team(rl1);
            t1.RobortReady();
        }
        /// <summary>
        /// 生成人数为2的机器人的队伍
        /// </summary>
        public static void CreateSingleRobortTeam(int nID1,int nID2)
        {
            Player r1 = new Player(nID1, true);
            Player r2 = new Player(nID2, true);
            List<Player> rl1 = new List<Player>();
            rl1.Add(r1);
            rl1.Add(r2);
            Team t1 = new Team(rl1);
            t1.RobortReady();
        }
        /// <summary>
        /// 生成人数为3的机器人的队伍
        /// </summary>
        public static void CreateSingleRobortTeam(int nID1, int nID2,int nID3)
        {
            Player r1 = new Player(nID1, true);
            Player r2 = new Player(nID2, true);
            Player r3 = new Player(nID3, true);
            List<Player> rl1 = new List<Player>();
            rl1.Add(r1);
            rl1.Add(r2);
            rl1.Add(r3);
            Team t1 = new Team(rl1);
            t1.RobortReady();
        }
        /// <summary>
        /// 生成多个机器人的队伍
        /// </summary>
        public static void CreateMultiRobortTeam(List<Player> rl1)
        {
            Team t4 = new Team(rl1);
            t4.RobortReady();
        }
        /// <summary>
        /// 初始化机器人队伍
        /// </summary>
        public static void InitRobot()
        {
            CreateSingleRobortTeam(101);
            CreateSingleRobortTeam(102,103);
            CreateSingleRobortTeam(104,105,106);
        }

        /// <summary>
        /// 机器人移动计时器
        /// </summary>
        private System.Timers.Timer RobortTimer;

        /// <summary>
        /// 机器人移动目标改变
        /// </summary>
        public void ChangeRobortMoveState(object source, ElapsedEventArgs e)
        {
            foreach (Player p in cdicMembers.Values)
            {
                RobortMoveInfo stMove;
                stMove.nPlayerID = p.NPlayerID;
                Random r = new Random(Guid.NewGuid().GetHashCode());
                stMove.nX = r.Next(100, SubStepData.width);
                stMove.nY = r.Next(100, SubStepData.height);

                byte[] btMove = Common.Method.StructToBytes(stMove);
                if (null == p.m_pCurStep)
                {
                    if (null != RobortTimer)
                    {
                        RobortTimer.Close();
                    }
                    return;
                }
                Team.TeamMAD(((BattleStep)p.m_pCurStep).PTeamA,
                                (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_RobortMove,
                                 btMove);
                Team.TeamMAD(((BattleStep)p.m_pCurStep).PTeamB,
                            (byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                            (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_RobortMove,
                             btMove);
            }
        }

        /// <summary>
        /// 启动机器人移动
        /// </summary>
        public void RobotMove()
        {
            if (IsRobortTeam())
            {
                    RobortTimer = new System.Timers.Timer(3000);

                    // Hook up the Elapsed event for the timer.
                    RobortTimer.Elapsed += new ElapsedEventHandler(ChangeRobortMoveState);

                    // Set the Interval to Loop ( milliseconds).
                    RobortTimer.Interval = 3000;
                    RobortTimer.Enabled = true;
                    RobortTimer.AutoReset = true;
            }
        }
    }
}
