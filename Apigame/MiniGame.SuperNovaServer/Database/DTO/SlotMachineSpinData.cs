using System.Collections.Generic;
using Newtonsoft.Json;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class SlotMachineSpinData
    {
        public int AccountID { get; set; }

        public long SpinID { get; set; }

        public string SlotsData { get; set; }

        public List<PrizeLine> PrizesData { get; set; }

        public long TotalBetValue { get; set; }

        public long TotalPrizeValue { get; set; }

        public long PaylinePrizeValue { get; set; }

        public bool IsJackpot { get; set; }

        public long Jackpot { get; set; }

        public string PositionData { get; set; }

        public long Balance { get; set; }


        public int ResponseStatus { get; set; }
        [JsonIgnore]
        public long PrizeFund { get; set; } //quy

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

        public SlotMachineSpinData()
        {
        }


        public SlotMachineSpinData(string SlotsData, string PrizesData, string PositionData,
            int TotalBetValue, long TotalPrizeValue, long PayLinePrizeValue, long Jackpot, long Balance)
        {
            AccountID = AccountID;
            SpinID = SpinID;
            this.SlotsData = SlotsData;
            this.PrizesData = SetPrizeLines(PrizesData, PositionData);
            this.TotalBetValue = TotalBetValue;
            this.Jackpot = Jackpot;
            IsJackpot = Jackpot > 0;
            this.Balance = Balance;
            this.TotalPrizeValue = TotalPrizeValue;
            this.PaylinePrizeValue = PayLinePrizeValue;
        }
    }


    public class PrizeLine
    {
        public int LineID;
        public int PrizeID;
        public long PrizeValue;
        public Dictionary<int, int> Items; //<SlotPosition, SymbolID>

        public PrizeLine() { }

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
                    Items = new Dictionary<int, int>();
                    foreach (string PositionValue in PositionValues)
                    {
                        int postion = 0;
                        int.TryParse(PositionValue, out postion);
                        if (postion > 0 && !Items.ContainsKey(postion))
                            Items.Add(postion, postion);
                    }
                }
            }
        }
    }
}