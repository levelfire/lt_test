using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server_Form
{
    public class DiffData
    {
        //不同点ID
        public int ID
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
    }
}
