using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public enum Gate
    {
        Odd = 1,
        ThreeUp = 2,
        ThreeDown = 3,
        Even = 4,
        FourUp = 5,
        FourDown = 6
    }

    public enum GateState
    {
        NON_TRADE = -1,
        CAN_TRADE = 0,
        TRADED = 1
    }

    public class GateBet
    {
        public long Owner { get; set; }
        public int GateState { get; set; }
        public List<BetLog> Logs { get; set; }
    }

    public class BetInfo
    {
        public long TotalBet { get; set; }
        public long OwnBet { get; set; }
        public int State { get; set; }
    }
}