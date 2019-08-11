using System;
using System.Collections.Generic;

namespace SlotGame._20lines.Game2.Database.DTO
{
    public class BonusGame
    {

        public string GoldMinerData { get; set; }

        /// <summary>
        /// Hệ số nhân khởi điểm
        /// </summary>
        public int StartBonus { get; set; }

        /// <summary>
        /// Tổng tiền thắng Bonus
        /// </summary>
        public long PrizeValue { get; set; }

    }
}
