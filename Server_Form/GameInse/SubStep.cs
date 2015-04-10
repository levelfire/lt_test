using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Timers;
using Server_Form.Protorol;

namespace Server_Form.GameInse
{
    public class SubStep
    {
        /// <summary>
        /// 图中不同点的列表
        /// </summary>
        public ConcurrentDictionary<int, Diff> DiffList = new ConcurrentDictionary<int, Diff>(Define.concurrencyLevel, Define.initialCapacity);

        /// <summary>
        /// 图中怪物列表
        /// </summary>
        public ConcurrentDictionary<int, Monster> MonsterList = new ConcurrentDictionary<int, Monster>(Define.concurrencyLevel, Define.initialCapacity);


        public SubStep(Step ParentStep)
        {
            this.ParentStep = ParentStep;
        }

        public System.Timers.Timer BossTimer;
        public int m_nBossID;

        /// <summary>
        /// 子管卡图ID
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// 子管卡图Index
        /// </summary>
        public int Index
        {
            get;
            set;
        }
        /// <summary>
        /// 父关卡
        /// </summary>
        protected Step ParentStep
        {
            get;
            set;
        }
        public Diff GetDiff(int DiffIndex)
        {
            if (DiffList.ContainsKey(DiffIndex))
            {
                return DiffList[DiffIndex];
            }
            else
            {
                return null;
            }
        }

        public void BossFire(object source, ElapsedEventArgs e)
        {
            foreach (Player p in ParentStep.cdPlayerList.Values)
            {
                p.MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                      (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_BossFire,
                                        System.BitConverter.GetBytes(m_nBossID));
            }
        }

        public void StartBossFireLoop()
        {
            //if (Loop > 0)
            //{
            int nloop = Server_Form.GameData.BossData.BossDataList[m_nBossID].m_nFireLoop;
            BossTimer = new System.Timers.Timer(nloop);

                // Hook up the Elapsed event for the timer.
            BossTimer.Elapsed += new ElapsedEventHandler(BossFire);

                // Set the Interval to Loop ( milliseconds).
            BossTimer.Interval = nloop;
            BossTimer.Enabled = true;
            BossTimer.AutoReset = true;
            //}
        }

        public void Start()
        {
            //2011.11.17 子场景开始后，怪物开始移动
            foreach (Monster m in MonsterList.Values)
            {
                m.StartMoveLoop();
            }
            //// boss发子弹循环，暂时放在substep下；目前没有boss实例，之后boss功能复杂化后，在写boss的实例类 [12/26/2011 test]
            //if (0 != m_nBossID)
            //{
            //    StartBossFireLoop();
            //}
        }
    }
}
