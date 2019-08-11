using System;
using System.Collections.Generic;

namespace SlotGame._20Lines.Game1.Database.DTO
{
    public class JackpotHistory
    {
        public long RowNum { get; set; }

        public long SpinID { get; set; }

        public int RoomID { get; set; }

        public long BetValue { get; set; }

        public int AccountID { get; set; }

        public string Username { get; set; }

        public int PrizeID { get; set; }

        public int GameType { get; set; }

        public long PrizeValue { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    public class JackpotHistoryList
    {
        public List<JackpotHistory> JackpotsHistory { get; set; }
        public int TotalRecord { get; set; }
    }
}
