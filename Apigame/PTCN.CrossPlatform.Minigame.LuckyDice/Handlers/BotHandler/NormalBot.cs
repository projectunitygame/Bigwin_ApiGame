using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using Utilities;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Handlers.BotHandler
{
    public class NormalBot : Bot
    {
        public override void Init()
        {
            Style = BetStyle.Normal;
        }

        public override void Check()
        {
            var side = RandomUtil.NextInt(2);
            BetSide = (BetSide)side;
            BetAmount = BetValues[RandomUtil.NextInt(BetValues.Length)];
            if (BetAmount < 0)
                BetAmount = RandomUtil.NextInt(BetValues[0], BetValues.Max());
        }
    }
}