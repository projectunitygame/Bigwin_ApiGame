using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Database.DTO
{
    public enum PlayerStatus
    {
        None,
        X2 = 1,
        Bonus = 2
    }

    public enum MoneyType
    {
        Gold = 1,
        Coin = 2
    }

    public enum X2Game
    {
        Init = 1,
        Select = 2,
        Stop = 0
    }
}