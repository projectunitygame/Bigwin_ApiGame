using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public class BetGateData
    {
        public long amount { get; set; }
        public Gate gate { get; set; }
    }

    public class BetGateResponse
    {
        public long amount { get; set; }
        public long gateTotal { get; set; }
        public Gate gate { get; set; }
        public int error { get; set; }
    }
}