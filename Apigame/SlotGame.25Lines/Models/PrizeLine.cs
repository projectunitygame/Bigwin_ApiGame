using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models
{
    public class PrizeLine
    {
        public int LineId { get; set; } // id dòng trúng
        public int PrizeId { get; set; }
        public int[] Position { get; set; } // các vị trí trúng
        public int PrizeValue { get; set; }
    }
}