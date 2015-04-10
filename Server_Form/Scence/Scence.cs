using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server_Form.GameInse;
using System.Collections.Concurrent;

namespace Server_Form.Scence
{
    class Scence
    {
        //场景中的玩家
        public ConcurrentDictionary<int, Player> PlayerList = new ConcurrentDictionary<int, Player>(Define.concurrencyLevel, Define.initialCapacity);

        //protected SubStep CurrentSubStep;

        public bool AddPlayer(Player pPlayer)
        {
            if (PlayerList.TryAdd(pPlayer.NPlayerID, pPlayer))
            {
                return true;
            }
            return false;
        }

    }
}
