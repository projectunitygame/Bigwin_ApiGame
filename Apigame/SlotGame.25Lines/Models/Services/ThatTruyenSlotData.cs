using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities;

namespace SlotGame._25Lines.Models.Services
{
    public class ThatTruyenSlotData : IGenerateSlotData
    {
        private readonly IEnumerable<Reel> _reels;
        // 1 - Wild, 2 Scatter, 3 Bonus, 4A, 5B, 6C, 7D, 8E, 9F, 10G, 11H

        public ThatTruyenSlotData()
        {
            _reels = new List<Reel>()
            {
                new Reel(){ReelId = 1, ReelData = new int[36] // Reel 1
                {
                    4,9,10,1,9,8,5,10,11,3,6,11,1,10,11,8,2,9,5,10,1,11,10,9,11,5,8,6,9,10,1,11,6,9,8,7
                }},

                new Reel(){ReelId = 2, ReelData = new int[36] // Reel 2
                {
                    7,2,11,8,3,10,11,6,9,1,10,11,8,10,8,6,4,7,11,10,11,5,1,9,6,10,5,9,7,11,4,10,11,5,6,1
               }},

                new Reel() {ReelId = 3, ReelData = new int[35] // Reel 3
                {
                    3,8,7,10,9,11,8,11,2,9,11,10,7,5,9,11,11,9,8,11,4,10,6,7,9,8,1,10,11,9,10,11,7,4,8

                }},

                new Reel(){ReelId = 4, ReelData = new int[34] // Reel 4
                {
                    3,11,6,11,10,5,6,1,10,7,9,5,10,7,4,8,6,10,7,9,5,8,10,4,9,6,8,9,2,10,7,9,8,11

                }},

                new Reel() {ReelId = 5, ReelData = new int[38] //Reel 5
                {
                    10,3,11,11,7,10,6,11,5,10,1,9,7,10,5,11,10,8,6,9,11,8,7,5,9,8,10,4,11,6,8,9,11,2,8,7,11,5

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
            return slotsData;
        }
    }
}