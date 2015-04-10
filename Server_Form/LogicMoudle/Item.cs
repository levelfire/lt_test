using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.LogicMoudle
{
    public class Item
    {
        public enum ItemUseType
        {
            Find        = 0x0001,
            Speed       = 0x0002,
            Invincible  = 0x0004,
            AOE         = 0x0008,
            Stun        = 0x0010,
            TimeSlow    = 0x0020,
            AT          = 0x0040,
            Hammer      = 0x0080,
            Trans       = 0x0100,
        }
        

        private int m_nInsID;
        public int NInsID
        {
            get { return m_nInsID; }
            set { m_nInsID = value; }
        }

        private int m_nID;

        public int NID
        {
            get { return m_nID; }
            set { m_nID = value; }
        }
        private int m_nCount;

        public int NCount
        {
            get { return m_nCount; }
            set { m_nCount = value; }
        }

        private int m_nFlag;

        public int NFlag
        {
            get { return m_nFlag; }
            set { m_nFlag = value; }
        }
        private int m_nType;

        public int NType
        {
            get { return m_nType; }
            set { m_nType = value; }
        }



    }
}
