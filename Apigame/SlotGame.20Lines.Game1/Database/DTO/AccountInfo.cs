using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotGame._20Lines.Game1.Database.DTO
{
    public class AccountInfo
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public int FreeSpin { get; set; }
        public string LastLineData { get; set; }
        public long LastPrizeValue { get; set; } 
        public long BonusID { get; set; }
        public int ResponseStatus { get; set; }
    }
}
