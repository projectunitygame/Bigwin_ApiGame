using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models
{
    public class SpinResult
    {
        public int SpinId { get; set; }
        public int[] SlotsData { get; set; }
        public IEnumerable<PrizeLine> PrizeLines {get;set;} // các dòng trúng thưởng
        public BonusGame BonusGame;
        public int AddFreeSpin { get; set; }
        public int FreeSpins { get; set; } // Số lượt FreeSpins còn lại
        public bool IsJackpot { get; set; } // Có trúng jackpot hay không
        public int TotalPrizeValue { get; set; } // Tổng số tiền thắng của phiên quay bao gồm dòng + jackpot
        public int TotalPaylinePrizeValue { get; set; }

        public int TotalJackpotValue { get; set; } // Số tiền thắng jackpot
        public long Balance { get; set; }
        public int Jackpot { get; set; } // Giá trị jackpot hiện tại
        public int ResponseStatus { get; set; }
    }
}