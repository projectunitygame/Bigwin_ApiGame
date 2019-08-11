using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTO
{

    public class HistoryInfor
    {
        public long SpinID { get; set; }
        public int RoomID { get; set; }
        public int AccountID { get; set; }
        public string Username { get; set; }
        public int BetValue { get; set; }
        public string LinesData { get; set; }
        public string SlotsData { get; set; }
        public string PrizesData { get; set; }
        public int TotalLines { get; set; }
        public long TotalBetValue { get; set; }
        public long TotalPrizeValue { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class ListHistoryInfo
    {
        public int TotalCount { get; set; }
        public List<HistoryInfor> HistoryInfo { get; set; }
    }
    public class TopAccount
    {
        public string AccountName { get; set; }
        public int TotalValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    [Serializable]
    public class PrizeTable
    {
        public int PrizeID { get; set; }
        public string PrizeName{ get; set; }
        public int DisplayQuantity{ get; set; }
    }

    [Serializable]
    public class MainGame_AccountPrizes
    {
        public int AccountPrizeID { get; set; }
        public int PrizeID { get; set; }
        public string PrizeName { get; set; }
        public long PrizeValue { get; set; }
        public int Status { get; set; }
    }

    [Serializable]
    public class HonorEvent
    {
        public string Username { get; set; }
        public string PrizeName { get; set; }
        public string EventName { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    [Serializable]
    public class RegisterEventInfor
    {
        public string UserName { get; set; }
        public int ResponseStatus { get; set; }
    }

    /// <summary>
    /// Hòm quà sự kiện
    /// Người tạo: Vinh.vu
    /// </summary>
    [Serializable]
    public class Event_AccountPrizes
    {
        public long LogID { get; set; }
        public long AccountPrizeID { get; set; }
        public int PrizeType { get; set; }
        public int PrizeID { get; set; }
        public int Quantity { get; set; }
        public int EventID { get; set; }
        public int Status { get; set; }
        public string Description { get; set; }
        
    }

    [Serializable]
    public class Spins_ResultTable
    {
        public int Index { get; set; }
        public int IsDrop { get; set; }
        public int PrizeID { get; set; }
        public int PrizeValue { get; set; }
    }

    [Serializable]
    public class Spins_ResultTableClient
    {
        public int BagPosition { get; set; }
        public int Index { get; set; }
        public int IsDrop { get; set; }
        public int PrizeID { get; set; }
        public int PrizeValue { get; set; }
    }
    /// <summary>
    /// Lịch sử gieo quẻ
    /// </summary>
    [Serializable]
    public class HistoryGieoQue
    {
        public DateTime CreatedTime { get; set; }
        public string LogType { get; set; }
        public int Number { get; set; }
        public string Details { get; set; }
        public string Prizes { get; set; }
    }
   /// <summary>
   /// Lịch sử hũ
   /// Người tạo: Vinh.vu
   /// </summary>
    [Serializable]
    public class HistoryHu
    {
        public int STT { get; set; }
        public DateTime CreatedTime { get; set; }
        public string RoomName { get; set; }
        public string Username { get; set; }
        public int PrizeID { get; set; }
        public int PrizeValue { get; set; }
        public string PrizeName { get; set; }
    }
    /// <summary>
    /// Lịch sử vé
    /// Người tạo: vinh.vu
    /// </summary>
    [Serializable]
    public class HistoryVe
    {
        public DateTime CreatedTime { get; set; }
        public string TicketName { get; set; }
        public string GameName { get; set; }
        public int Quantity { get; set; }
    }
    [Serializable]
    public class RoomNauBanh
    {
        public int GameSessionID { get; set; }
        public int RoomID { get; set; }
        public string QuestionData { get; set; }
        public int CardID { get; set; }
        public int CardValue { get; set; }
        public int Status { get; set; }
    }
    /// <summary>
    /// Thông tin phòng thưởng tết.
    /// </summary>
    [Serializable]
    public class E3_ResultInfor
    {
        public int GameSessionID { get; set; }
        public int RoomID { get; set; }
        public int CardID { get; set; }
        public int CardValue { get; set; }
        public int Status { get; set; }
    }
}
 