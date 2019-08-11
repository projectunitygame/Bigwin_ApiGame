using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SlotGame._20lines.Game2.Database.DTO
{
    public class SpinData
    {
        public int AccountID { get; set; }

        public long SpinID { get; set; }

        public List<int> SlotsData { get; set; }

        public List<PrizeLine> PrizesData { get; set; }

        public long TotalBetValue { get; set; }

        public long TotalPrizeValue { get; set; }

        public bool IsJackpot { get; set; }

        public long Jackpot { get; set; }

        public long Balance { get; set; }

        public int ResponseStatus { get; set; }

        public int TotalFreeSpin { get; set; }
        [JsonIgnore]
        public long PrizeFund { get; set; } //quy

        public BonusGame BonusGame { get; set; }

        public int PrizeValueFreeSpins { get; set; }

        public static List<int> SetSlots(string _SlotsData)
        {
            if (string.IsNullOrEmpty(_SlotsData))
                return new List<int>();

            string[] SlotItems = _SlotsData.Split(',');
            if (SlotItems.Length > 0)
            {
                var slotsData = new List<int>();
                foreach (string SlotItem in SlotItems)
                {
                    int SymbolID = 0;
                    int.TryParse(SlotItem.Trim(), out SymbolID);
                    if (SymbolID > 0)
                        slotsData.Add(SymbolID);
                }
                return slotsData;
            }
            return new List<int>();
        }

        public static List<PrizeLine> SetPrizeLines(string _PrizesData, string _PositionData = "")
        {
            if (string.IsNullOrEmpty(_PrizesData))
                return new List<PrizeLine>();

            string[] PrizeLinesData = _PrizesData.Split(';');
            string[] PositionLinesData = _PositionData.Split(';');
            if (PrizeLinesData.Length > 0)
            {
                var prizesData = new List<PrizeLine>();
                for (int i = 0, length = PrizeLinesData.Length; i < length; i++)
                {
                    string PrizeLineData = PrizeLinesData[i];
                    string PositionLineData = PositionLinesData[i];
                    if (string.IsNullOrEmpty(PrizeLineData) || string.IsNullOrEmpty(PositionLineData))
                        continue;

                    PrizeLine prizeLine = new PrizeLine(PrizeLineData, PositionLineData);
                    prizesData.Add(prizeLine);
                }
                return prizesData;
            }
            return new List<PrizeLine>();
        }


    }


    public class PrizeLine
    {
        public int LineID;
        public int PrizeID;
        public long PrizeValue;
        public List<int> Items;

        public PrizeLine(string Data, string Position = "")
        {
            if (string.IsNullOrEmpty(Data))
                return;

            string[] ItemValues = Data.Split(',');
            if (ItemValues.Length > 2)
            {
                int.TryParse(ItemValues[0].Trim(), out LineID);
                int.TryParse(ItemValues[1].Trim(), out PrizeID);
                long.TryParse(ItemValues[2].Trim(), out PrizeValue);
            }
            if (!string.IsNullOrEmpty(Position))
            {
                string[] PositionValues = Position.Split(',');
                if (PositionValues.Length > 0)
                {
                    Items = new List<int>();
                    foreach (string PositionValue in PositionValues)
                    {
                        int.TryParse(PositionValue, out int postion);
                        Items.Add(postion);
                    }
                }
            }
        }
    }
}
