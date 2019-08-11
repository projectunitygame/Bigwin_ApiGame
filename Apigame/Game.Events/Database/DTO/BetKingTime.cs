using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.DTO
{
    public class BetKingTime
    {
        public int Start { get; set; }
        public int End { get; set; }
        [JsonIgnore]
        public int Day { get; set; }
    }
}