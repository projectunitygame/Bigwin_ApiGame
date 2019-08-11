using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotGame._20lines.Game2.Database.DTO
{
    public class SlotMachineHistory
    {
        public int Id { get; set; }

        public long SpinId { get; set; }

        public long ParentSpinId { get; set; }

        public long AccountId { get; set; }

        public string UserName { get; set; }

        public int TotalLines { get; set; }

        public string LineData { get; set; }

        public int BetType { get; set; }

        public int BetValue { get; set; }

        public int TotalBetValue { get; set; }

        public long TotalPrizeValue { get; set; }

        public int StartBonus { get; set; }

        public long TotalBonusValue { get; set; }
        public int StatusBonus { get; set; }

        public long TotalJackPortValue { get; set; }

        public string SlotsData { get; set; }

        public string PrizesData { get; set; }

        public string Description { get; set; }

        public int GameStatus { get; set; }

        public int Status { get; set; }
        public bool IsJackport { get; set; }
        public int RoomId { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class SlotMachineHistoryMobile
    {
        public long SpinId { get; set; }
        public int BetType { get; set; }
        public int BetValue { get; set; }
        public int TotalBetValue { get; set; }
        public long TotalPrizeValue { get; set; }
        public string Description { get; set; }
        public bool IsJackport { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class ListHistory
    {
        public long TotalRecord { get; set; }
        public List<SlotMachineHistory> History { get; set; }
    }


    public class ThongPhatHistory
    {
        public long SpinID { get; set; }
        public int RoomID { get; set; }
        public long AccountID { get; set; }
        public string Username { get; set; }
        public string SlotsData { get; set; }
        public long BetValue { get; set; }
        public DateTime CreatedTime { get; set; }
        public int TwinType { get; set; }
        public bool IsFree { get; set; }
        public long TotalPrizeValue { get; set; }
    }
}
