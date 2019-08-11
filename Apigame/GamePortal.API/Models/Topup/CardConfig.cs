using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models.Topup
{
    public class CardConfig
    {
        public int ID { get; set; }
        public int Type { get; set; }
        public int Prize { get; set; }
        public bool Enable { get; set; }
        public int Promotion { get; set; }
        public int PromotionCashout { get; set; }
        public int CashoutRate { get; set; }
        public bool EnableCashout { get; set; }
        public int TopupRate { get; set; }
        public string PayOrderConfig { get; set; }
    }

    public class CardCheck
    {
        public int Type { get; set; }
        public bool Enable { get; set; }
        public List<CfgCard> Prizes { get; set; }
    }

    public class CfgCard
    {
        public long Prize { get; set; }
        public int Promotion { get; set; }
        public int Rate { get; set; }
    }
}