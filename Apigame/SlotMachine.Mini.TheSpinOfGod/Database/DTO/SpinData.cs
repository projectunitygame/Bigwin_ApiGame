
namespace DataAccess.DTO
{
    public class InputSpin
    {
        public int ServiceId { get; set; } 
        public string AccessToken { get; set; } 
        public long AccountId { get; set; } 
        public string UserName { get; set; } 
        public int BetType { get; set; } 
        public int RoomId { get; set; } 
        public string LinesData { get; set; } 
        public string ClientIp { get; set; } 
        public int SourceId { get; set; }
        public int MerchantId { get; set; }
    }
    public class SpinData
    {
        public long SpinId { get; set; }
        public string SlotData { get; set; }
        public string PrizesData { get; set; }
        public int TotalBetValue { get; set; }
        public long TotalPrizeValue {get;set;}
        public bool IsJackpot { get; set; }
        public long Jackpot { get; set; }
        public long Balance { get; set; }
        public int ResponseStatus { get; set; }
        public int TotalJackPot { get; set; }
        public string LuckyData { get; set; }
    }
}
