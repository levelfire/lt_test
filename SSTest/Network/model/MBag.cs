using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SuperStar.Scripts.Network.model
{
    //获取背包信息
    public class MBag : ResultBase
    {
        public databag data { get; set; }
    }

    public class databag
    {
        public baginfo backpack { get; set; }
    }

    public class baginfo
    {
        public List<item> s_equipment { get; set; }
        public List<item> s_material { get; set; }
        public List<item> s_fragment { get; set; }
        public List<item> s_consumables { get; set; }
        public List<item> s_card { get; set; }
    }

    public class MBagOP : ResultBase
    {
        public List<object> data { get; set; }
    }
}
