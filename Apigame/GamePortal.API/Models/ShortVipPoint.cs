using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class ShortVipPoint
    {
        public int LevelVip { get; set; }
        public long Exp { get; set; }
        public int VipPoint { get; set; }
        public long Gold { get; set; }
        public string DateBirthDay { get; set; }
        public int ResponseStatus { get; set; }
    }
}