using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SuperStar.Scripts.Network.model
{
    //获取玩家信息
    public class MUserInfo : ResultBase
    {
        public data data { get; set; }
    }

    public class player
    {
        public string uname { get; set; }//唯一名字
        public string dis_name { get; set; }//显示名字
        public int gold { get; set; }//金币
        public int gem { get; set; }//钻石
        public int strength { get; set; }//体力
        public int strength_cd { get; set; }//体力cd
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

        public List<item> s_equipment { get; set; }//装备
        public List<item> s_material { get; set; }//材料
        public List<item> s_fragment { get; set; }//碎片
        public List<item> s_consumables { get; set; }//消耗品
        public List<item> s_card { get; set; }//图鉴
    }

    public class item
    {
        public string uuid { get; set; }//实例ID（唯一ID）
        public int id { get; set; }//物品ID（配置ID）
        public int count { get; set; }//数量
    }
}
