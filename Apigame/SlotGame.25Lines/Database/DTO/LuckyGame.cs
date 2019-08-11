using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace SlotGame._25Lines.Database.DTO
{
    public class LuckyGame
    {
        [JsonIgnore]
        public int TurnId { get; set; }
        public int RemainTurn { get; set; }
        public int PrizeValue { get; set; }
        public long Balance { get; set; }
        public int ResponseStatus { get; set; }
    }
}