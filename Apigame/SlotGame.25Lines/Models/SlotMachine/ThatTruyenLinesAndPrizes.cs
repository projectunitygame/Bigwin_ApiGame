using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotGame._25Lines.Models.SlotMachine
{
    public class ThatTruyenLinesAndPrizes
    {
        private static readonly Lazy<ThatTruyenLinesAndPrizes> _instance = new Lazy<ThatTruyenLinesAndPrizes>(() => new ThatTruyenLinesAndPrizes());

        /// <summary>
        /// Lưu trữ 25 dòng 
        /// </summary>
        private IEnumerable<Line> Lines;

        /// <summary>
        /// Lưu trữ thông tin giải
        /// </summary>
        private IEnumerable<Prize> Prizes;

        private ThatTruyenLinesAndPrizes()
        {
            Init();
        }

        public static ThatTruyenLinesAndPrizes Instance => _instance.Value;

        #region Initialization

        private void Init()
        {
            Lines = new List<Line>()
            {
                new Line() {LineId = 1, Slots = new int[5] {5, 6, 7, 8, 9}},
                new Line() {LineId = 2, Slots = new int[5] {0, 1, 2, 3, 4}},
                new Line() {LineId = 3, Slots = new int[5] {10, 11, 12, 13, 14}},
                new Line() {LineId = 4, Slots = new int[5] {10, 6, 2, 8, 14}},
                new Line() {LineId = 5, Slots = new int[5] {0, 6, 12, 8, 4}},
                new Line() {LineId = 6, Slots = new int[5] {5, 1, 2, 3, 9}},
                new Line() {LineId = 7, Slots = new int[5] {5, 11, 12, 13, 9}},
                new Line() {LineId = 8, Slots = new int[5] {0, 1, 7, 13, 14}},
                new Line() {LineId = 9, Slots = new int[5] {10, 11, 7, 3, 4}},
                new Line() {LineId = 10, Slots = new int[5] {5, 11, 7, 3, 9}},
                new Line() {LineId = 11, Slots = new int[5] {5, 1, 7, 13, 9}},
                new Line() {LineId = 12, Slots = new int[5] {0, 6, 7, 8, 4}},
                new Line() {LineId = 13, Slots = new int[5] {10, 6, 7, 8, 14}},
                new Line() {LineId = 14, Slots = new int[5] {0, 6, 2, 8, 4}},
                new Line() {LineId = 15, Slots = new int[5] {10, 6, 12, 8, 14}},
                new Line() {LineId = 16, Slots = new int[5] {5, 6, 2, 8, 9}},
                new Line() {LineId = 17, Slots = new int[5] {5, 6, 12, 8, 9}},
                new Line() {LineId = 18, Slots = new int[5] {0, 1, 12, 3, 4}},
                new Line() {LineId = 19, Slots = new int[5] {10, 11, 2, 13, 14}},
                new Line() {LineId = 20, Slots = new int[5] {0, 11, 12, 13, 4}},
                new Line() {LineId = 21, Slots = new int[5] {10, 1, 2, 3, 14}},
                new Line() {LineId = 22, Slots = new int[5] {5, 11, 2, 13, 9}},
                new Line() {LineId = 23, Slots = new int[5] {5, 1, 12, 3, 9}},
                new Line() {LineId = 24, Slots = new int[5] {0, 11, 2, 13, 4}},
                new Line() {LineId = 25, Slots = new int[5] {10, 1, 12, 3, 14}}
            };

            Prizes = new List<Prize>()
            {
                new Prize() {PrizeId = 1, PrizeName = "5 Wilds", Multiplier = 8000},
                new Prize() {PrizeId = 2, PrizeName = "4 Wilds", Multiplier = 500},
                new Prize() {PrizeId = 3, PrizeName = "3 Wilds", Multiplier = 50},
                new Prize() {PrizeId = 4, PrizeName = "5A", Multiplier = 5000},
                new Prize() {PrizeId = 5, PrizeName = "4A", Multiplier = 150},
                new Prize() {PrizeId = 6, PrizeName = "3A", Multiplier = 40},
                new Prize() {PrizeId = 7, PrizeName = "5B", Multiplier = 400},
                new Prize() {PrizeId = 8, PrizeName = "4B", Multiplier = 100},
                new Prize() {PrizeId = 9, PrizeName = "3B", Multiplier = 35},
                new Prize() {PrizeId = 10, PrizeName = "5C", Multiplier = 300},
                new Prize() {PrizeId = 11, PrizeName = "4C", Multiplier = 90},
                new Prize() {PrizeId = 12, PrizeName = "3C", Multiplier = 30},
                new Prize() {PrizeId = 13, PrizeName = "5D", Multiplier = 150},
                new Prize() {PrizeId = 14, PrizeName = "4D", Multiplier = 75},
                new Prize() {PrizeId = 15, PrizeName = "3D", Multiplier = 25},
                new Prize() {PrizeId = 16, PrizeName = "5E", Multiplier = 100},
                new Prize() {PrizeId = 17, PrizeName = "4E", Multiplier = 60},
                new Prize() {PrizeId = 18, PrizeName = "3E", Multiplier = 20},
                new Prize() {PrizeId = 19, PrizeName = "5F", Multiplier = 75},
                new Prize() {PrizeId = 20, PrizeName = "4F", Multiplier = 40},
                new Prize() {PrizeId = 21, PrizeName = "3F", Multiplier = 10},
                new Prize() {PrizeId = 22, PrizeName = "5G", Multiplier = 45},
                new Prize() {PrizeId = 23, PrizeName = "4G", Multiplier = 20},
                new Prize() {PrizeId = 24, PrizeName = "3G", Multiplier = 6},
                new Prize() {PrizeId = 25, PrizeName = "5H", Multiplier = 30},
                new Prize() {PrizeId = 26, PrizeName = "4H", Multiplier = 10},
                new Prize() {PrizeId = 27, PrizeName = "3H", Multiplier = 3},
                new Prize() {PrizeId = 28, PrizeName = "2W", Multiplier = 4},
                new Prize() {PrizeId = 29, PrizeName = "2A", Multiplier = 2}
            };
        }

        #endregion

        public Line GetLine(int lineId)
        {
            return Lines.FirstOrDefault(x => x.LineId == lineId);
        }

        public int GetMultiById(int prizeId) => Prizes.First(x => x.PrizeId == prizeId).Multiplier;
    }
}