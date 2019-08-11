using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._20lines.Game2.Models
{
    public enum MoneyType
    {
        Gold = 1,
        Coin = 2
    }

    public class Enums
    {

        public enum ErrorCode
        {
            SuccessRequest = 1,
            Exception = -99,
            NotAuthen = -1001
        }
    }
}