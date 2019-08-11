using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MinigameVuabai.SignalR.Models
{
    [Serializable]
    public class SpinsInfo
    {
        public long _SpinID { get; set; }
        public string _SlotsData { get; set; }
        public string _PrizesData { get; set; }
        public int _TotalBetValue { get; set; }
        public long _TotalPrizeValue { get; set; }
        public bool _IsJackpot { get; set; }
        public long _Jackpot { get; set; }
        public long _Balance { get; set; }
        public int _ResponseStatus { get; set; }
        public int IsAutoFreeze { get; set; }
        public string LuckyData { get; set; }
        public int TotalJackPot { get; set; }
    }

}