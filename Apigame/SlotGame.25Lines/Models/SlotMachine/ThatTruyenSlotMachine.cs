using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Log;

namespace SlotGame._25Lines.Models.SlotMachine
{
    public class ThatTruyenSlotMachine : ISlotMachine
    {
        public IEnumerable<PrizeLine> GetLinesPrize(int[] slotsData, int betValue, string lineData, out bool isJackpot, out int payLineValue)
        {
            isJackpot = false;
            var lines = Array.ConvertAll(lineData.Split(','), int.Parse);
            var prizeLines = new List<PrizeLine>();
            payLineValue = 0;

            foreach (var line in lines) // Duyệt qua các lines kiểm tra trúng thưởng
            {
                var prizeId = 0;
                var linePosition = ThatTruyenLinesAndPrizes.Instance.GetLine(line).Slots;
                var startSymbol = slotsData[linePosition[0]];
                if (startSymbol == 2 || startSymbol == 3)
                    continue;
                var countSymbol = 1;
                for (var i = 1; i < 5; i++)
                {
                    var currentSymbol = slotsData[linePosition[i]];
                    if (startSymbol == 1)
                    {
                        if (currentSymbol != startSymbol) // Neu la bieu tuong khac wild
                        {
                            if(currentSymbol == 2 || currentSymbol == 3)
                                break;
                            //kiem tra giai nhieu wild va 1 giai khac
                            if (countSymbol > 1) // giai wild
                            {
                                prizeId = GetPrizeId(startSymbol, countSymbol, out var localJackpot);
                                if (!isJackpot)
                                    isJackpot = localJackpot;
                            }

                            startSymbol = currentSymbol;
                            countSymbol++;
                        }
                        else
                        {
                            countSymbol++;
                        }
                    }
                    else
                    {
                        if (currentSymbol == 1)
                        {
                            countSymbol++;
                        }                           
                        else if (currentSymbol == startSymbol)
                            countSymbol++;
                        else
                            break;
                    }
                }

                if (countSymbol < 2)
                    continue;

                if (prizeId > 0) // check special condition
                {
                    var secondPrize = GetPrizeId(startSymbol, countSymbol, out var localJackpot2); //  other prize
                    if (!isJackpot)
                        isJackpot = localJackpot2;
                    var firstPrize = prizeId;
                    var firstMutil = ThatTruyenLinesAndPrizes.Instance.GetMultiById(firstPrize);
                    var secondMutil = ThatTruyenLinesAndPrizes.Instance.GetMultiById(secondPrize);
                    prizeId = firstMutil > secondMutil ? firstPrize : secondPrize;

                }
                else
                {
                    prizeId = GetPrizeId(startSymbol, countSymbol, out var localJackpot3);
                    if (!isJackpot)
                        isJackpot = localJackpot3;
                }
    
                

                if (prizeId <= 0) continue;
                {
                    var multi = ThatTruyenLinesAndPrizes.Instance.GetMultiById(prizeId);
                    if (prizeId == 4)
                        multi = 0;
                    var prizeValue = betValue * multi;
                    payLineValue += prizeValue;
                    var positions = new int[countSymbol];

                    for (var i = 0; i < positions.Length; i++)
                    {
                        positions[i] = linePosition[i];
                    }

                    var prizeLine = new PrizeLine()
                    {
                        LineId = line,
                        PrizeId = prizeId,
                        PrizeValue = prizeValue,
                        Position = positions
                    };

                    prizeLines.Add(prizeLine); // add dong trung
                }
            }

            return prizeLines;
        }

        int GetPrizeId(int startSymbol, int countSymbol, out bool isJackpot)
        {
            var prizeId = 0;
            isJackpot = false;
            if (startSymbol == 1) // Wild
            {
                if (countSymbol == 5)
                    prizeId = 1;
                else if (countSymbol == 4)
                {
                    prizeId = 2;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 3;
                }
                else if (countSymbol == 2)
                {
                    prizeId = 28;
                }
            }
            else if (startSymbol == 4) // A
            {
                if (countSymbol == 5)
                {
                    prizeId = 4;
                    isJackpot = true;
                }
                else if (countSymbol == 4)
                    prizeId = 5;
                else if (countSymbol == 3)
                    prizeId = 6;
                else if (countSymbol == 2)
                    prizeId = 29;
            }
            else if (startSymbol == 5) // B
            {
                if (countSymbol == 5)
                    prizeId = 7;
                else if (countSymbol == 4)
                {
                    prizeId = 8;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 9;
                }
            }
            else if (startSymbol == 6) // C
            {
                if (countSymbol == 5)
                    prizeId = 10;
                else if (countSymbol == 4)
                {
                    prizeId = 11;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 12;
                }
            }
            else if (startSymbol == 7) //D
            {
                if (countSymbol == 5)
                    prizeId = 13;
                else if (countSymbol == 4)
                {
                    prizeId = 14;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 15;
                }
            }
            else if (startSymbol == 8) // E
            {
                if (countSymbol == 5)
                    prizeId = 16;
                else if (countSymbol == 4)
                {
                    prizeId = 17;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 18;
                }
            }
            else if (startSymbol == 9)// F
            {
                if (countSymbol == 5)
                    prizeId = 19;
                else if (countSymbol == 4)
                {
                    prizeId = 20;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 21;
                }
            }
            else if (startSymbol == 10) // G
            {
                if (countSymbol == 5)
                    prizeId = 22;
                else if (countSymbol == 4)
                {
                    prizeId = 23;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 24;
                }
            }
            else if (startSymbol == 11) // H
            {
                if (countSymbol == 5)
                    prizeId = 25;
                else if (countSymbol == 4)
                {
                    prizeId = 26;
                }
                else if (countSymbol == 3)
                {
                    prizeId = 27;
                }
            }

            return prizeId;
        }

    }
}