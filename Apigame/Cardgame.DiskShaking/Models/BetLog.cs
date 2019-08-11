using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public class BetLog
    {
        public long accountId { get; set; }
        public string accountName { get; set; }
        public Gate betGate { get; set; }
        public long betAmount { get; set; }
    }
}