using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class TopupHistory
    {
        public int CardType { get; set; }
        public int Amount { get; set; }
        public string Pin { get; set; }
        public string Serial { get; set; }
        public int Status { get; set; }
    }
}