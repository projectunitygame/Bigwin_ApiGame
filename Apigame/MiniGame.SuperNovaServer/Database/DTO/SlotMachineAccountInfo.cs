using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class SlotMachineAccountInfo
    {
        public long AccountID { get; set; }

        public string UserName { get; set; }

        public long LastPrizeValue { get; set; }

        public string LastLinesData { get; set; }

        public int ResponseStatus { get; set; }

        public string Message { get; set; }

        public long Jackpot { get; set; }
        
        public long Balance { get; set; }
        
        public int BetValue { get; set; }

        public int SourceID { get; set; }

        public int MerchantID { get; set; }

    }
}
