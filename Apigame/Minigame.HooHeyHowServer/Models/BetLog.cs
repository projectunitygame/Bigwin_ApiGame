using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Minigame.HooHeyHowServer.Models
{
    [Serializable]
    public class BetLog
    {
        [JsonIgnore]
        public long accountId { get; set; }
        [JsonIgnore]
        public string accountName { get; set; }
        [JsonIgnore]
        public BetGate betGate { get; set; }
        [JsonIgnore]
        public long amount { get; set; }
        public long award { get; set; }
    }
}