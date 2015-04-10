using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Server_Form.GameData;
using Server_Form.Protorol;
using System.Runtime.InteropServices;

namespace Server_Form.GameInse
{
    public class Step
    {
        public enum StepType
        {
            Stage   =   0,//关卡模式
            Battle  =   1,//对抗模式
        }
        /// <summary>
        /// 进入关卡的玩家列表
        /// </summary>
        public ConcurrentDictionary<int, Player> cdPlayerList = new ConcurrentDictionary<int, Player>(Define.concurrencyLevel, Define.initialCapacity);

        /// <summary>
        /// 子场景列表
        /// </summary>
        public ConcurrentDictionary<int, SubStep> cdSubList = new ConcurrentDictionary<int, SubStep>(Define.concurrencyLevel, Define.initialCapacity);

        public int nMonsterInseIndex = 0;

        /// <summary>
        /// 图中怪物列表
        /// </summary>
        public ConcurrentDictionary<int, Monster> MonsterList = new ConcurrentDictionary<int, Monster>(Define.concurrencyLevel, Define.initialCapacity);

        protected StepType m_nStepType;

        public StepType NStepType
        {
            get { return m_nStepType; }
            set { m_nStepType = value; }
        }
        /// <summary>
        /// 关卡ID
        /// </summary>
        public int nID
        {
            get;
            set;
        }
        /// <summary>
        /// 所属大关卡ID
        /// </summary>
        public int nStageID
        {
            get;
            set;
        }
        /// <summary>
        /// 当前子场景
        /// </summary>
        public SubStep pCurSubStep
        {
            get;
            set;
        }
        /// <summary>
        /// 关卡唯一Index
        /// </summary>
        public int nIndex
        {
            get;
            set;
        }
        /// <summary>
        /// 关卡开启时间(用于统计通关时间)
        /// </summary>
        public DateTime nStartTime
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public SubStep CreateSubStep(int nSubStepID)
        {
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool AddPlayer(Player pPlayer)
        {
            if (cdPlayerList.TryAdd(pPlayer.NPlayerID,pPlayer))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool RemovePlayer(Player pPlayer)
        {
            if (cdPlayerList.TryRemove(pPlayer.NPlayerID, out pPlayer))
            {
                return true;
            }
            return false;
        }

        public void Open(object state)
        {

        }

        //创建step场景实例，包括其下的所有子场景及不同点
        public static bool CreateScence(int nStageID, int nStepID, out Step poutStep)
        {
            if (StageData.StageList.ContainsKey(nStageID))
            {
                if (StageData.StageList[nStageID].StepList.ContainsKey(nStepID))
                {
                    StepData pDataStep = StageData.StageList[nStageID].StepList[nStepID];
                    Step pStep = new Step();
                    pStep.nStageID = nStageID;
                    pStep.nID = nStepID;
                    pStep.nIndex = nStepID;
                    int nSubIndex = 1;
                    foreach (SubStepData psub in pDataStep.SubStepList.Values)
                    {
                        SubStep pSubStep = new SubStep(pStep);
                        pSubStep.ID = psub.ID;
                        pSubStep.Index = nSubIndex;
                        nSubIndex++;
                        for (int i = 1; i <= psub.MaxDiff; ++i)
                        {
                            Random r = new Random(Guid.NewGuid().GetHashCode());
                            int rID = r.Next(1, psub.DiffList.Count + 1);
                            while (pSubStep.DiffList.ContainsKey(rID))
                            {
                                r = new Random(Guid.NewGuid().GetHashCode());
                                rID = r.Next(1, psub.DiffList.Count + 1);
                            }

                            int rorl = r.Next(0, 2);
                            Diff pDiff = new Diff(i, rorl, psub.DiffList[rID]);
                            pSubStep.DiffList.TryAdd(pDiff.ID, pDiff);
                            //DiffData pDiff = new DiffData();
                        }
                        foreach (int nMKey in psub.MonsterList.Keys)
                        {
                            Monster pMonster = new Monster(pStep.nMonsterInseIndex, psub.MonsterList[nMKey], pSubStep);
                            pStep.nMonsterInseIndex++;
                            pStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                            pSubStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                            pMonster.CurStep = pStep;
                        }
                        pSubStep.m_nBossID = 0;
                        if (0 != psub.m_BossID)
                        {
                            if (BossData.BossDataList.ContainsKey(psub.m_BossID))
                            {
                                pSubStep.m_nBossID = psub.m_BossID;
                            }
                        }
                        pStep.cdSubList.TryAdd(psub.ID, pSubStep);
                    }

                    pStep.NStepType = Step.StepType.Stage;
                    poutStep = pStep;
                    //查看是否是完成一关，包含子场景
                    if (0 < poutStep.cdSubList.Count)
                    {
                        //ThreadPool.QueueUserWorkItem(poutStep.Open);
                        return true;
                    }
                }
            }
            poutStep = null;
            return false;
        }

        //创建step场景实例，包括其下的所有子场景及不同点
        public static bool CreateBattleScence(int nStageID, int nStepID,int nSubID, Team pTA,Team pTB, out BattleStep poutStep)
        {
            if (StageData.StageList.ContainsKey(nStageID))
            {
                if (StageData.StageList[nStageID].StepList.ContainsKey(nStepID))
                {
                    StepData pDataStep = StageData.StageList[nStageID].StepList[nStepID];
                    BattleStep pStep = new BattleStep();
                    pStep.nStageID = nStageID;
                    pStep.nID = nStepID;
                    pStep.nIndex = nStepID;

                    if (pDataStep.SubStepList.ContainsKey(nSubID))
                    {
                        SubStepData psub = pDataStep.SubStepList[nSubID];
                        SubStep pSubStep = new SubStep(pStep);
                        pSubStep.ID = psub.ID;
                        pSubStep.Index = 1;
                        for (int i = 1; i <= psub.MaxDiff; ++i)
                        {
                            Random r = new Random(Guid.NewGuid().GetHashCode());
                            int rID = r.Next(1, psub.DiffList.Count + 1);
                            while (pSubStep.DiffList.ContainsKey(rID))
                            {
                                r = new Random(Guid.NewGuid().GetHashCode());
                                rID = r.Next(1, psub.DiffList.Count + 1);
                            }

                            int rorl = r.Next(0, 2);
                            Diff pDiff = new Diff(i, rorl, psub.DiffList[rID]);
                            pSubStep.DiffList.TryAdd(pDiff.ID, pDiff);
                            //DiffData pDiff = new DiffData();
                        }
                        foreach (int nMKey in psub.MonsterList.Keys)
                        {
                            Monster pMonster = new Monster(pStep.nMonsterInseIndex, psub.MonsterList[nMKey], pSubStep);
                            pStep.nMonsterInseIndex++;
                            pStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                            pSubStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                            pMonster.CurStep = pStep;
                        }
                        pStep.cdSubList.TryAdd(psub.ID, pSubStep);
                        pStep.pCurSubStep = pSubStep;
                    }

                    //目前对战场景只有一关
                    //int nSubIndex = 1;
                    //foreach (SubStepData psub in pDataStep.SubStepList.Values)
                    //{
                    //    SubStep pSubStep = new SubStep(pStep);
                    //    pSubStep.ID = psub.ID;
                    //    pSubStep.Index = nSubIndex;
                    //    nSubIndex++;
                    //    for (int i = 1; i <= psub.MaxDiff; ++i)
                    //    {
                    //        Random r = new Random(Guid.NewGuid().GetHashCode());
                    //        int rID = r.Next(1, psub.DiffList.Count + 1);
                    //        while (pSubStep.DiffList.ContainsKey(rID))
                    //        {
                    //            r = new Random(Guid.NewGuid().GetHashCode());
                    //            rID = r.Next(1, psub.DiffList.Count + 1);
                    //        }

                    //        int rorl = r.Next(0, 2);
                    //        Diff pDiff = new Diff(i, rorl, psub.DiffList[rID]);
                    //        pSubStep.DiffList.TryAdd(pDiff.ID, pDiff);
                    //        //DiffData pDiff = new DiffData();
                    //    }
                    //    foreach (int nMKey in psub.MonsterList.Keys)
                    //    {
                    //        Monster pMonster = new Monster(pStep.nMonsterInseIndex, psub.MonsterList[nMKey]);
                    //        pStep.nMonsterInseIndex++;
                    //        pStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                    //        pSubStep.MonsterList.TryAdd(pMonster.Index, pMonster);
                    //        pMonster.CurStep = pStep;
                    //    }
                    //    pStep.cdSubList.TryAdd(psub.ID, pSubStep);
                    //}

                    pStep.NStepType = Step.StepType.Battle;
                    pStep.PTeamA = pTA;
                    pStep.PTeamB = pTB;
                    pTA.NTeamState = Team.TeamState.Battle;
                    pTB.NTeamState = Team.TeamState.Battle;
                    poutStep = pStep;
                    //查看是否是完成一关，包含子场景
                    if (0 < poutStep.cdSubList.Count)
                    {
                        //ThreadPool.QueueUserWorkItem(poutStep.Open);
                        return true;
                    }
                }
            }
            poutStep = null;
            return false;
        }

        //生成关卡信息
        public static void MakeByte_EnterSubStep(int nStageID,int nStepID, int nSubID, Step pStep, out byte[] tempByte)
        {
            //配置进入场景结构
            PlayEnterSubStep stEnterStep;
            stEnterStep.nStageID = nStageID;
            stEnterStep.nStepID = nStepID;
            stEnterStep.nSubStepID = nSubID;

            //初始化怪物和不同点结构信息
            DiffMake sDiffmake;
            sDiffmake.nDiffID = 0;
            sDiffmake.nRightOrLeft = 0;

            MonsterMake sMonstermake;
            sMonstermake.nIndex = 0;
            sMonstermake.nID = 0;
            sMonstermake.nBody = 0;
            sMonstermake.nSpeed = 0;
            sMonstermake.nMoveWay = 0;
            sMonstermake.nHitEffect = 0;
            sMonstermake.nHitTime = 0;
            sMonstermake.nX = 0;
            sMonstermake.nY = 0;
            sMonstermake.nDirection = 0;

            //取得场景下怪物和不同点的数量
            int nDCount = pStep.cdSubList[nSubID].DiffList.Count;
            int nMCount = pStep.cdSubList[nSubID].MonsterList.Count;

            tempByte = new byte[Marshal.SizeOf(stEnterStep)
                + sizeof(int) + nDCount * Marshal.SizeOf(sDiffmake)
                + sizeof(int) + nMCount * Marshal.SizeOf(sMonstermake)
                + sizeof(int)];

            byte[] btDCount = System.BitConverter.GetBytes(pStep.cdSubList[nSubID].DiffList.Count);
            byte[] btMCount = System.BitConverter.GetBytes(pStep.cdSubList[nSubID].MonsterList.Count);
            byte[] byteEnterStep = Common.Method.StructToBytes(stEnterStep);

            int offset = 0;
            byteEnterStep.CopyTo(tempByte, offset);

            //拼接不同点及怪物信息
            offset += byteEnterStep.Length;
            //拼接不同点信息
            btDCount.CopyTo(tempByte, offset);
            offset += 4;
            foreach (Diff diff in pStep.cdSubList[nSubID].DiffList.Values)
            {
                sDiffmake.Make(diff);
                byte[] byteDiffmake = Common.Method.StructToBytes(sDiffmake);
                Array.Copy(byteDiffmake, 0, tempByte, offset, byteDiffmake.Length);
                offset += byteDiffmake.Length;
            }
            //拼接怪物信息
            btMCount.CopyTo(tempByte, offset);
            offset += 4;
            foreach (Monster pMonster in pStep.cdSubList[nSubID].MonsterList.Values)
            {
                sMonstermake.Make(pMonster);
                byte[] bytemonstermake = Common.Method.StructToBytes(sMonstermake);
                Array.Copy(bytemonstermake, 0, tempByte, offset, bytemonstermake.Length);
                offset += bytemonstermake.Length;
            }

            //拼接boss信息
            byte[] btBossID = System.BitConverter.GetBytes(pStep.cdSubList[nSubID].m_nBossID);
            btBossID.CopyTo(tempByte, offset);
            offset += 4; 
        }
    }
}
