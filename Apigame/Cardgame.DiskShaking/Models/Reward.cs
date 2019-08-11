using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    [Serializable]
    public class Reward
    {
        public int Gate { get; set; }
        public long Prize { get; set; }
        public long Refund { get; set; }
        public long Lose { get; set; }
    }
}