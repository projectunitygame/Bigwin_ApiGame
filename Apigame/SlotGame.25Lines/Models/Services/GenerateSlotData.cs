using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace SlotGame._25Lines.Models.Services
{
    public interface IGenerateSlotData
    {
        int[] GenerateSlotsData();

        int[] HandleSlotsData(int[] slotsData);
    }

    public class GenerateSlotData : IGenerateSlotData
    {
        private readonly IEnumerable<Reel> _reels;
        // 1 - Wild, 2 Scatter, 3 Bonus, 4A, 5B, 6C, 7D, 8E, 9F, 10G, 11H
  
        public GenerateSlotData()
        {
            _reels = new List<Reel>() // new version 
            {
                new Reel(){ReelId = 1, ReelData = new int[50] // Reel 1
                {
                    9,7,10,5,2,10,7,11,3,5,6,10,6,10,7,8,10,6,11,10,4,8,10,9,4,8,11,4,9,6,11,8,4,10,9,8,10,11,9,7,2,9,11,4,11,11,3,11,9,9
                }},

                new Reel(){ReelId = 2, ReelData = new int[52] // Reel 2
                {
                    10,9,1,10,9,7,11,4,11,7,5,8,6,9,3,11,8,11,4,9,7,1,9,7,10,4,8,11,4,8,10,6,1,10,6,5,2,8,10,4,11,10,2,11,9,4,11,10,3,7,6,9
                }},

                new Reel() {ReelId = 3, ReelData = new int[49] // Reel 3
                {
                    10,8,11,2,9,11,4,10,11,11,10,11,6,7,1,6,7,8,10,9,1,10,9,4,11,11,8,10,1,8,10,9,11,3,5,11,4,11,9,4,9,10,9,1,10,9,8,11,11
                }},

                new Reel(){ReelId = 4, ReelData = new int[48] // Reel 4
                {
                    10,7,1,10,7,11,9,7,11,10,5,11,10,8,10,4,9,8,6,3,11,9,8,4,6,11,9,4,10,6,9,4,10,9,4,11,10,8,9,4,11,10,2,5,9,11,11,2
                }},

                new Reel() {ReelId = 5, ReelData = new int[49] //Reel 5
                {
                    6,11,7,11,2,4,10,6,11,3,9,11,8,4,9,5,8,4,7,9,10,9,10,11,8,7,9,10,8,4,10,8,9,5,11,7,9,8,11,6,10,2,9,10,7,11,10,11,3
                }}
            };
        }

        public int[] GenerateSlotsData()
        {

            var slotsData = new int[15];
            var col = 0;
            foreach (var reel in _reels)
            {
                var reelLength = reel.ReelData.Length;
                var rnd = RandomUtil.NextInt(reelLength);
                slotsData[col] = reel.ReelData[rnd];
                slotsData[col + 5] = reel.ReelData[(rnd + 1) % (reelLength)];
                slotsData[col + 10] = reel.ReelData[(rnd + 2) % (reelLength)];
                col++;
            }

            return slotsData;
        }

        public int[] HandleSlotsData(int[] slotsData)
        {
            var newSlotsData = new int[15];
            newSlotsData[0] = slotsData[0];
            newSlotsData[5] = slotsData[5];
            newSlotsData[10] = slotsData[10];
            for (var i = 1; i < 5; i++)
            {
                if (slotsData[i] == 1 || slotsData[i + 5] == 1 || slotsData[i + 10] == 1)
                {
                    newSlotsData[i] = slotsData[i] == 2 || slotsData[i] == 3 ? slotsData[i] : 1;
                    newSlotsData[i + 5] = slotsData[i + 5] == 2 || slotsData[i + 5] == 3 ? slotsData[i + 5] : 1;
                    newSlotsData[i + 10] = slotsData[i + 10] == 2 || slotsData[i + 10] == 3 ? slotsData[i + 10] : 1;
                }                 
                else
                {
                    newSlotsData[i] = slotsData[i];
                    newSlotsData[i + 5] = slotsData[i + 5];
                    newSlotsData[i + 10] = slotsData[i + 10];
                }
            }

            return newSlotsData;
        }
    }
}
