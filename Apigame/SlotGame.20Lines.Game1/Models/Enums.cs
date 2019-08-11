using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._20Lines.Game1.Models
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
            NotAuthen = -1001,
            PlayerNull = -1002,
            DuplicateSpin = -10003,
            InvalidBalance = -10004
        }
    }
}