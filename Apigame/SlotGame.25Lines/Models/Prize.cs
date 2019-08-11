using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models
{
    public class Prize
    {
        public int PrizeId { get; set; }
        public string PrizeName { get; set; }
        public int Multiplier { get; set; }

        public int FreeSpins { get; set; }

        public int StartBonus { get; set; }
    }
}