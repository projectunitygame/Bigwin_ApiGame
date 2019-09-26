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
        public int Point { get; set; }
        public long Gold { get; set; }
        public int LevelMax { get; set; }
        public string LevelReward { get; set; }
        public int ResponseStatus { get; set; }
    }
}