using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models.Topup
{
    public class CardTopup
    {
        [JsonIgnore]
        public int CardType { get; set; }
        public long Value { get; set; }
        public decimal Bonus { get; set; }
        public long GoldValue { get; set; }
    }
}