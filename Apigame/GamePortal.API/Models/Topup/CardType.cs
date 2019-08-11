using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models.Topup
{
    public class CardType
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public string ShortCode { get; set; }
        public List<CardTopup> Prices { get; set; }
    }
}