using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace Server_Form.GameData
{
    class StepData
    {
        //房间关卡下，图的列表
        public ConcurrentDictionary<int, SubStepData> SubStepList = new ConcurrentDictionary<int, SubStepData>(Define.concurrencyLevel, Define.initialCapacity);
        //关卡ID
        public int ID
        {
            get;
            set;
        }
        //完成此关卡的时限，单位秒
        public int Time
        {
            get;
            set;
        }
    }
}
