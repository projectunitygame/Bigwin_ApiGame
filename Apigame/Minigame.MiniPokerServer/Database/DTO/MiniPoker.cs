using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigame.MiniPokerServer.Database.DTO
{
    public class MiniPoker
    {
    }
    public class MiniPokerAccountHistory
    {
        public long SpinID { get; set; }
        public DateTime CreatedTime { get; set; }
        public int RoomID { get; set; }
        public long BetValue { get; set; }
        public int CardTypeID { get; set; }
        public long PrizeValue { get; set; }
        public string CardResult { get; set; }
    }
    public class MiniPokerAccountHistoryDetail
    {
        public DateTime CreatedTime { get; set; }
        public int RoomID { get; set; }
        public long BetValue { get; set; }
        public int CardTypeID { get; set; }
        public long PrizeValue { get; set; }
        public int CardID1 { get; set; }
        public int CardID2 { get; set; }
        public int CardID3 { get; set; }
        public int CardID4 { get; set; }
        public int CardID5 { get; set; }
    }
    public class MiniPokerListCardModel
    {
        public int CardID1 { get; set; }
        public int CardID2 { get; set; }
        public int CardID3 { get; set; }
        public int CardID4 { get; set; }
        public int CardID5 { get; set; }
        public int CardTypeID { get; set; }

    }
    public class MiniPokerSpinResponse
    {
        public int AccountID { get; set; }
        public int BetType { get; set; }
        public long SpinID { get; set; }
        public long BetValue { get; set; }
        public long PrizeValue { get; set; }
        public long Balance { get; set; }
        public long Jackpot { get; set; }
        public int ResponseStatus { get; set; }
        public List<MiniPokerListCardModel> Cards { get; set; }
        public int IsAutoFreeze { get; set; }
    }
    public class MiniPokerTopWinnerModel
    {
        public long SpinID { get; set; }
        public DateTime CreatedTime { get; set; }
        public int AccountID { get; set; }
        public string Username { get; set; }
        public int RoomID { get; set; }
        public long BetValue { get; set; }
        public int CardTypeID { get; set; }
        public long PrizeValue { get; set; }
        public string CardResult { get; set; }
    }
}
