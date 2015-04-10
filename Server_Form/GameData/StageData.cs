using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Server_Form.GameData
{
    class StageData
    {
        //全局Stage列表
        public static ConcurrentDictionary<int, StageData> StageList = new ConcurrentDictionary<int, StageData>(Define.concurrencyLevel, Define.initialCapacity);
        //Stage下，step的列表
        public ConcurrentDictionary<int, StepData> StepList = new ConcurrentDictionary<int, StepData>(Define.concurrencyLevel, Define.initialCapacity);
        //StageID
        public int ID
        {
            get;
            set;
        }
    }
}
