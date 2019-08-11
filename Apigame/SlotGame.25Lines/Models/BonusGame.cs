using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SlotGame._25Lines.Models
{
    public class BonusGame
    {
        public string BonusData { get; set; }
        [JsonIgnore]
        public float DataMultiplier { get; set; }
        public int Mutiplier { get; set; }
        public int TotalPrizeValue { get; set; }
    }
}