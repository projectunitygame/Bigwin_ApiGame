using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.DTO
{
    public class Jackpot
    {
        public int RoomID {get;set;}
        public long JackpotFund { get; set; }
        public int GameID { get; set; }
    }
}