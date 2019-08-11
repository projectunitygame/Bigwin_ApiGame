using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._20lines.Game2.Database.DTO
{
    public class AccountInfo
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public int FreeSpin { get; set; }
        public string LastLineData { get; set; }
        public long LastPrizeValue { get; set; }
        public long BonusID { get; set; }
        public string BonusData { get; set; }
        public int ResponseStatus { get; set; }
    }
}