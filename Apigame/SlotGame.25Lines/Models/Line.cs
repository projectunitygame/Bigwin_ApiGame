using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models
{
    public class Line
    {
        public int LineId { get; set; }
        public int[] Slots { get; set; }
    }
}