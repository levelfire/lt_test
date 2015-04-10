using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form.GameInse
{
    public class Diff
    {
        public Diff(int nIndex,int rorl,DiffData pDData)
        {
            if (null == pDData)
            {
                return;
            }
            Index = nIndex;
            RightOrLeft = rorl;

            ID = pDData.ID;
            bKnocked = false;
            PosX = pDData.PosX;
            PosY = pDData.PosY;
            RightX = pDData.RightX;
            RightY = pDData.RightY;
            LeftX = pDData.LeftX;
            LeftY = pDData.LeftY;
        }
        public Diff()
        {
            
        }
        //不同点ID
        public int ID
        {
            get;
            set;
        }
        //不同点Index
        public int Index
        {
            get;
            set;
        }

        public int RightOrLeft
        {
            get;
            set;
        }

        //差异mc摆放的x坐标
        public int PosX
        {
            get;
            set;
        }
        //差异mc摆放的y坐标
        public int PosY
        {
            get;
            set;
        }

        //击中判定矩形的左边界
        public int LeftX
        {
            get;
            set;
        }
        //击中判定矩形的上边界
        public int LeftY
        {
            get;
            set;
        }
        //击中判定矩形的右边界
        public int RightX
        {
            get;
            set;
        }
        //击中判定矩形的下边界
        public int RightY
        {
            get;
            set;
        }

        public bool bKnocked
        {
            get;
            set;
        }
    }
}
