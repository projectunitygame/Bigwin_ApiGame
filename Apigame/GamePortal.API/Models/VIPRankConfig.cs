using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class VIPRankConfig
    {
        public int Rank { get; set; }
        public int VP { get; set; }
        public long Reward { get; set; }
    }
}