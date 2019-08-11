using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigames.DataAccess.DTO
{
    public class MiniHilo
    {
    }
    public class HiLoGetJackpot
    {
        public long JackpotFund { get; set; }
    }
    public class HiLoGetAccountInfoResponse
    {
        public long currentTurnId { get; set; }
        public int currentStep { get; set; }
        public int currentRoomId { get; set; }
        public int currentBetType { get; set; }
        public long currentBetValue { get; set; }
        public string currentCardData { get; set; }
        public string currentAces { get; set; }
        public int acesCount { get; set; }
        public int remainTime { get; set; }
        public decimal betRateUp { get; set; }
        public decimal betRateDown { get; set; }
        public int responseStatus { get; set; }

        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime CurrentTime { get; set; }
    }
    public class HiLoSetBetResponse
    {
        public long betValue { get; set; }
        public long prizeValue { get; set; }
        public int isJackpot { get; set; }
        public long turnId { get; set; }
        public int step { get; set; }
        public int cardId { get; set; }
        public decimal betRateUp { get; set; }
        public decimal betRateDown { get; set; }
        public long balance { get; set; }
        public int responseStatus { get; set; }
        public int IsBonus { get; set; }
        public int TotalPoint { get; set; }
    }
    public class HiLoGetTopAccount
    {
        public int AccountID { get; set; }
        public string UserName { get; set; }
        public int RoomID { get; set; }
        public int Type { get; set; }
        public long TurnID { get; set; }
        public long PrizeValue { get; set; }
        public DateTime CreatedTime { get; set; }
    }
    public class HiLoGetAccountHistory
    {
        public long TurnID { get; set; }
        public long BetValue { get; set; }
        public int LocationID { get; set; }
        public long PrizeValue { get; set; }
        public int CardID { get; set; }
        public int Step { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
