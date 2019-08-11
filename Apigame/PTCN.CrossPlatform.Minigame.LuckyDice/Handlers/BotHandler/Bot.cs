using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using Utilities.IP;
using Utilities.Session;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Handlers.BotHandler
{
    public abstract class Bot
    {
        public string DisplayName { get; set; }
        public BetSide BetSide { get; set; }
        public int BetAmount { get; set; }
        public BetStyle Style { get; set; }

        public int[] BetValues { get; set; }

        public BetSide[] BetSides { get; set; }

        public int Wins { get; set; } // so tran thang

        public int Losses { get; set; } // so tran thua

        public int WinStreak { get; set; } // chuoi thang

        public int LoseStreak { get; set; } // chuoi thua

        public void UpdateBetValues(int[] betValues)
        {
            this.BetValues = betValues;
        }

        public int Vip { get; set; }

        protected Bot() => Init();

        public abstract void Init();

        public abstract void Check();

    }
}