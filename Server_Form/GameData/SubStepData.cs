using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Server_Form.GameData
{
    class SubStepData
    {
        /// <summary>
        /// 图中不同点的列表
        /// </summary>
        public ConcurrentDictionary<int, DiffData> DiffList = new ConcurrentDictionary<int, DiffData>(Define.concurrencyLevel, Define.initialCapacity);
        /// <summary>
        /// 图中怪物列表
        /// </summary>
        public ConcurrentDictionary<int, MonsterData> MonsterList = new ConcurrentDictionary<int, MonsterData>(Define.concurrencyLevel, Define.initialCapacity);

        public static int width = 806;
        public static int height = 340;
        public static int dis_rtol = 466;
        /// <summary>
        /// 子管卡图ID
        /// </summary>
        public int ID
        {
            get;
            set;
        }
        /// <summary>
        /// 可以出现的差异的数量
        /// </summary>
        public int MaxDiff
        {
            get;
            set;
        }

        public int m_BossID;
    }
}
