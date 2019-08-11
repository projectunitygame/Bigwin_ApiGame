using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class SlotMachineHistoryData
    {
        public long SessionID { get; set; }
        public long ParentSessionID { get; set; }
        public long AccountID { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public int BetType { get; set; }
        public long BetValue { get; set; }
        public long TotalBetValue { get; set; }
        public long TotalPrizeValue { get; set; }
        public long RefundValue { get; set; }
        public string Description { get; set; }
        public int ResponseStatus { get; set; }
    }

    public class SlotMachineHistoryDetailData : SlotMachineHistoryData
    {
        public int TotalLines { get; set; }
        public long TotalPayLineValue { get; set; }
        public long TotalJackpotValue { get; set; }
        public long TotalBonusValue { get; set; }
        public long TotalX2Value { get; set; }
        public long TotalFreeSpinValue { get; set; }
        public int TotalFreeSpins { get; set; }
        public string SlotsData { get; set; }
        public string PrizesData { get; set; }
    }

    public class SlotDiamondDetailSpin
    {
        public long SpinID { get; set; }
        public int LineID { get; set; }
        public int Multiplier { get; set; }
        public long PrizeValue { get; set; }
        public int PrizeID { get; set; }
    }

    public class ReSultSlotDiamondDetailSpin
    {
        public List<SlotDiamondDetailSpin> DetailSpin { get; set; }
        public string LineData { get; set; }
    }
}
