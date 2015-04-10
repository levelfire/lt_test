using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Server_Form.GameData
{
    public class BossData
    {
        /// <summary>
        /// 全局怪物表
        /// </summary>
        public static ConcurrentDictionary<int, BossData> BossDataList = new ConcurrentDictionary<int, BossData>(Define.concurrencyLevel, Define.initialCapacity);

        public int m_nID;

        public int m_nFireLoop;

        public static void init(int nID,int nLoop)
        {
            BossData data = new BossData();
            data.m_nID = nID;
            data.m_nFireLoop = nLoop;
            BossDataList.TryAdd(data.m_nID, data);
        }
    }
}
