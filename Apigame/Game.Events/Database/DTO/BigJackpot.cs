using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.DTO
{
    public class BigJackpot
    {
        public int RoomId { get; set; }
        public int JackpotCount { get; set; }
        public int QuantityInDay { get; set; }
        public bool IsEventJackpot { get; set; }
        public int Multi { get; set; }
    }

    public class BigJackpotInfo
    {
        public List<BigJackpot> List { get; set; }
        public bool IsEvent { get; set; }
    }

    public class BigJackpotHistory
    {
        public string AccountName { get; set; }
        public long Jackpot { get; set; }
        public int RoomID { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}