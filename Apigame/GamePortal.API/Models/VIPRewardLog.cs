using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class VIPRewardLog
    {
        public int Rank { get; set; }
        public long Reward { get; set; }
        public bool Status { get; set; }
    }
}