using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
namespace Game.Events.Database.DTO
{
    public class BigWinPlayers
    {
        [JsonIgnore]
        public long ID { get; set; }
        [JsonIgnore]
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public int PrizeValue { get; set; }
        public int Type { get; set; }
        public int GameID { get; set; }
        [JsonIgnore]
        public int SessionID { get; set; }
        [JsonIgnore]
        public string Description { get; set; }
    }
}