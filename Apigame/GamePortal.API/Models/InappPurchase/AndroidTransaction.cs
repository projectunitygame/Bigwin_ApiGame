using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models.InappPurchase
{
    public class AndroidTransaction
    {
        /// <summary>
        /// must be "androidpublisher#productPurchase"
        /// </summary>
        public string kind { get; set; }
        public long purchaseTime { get; set; }
        /// <summary>
        /// purchaseState = 0 => Purchased
        /// </summary>
        public int purchaseState { get; set; }
        public int consumptionState { get; set; }
        public string developerPayload { get; set; }
        public string OrderId { get; set; }
    }
}