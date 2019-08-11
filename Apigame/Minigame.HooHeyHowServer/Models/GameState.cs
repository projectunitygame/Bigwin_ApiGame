using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Minigame.HooHeyHowServer.Models
{
    public enum GameState
    {
        PREPAIRING = 0,
        SHAKING = 1,
        BETTING = 2,
        REINITIALIZE = 4,
    }
    [Serializable]
    public enum BetGate
    {
        NONE = 0,
        DEER = 1,
        GOURD = 2,
        CHICKEN = 3,
        FISH = 4,
        CRAB = 5,
        SHRIMP = 6,
    }

    public enum MoneyType
    {
        GOLD = 1,
        COIN = 2
    }

    public class Timing
    {
        private static int[] _timing = new int [] { 15, 5, 60, 5 };
        public static int GetElappsed(GameState state)
        {
            return _timing[(int)state];
        }
    }
}