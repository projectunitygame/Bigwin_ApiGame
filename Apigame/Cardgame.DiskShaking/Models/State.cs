using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public enum State
    {
        WAITING = 0, //waiting
        SHAKING = 1, //xoc dia
        BETTING = 2, //dat cua
        SELL = 3, //nha cai duoi cua
        SHOW_RESULT = 4
    }

    public enum Action
    {
        BET = 1,
        SELL = 2,
        BUY = 3
    }

    public class Timing
    {
        private static int[] _timing = new int[] { 5, 3, 45, 5, 10 };
        public static int GetElappsed(State state)
        {
            return _timing[(int)state];
        }
    }
}