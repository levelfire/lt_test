using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTest.Model
{
    public enum BagItemFlag
    {
        equipment,

    }
    public class MBag : ResultBase
    {
        public string sign { get; set; }
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
}
