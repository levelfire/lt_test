using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Data;

namespace Server_Form.GameData
{
    public class MonsterData
    {
        /// <summary>
        /// 全局怪物表
        /// </summary>
        public static ConcurrentDictionary<int, MonsterData> MonsterDataList = new ConcurrentDictionary<int, MonsterData>(Define.concurrencyLevel, Define.initialCapacity);

        //static MonsterData()
        //{
        //    using (DataTable dt = DAL.Monster.GetMonsterData())
        //    {
        //        foreach (DataRow dr in dt.Rows)
        //        {

        //            this.ID = Convert.ToInt32(dr["ID"]);
        //            this.Body = Convert.ToInt32(dr["body"]);
        //            this.Speed = Convert.ToInt32(dr["speed"]);
        //            this.MoveWay = Convert.ToInt32(dr["moveway"]);
        //            this.HitEffect = Convert.ToInt32(dr["hiteffect"]);
        //            this.HitTime = Convert.ToInt32(dr["hittime"]);
        //            MonsterDataList.TryAdd(this.ID, this);
        //        }
        //    }
        //}

        public static void init()
        {
            //DataTable dt = DAL.Monster.GetMonsterData();
            DataTable dt = null;
            foreach (DataRow dr in dt.Rows)
            {
                MonsterData data = new MonsterData();
                data.ID = Convert.ToInt32(dr["ID"]);
                data.Body = Convert.ToInt32(dr["body"]);
                data.Speed = Convert.ToInt32(dr["speed"]);
                data.MoveWay = Convert.ToInt32(dr["moveway"]);
                data.Angle = Convert.ToInt32(dr["angle"]);
                data.Loop = Convert.ToInt32(dr["loop"]);
                data.HitEffect = Convert.ToInt32(dr["hiteffect"]);
                data.HitTime = Convert.ToInt32(dr["hittime"]);
                MonsterDataList.TryAdd(data.ID, data);
            }
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
    }
}
