using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SlotGame._25Lines.Models.Services
{
    public interface IGenerateBonusData
    {
        BonusGame GenerateBonusGame(int totalBet, int startBonus);
    }

    public class GenerateBonusData : IGenerateBonusData
    {
        IEnumerable<BonusStep> Steps;
        public GenerateBonusData()
        {
            Steps = new List<BonusStep>()
            {
                new BonusStep() {StepId = 1, Items = new float[12] {
                    0.5f, 0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.5f, 1.5f, 2.0f, 2.0f
                }},
                new BonusStep() {StepId = 2, Items = new float[11]
                {
                    0.0f, 0.5f, 0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.0f, 1.5f, 1.5f, 2.0f
                }},
                new BonusStep() {StepId = 3, Items = new float[10]
                {
                    0.0f, 0.0f, 0.5f, 0.5f, 0.5f, 1.0f, 1.0f, 1.5f, 1.5f, 2.0f
                }},
                new BonusStep() {StepId = 4, Items = new float[9]
                {
                    0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 1.0f, 1.0f, 1.5f, 2.0f
                }},
                new BonusStep() {StepId = 5, Items = new float[8]
                {
                    0.0f, 0.0f, 0.0f, 0.5f, 0.5f, 1.0f, 1.5f, 2.0f
                }},
                new BonusStep() {StepId = 6, Items = new float[7]
                {
                    0.0f, 0.0f, 0.0f, 0.5f, 1.0f, 1.5f, 2.0f
                }},
                new BonusStep() {StepId = 7, Items = new float[6]
                {
                    0.0f, 0.0f, 0.0f, 0.5f, 1.0f, 1.5f
                }},
                new BonusStep() {StepId = 8, Items = new float[5]
                {
                    0.0f, 0.0f, 0.0f, 0.5f, 2.0f
                }},
                new BonusStep() {StepId = 9, Items = new float[4]
                {
                    0.0f, 0.0f, 0.0f, 1.0f
                }},
                new BonusStep() {StepId = 10, Items = new float[3]
                {
                    0.0f, 0.0f, 1.5f
                }},
                new BonusStep() {StepId = 11, Items = new float[2]
                {
                    0.0f, 0.5f
                }},
                new BonusStep() {StepId = 12, Items = new float[1]
                {
                    0.0f
                }},
            };
        }
        public BonusGame GenerateBonusGame(int totalBet, int startBonus)
        {
            var numOfItems = 12;
            var bonusData = string.Empty;
            var totalPrizeValue = 0;
            var totalMultiplier = 0.0f;
            foreach (var bonusStep in Steps)
            {
                var rnd = RandomUtil.NextInt(numOfItems);
                var prizeValue = Convert.ToInt32(bonusStep.Items[rnd] * totalBet);
                bonusData += $"{bonusStep.StepId},{prizeValue};";
                totalPrizeValue += prizeValue;
                totalMultiplier += bonusStep.Items[rnd];
                numOfItems--;

                if(prizeValue == 0)
                    break;
            }

            bonusData = bonusData.Substring(0, bonusData.Length - 1);

            //var multi = RandomUtil.NextInt(startBonus, startBonus + 3); // he so nhan tong
            var multi = GetRandomNumber(startBonus, startBonus + 2); // he so nhan tong

            return new BonusGame(){BonusData = bonusData, TotalPrizeValue = (int)totalPrizeValue * multi, Mutiplier = multi, DataMultiplier = totalMultiplier};
        }

        private static readonly object syncLock = new object();
        private static readonly Random getrandom = new Random();
        public static int GetRandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                return getrandom.Next(min, max);
            }
        }
    }
}
