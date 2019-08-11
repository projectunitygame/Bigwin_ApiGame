using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Database.DTO
{
    public class OutputSpinData
    {
        public int SpinId { get; set; }
        public int TotalJackpotValue { get; set; }
        public int FreeSpins { get; set; }
        public int Jackpot { get; set; }
        public long Balance { get; set; }
        public int ResponseStatus { get; set; }
    }
}