using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Database.DTO
{
    public class AccountInfo
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }

        public int RoomId { get; set; }

        public int FreeSpins { get; set; }

        public PlayerStatus Status { get; set; }

        public int LastPrizeValue { get; set; }

        public string LastLineData { get; set; }

        public int BonusSpinId { get; set; }
        public string BonusData { get; set; }

        public int TurnId { get; set; }

    }
}