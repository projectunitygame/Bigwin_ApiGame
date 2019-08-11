using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotGame._25Lines.Database.DTO
{
    public class JackpotHistory
    {
        public long SpinID { get; set; }
        public int RoomID { get; set; }
        public long AccountID { get; set; }
        public string Username { get; set; }
        public long PrizeValue { get; set; }
        public int GameType { get; set; }
        public DateTime CreatedTime { get; set; }
		public int Jackpot { get; set; }
    }

    public class JackpotHistoryList
    {
        public List<JackpotHistory> JackpotsHistory { get; set; }
        public int TotalRecord { get; set; }
    }
    public class Top2Jackpot
    {
        public int GameType { get; set; }
        public int RoomID { get; set; }
        public string Username { get; set; }
        public long PrizeValue { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
