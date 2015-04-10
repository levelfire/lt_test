using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server_Form.GameData;
using System.Timers;
using Server_Form.Protorol;

namespace Server_Form.GameInse
{
    public class Monster
    {
        public Monster(int nIndex, MonsterData pMData,SubStep pSub)
        {
            if (null == pMData)
            {
                return;
            }
            Index = nIndex;
            ID = pMData.ID;
            Body = pMData.Body;
            Speed = pMData.Speed;
            MoveWay = pMData.MoveWay;
            Angle = pMData.Angle;
            Loop = pMData.Loop;
            HitEffect = pMData.HitEffect;
            HitTime = pMData.HitTime;

            //Random r = new Random(Guid.NewGuid().GetHashCode());
            //X = r.Next(100, SubStepData.width);
            //Y = r.Next(100, SubStepData.height);

            bool nSuss = false;

            while (!nSuss)
            {
                nSuss = true;

                Random r = new Random(Guid.NewGuid().GetHashCode());
                X = r.Next(100, SubStepData.width);
                Y = r.Next(100, SubStepData.height);

                foreach (Diff d in pSub.DiffList.Values)
                {
                    if (((X - d.PosX) * (X - d.PosX) + (Y - d.PosY) * (Y - d.PosY)) <= 90)
                    {
                        nSuss = false;
                        break;
                    }
                }
            }

            Direction = Angle;
            //if (0 != MoveWay)
            //{
            //    if (1 == r.Next(1, 3))
            //    {
            //        Direction = 1;
            //    }
            //    else
            //    {
            //        Direction = -1;
            //    }
            //}
            //else
            //{
            //    Direction = 0;
            //}
        }
        /// <summary>
        /// 怪物Index
        /// </summary>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// 怪物ID
        /// </summary>
        public int ID
        {
            get;
            set;
        }

        /// <summary>
        /// 体型
        /// </summary>
        public int Body
        {
            get;
            set;
        }

        /// <summary>
        /// 速度
        /// </summary>
        public int Speed
        {
            get;
            set;
        }

        /// <summary>
        /// 移动方式
        /// </summary>
        public int MoveWay
        {
            get;
            set;
        }

        /// <summary>
        /// 移动角度
        /// </summary>
        public int Angle
        {
            get;
            set;
        }

        /// <summary>
        /// 移动状态变化循环时间
        /// </summary>
        public int Loop
        {
            get;
            set;
        }

        /// <summary>
        /// 碰撞效果
        /// </summary>
        public int HitEffect
        {
            get;
            set;
        }

        /// <summary>
        /// 碰撞效果持续时间
        /// </summary>
        public int HitTime
        {
            get;
            set;
        }

        /// <summary>
        /// 出生点X
        /// </summary>
        public int X
        {
            get;
            set;
        }

        /// <summary>
        /// 出生点Y
        /// </summary>
        public int Y
        {
            get;
            set;
        }

        private Step m_CurStep;

        /// <summary>
        /// 所属Step
        /// </summary>
        public Step CurStep
        {
            get { return m_CurStep; }
            set { m_CurStep = value; }
        }

        /// <summary>
        /// 初始运动方向
        /// </summary>
        /// 现在只有轴向移动，暂时用，正向1，反向-1，不动0
        /// 等以后需要多种运动模式了，再用向量计算
        public int Direction
        {
            get;
            set;
        }

        private System.Timers.Timer aTimer;

        enum MoveDirection
        {
            Stop = 0,
            OneWay = 1,
            Reverse = 2,
            Random = 3,
        }

        public void ChangeMoveState(object source, ElapsedEventArgs e)
        {
            int nOldA = Angle;
            switch (MoveWay)
            {
                case (int)MoveDirection.Reverse:
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        int nOffset = r.Next(0, 2);

                        Angle += 180 * nOffset;
                        if (Angle >= 360)
                        {
                            Angle = Angle % 360;
                        }
                    }
                    break;
                case (int)MoveDirection.Random:
                    {
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        Angle = r.Next(0, 360);
                    }
                    break;
                case (int)MoveDirection.Stop:
                case (int)MoveDirection.OneWay:
                default:
                    break;
            }

            if (nOldA != Angle)
            {
                MonsterMoveChange stChange;

                stChange.nMonsterIndex = Index;
                stChange.nAngle = Angle;

                byte[] btChange = Common.Method.StructToBytes(stChange);

                foreach (Player p in m_CurStep.cdPlayerList.Values)
                {
                    p.MakeAndSendDatagram((byte)ProtorlEnum.FrameGroup.FrameGroup_ServerToUser,
                                          (byte)ProtorlEnum.FrameType_ServerToUser.FrameType_MonsterChangeMoveDir,
                                            btChange);
                }
            }
        }

        public void StartMoveLoop()
        {
            if (Loop > 0)
            {
                aTimer = new System.Timers.Timer(Loop);

                // Hook up the Elapsed event for the timer.
                aTimer.Elapsed += new ElapsedEventHandler(ChangeMoveState);

                // Set the Interval to Loop ( milliseconds).
                aTimer.Interval = Loop;
                aTimer.Enabled = true;
                aTimer.AutoReset = true;
            }
        }
    }
}
