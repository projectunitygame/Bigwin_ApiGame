using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models.EventBetKing
{
    public class BetKingState
    {
        public string ID { get; set; }
        public long Award { get; set; }
        public long Lose { get; set; }
        public long Gained { get; set; }
    }
}