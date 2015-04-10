using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.SuperStar.Scripts.Network.model
{
    public class ResultBase
    {
        public int result { get; set; }
        //public List<object> data { get; set; }
        public string sign { get; set; }
        public string error { get; set; }
    }

}
