using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities;

namespace Minigame.HooHeyHowServer.Models
{
    public class GameResult
    {
        public BetGate Dice1 { get; private set; }
        public BetGate Dice2 { get; private set; }
        public BetGate Dice3 { get; private set; }
        public long SessionId { get; private set; }

        public GameResult()
        {
            Refresh();
        }

        public void GenerateResult()
        {
            Dice1 = (BetGate)(1 + RandomUtil.NextByte(6));
            Dice2 = (BetGate)(1 + RandomUtil.NextByte(6));
            Dice3 = (BetGate)(1 + RandomUtil.NextByte(6));
        }

        public void Refresh()
        {
            Dice1 = BetGate.NONE;
            Dice2 = BetGate.NONE;
            Dice3 = BetGate.NONE;
        }
    }
}