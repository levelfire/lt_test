using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSTest.Model
{
    public class MUserInfo : ResultBase
    {
        public int result { get; set; }
        public string sign { get; set; }
        public data data { get; set; }
    }

    public class player
    {
        public string uname { get; set; }
        public string dis_name { get; set; }
        public int gold { get; set; }
        public int gem { get; set; }
        public int strength { get; set; }
        public int strength_cd { get; set; }
    }
    public class data
    {
        public backpack backpack { get; set; }
        public player player { get; set; }
    }

    public class backpack
    {
        public int capacity { get; set; }

        public List<item> equipment { get; set; }
        public List<item> material { get; set; }
        public List<item> property { get; set; }
        public List<item> equip_piece { get; set; }
        public List<item> inscription { get; set; }
        public List<item> inscription_piece { get; set; }

        public List<item> s_equipment { get; set; }
        public List<item> s_material { get; set; }
        public List<item> s_fragment { get; set; }
        public List<item> s_consumables { get; set; }
        public List<item> s_card { get; set; }
    }

    public class item
    {
        public string uuid { get; set; }
        public int id { get; set; }
        public int count { get; set; }
    }
}
