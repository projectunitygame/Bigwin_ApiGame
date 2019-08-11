using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Database.DTO
{
    public class InputSpinData
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public int RoomId { get; set; }
        public MoneyType MoneyType { get; set; }
        public string LineData { get; set; }
        public int TotalBetValue { get; set; }
        public string SlotsData { get; set; }
        public bool IsJackpot { get; set; }
        public int AddFreeSpins { get; set; }
        public int TotalPrizeValue { get; set; }
        public int TotalBonusValue { get; set; }

    }
}