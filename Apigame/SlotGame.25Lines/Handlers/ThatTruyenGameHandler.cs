using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SlotGame._25Lines.Database.DAO;
using SlotGame._25Lines.Database.DAOImpl;
using SlotGame._25Lines.Database.DTO;
using SlotGame._25Lines.Hubs;
using SlotGame._25Lines.Models;
using SlotGame._25Lines.Models.Services;
using SlotGame._25Lines.Models.SlotMachine;
using Utilities;
using Utilities.Cache;

namespace SlotGame._25Lines.Handlers
{
    public class ThatTruyenGameHandler : IGameHandler
    {
        //local var
        private static readonly Lazy<ThatTruyenGameHandler> _instance = new Lazy<ThatTruyenGameHandler>(() => new ThatTruyenGameHandler(new SlotMachine25LinesImpl(), new ThatTruyenSlotData(), new GenerateBonusData()));
        private readonly ISlotMachine25Lines _dbService;
        private readonly IGenerateSlotData _slotsGeneService;
        private readonly IGenerateBonusData _bonusGeneService;
        private readonly ITestService _testService;

        private readonly ConcurrentDictionary<long, long> _jackpotGoldList;
        private readonly ConcurrentDictionary<long, long> _jackpotCoinList;
        private Timer _jackpotTimer;
        public static int[][] _missData =
        {
            new int[]{8,11,9,10,10,7,8,11,7,1,4,10,11,4,9},
            new int[]{2,11,10,9,8,9,10,9,5,7,5,11,11,8,11},
            new int[]{5,7,10,6,8,8,2,9,1,6,6,11,11,10,9},
            new int[]{9,1,8,10,6,11,7,3,4,8,5,2,8,9,9},
            new int[]{8,10,9,5,10,2,11,8,6,3,9,6,11,1,11},
            new int[]{10,8,7,7,8,11,6,5,9,9,8,4,9,5,11},
            new int[]{3,5,11,10,5,6,9,11,7,10,11,7,9,9,1},           
            new int[]{9,10,9,7,9,8,11,8,9,11,7,5,1,5,2},
            new int[]{6,4,11,8,8,9,10,7,6,7,10,11,4,10,5},
            new int[]{5,10,6,4,11,10,11,7,8,11,11,8,9,6,7},
            new int[]{2,6,8,6,11,9,1,11,8,6,5,7,4,9,8},
            new int[]{11,8,7,10,8,8,6,5,5,7,2,4,9,6,5},
            new int[]{5,11,6,5,5,8,10,7,8,11,6,11,9,10,10},
            new int[]{5,9,8,8,8,10,1,3,6,7,11,10,8,10,5},
            new int[]{6,7,9,10,10,9,11,8,7,5,8,4,1,9,11},
            new int[]{3,11,9,6,7,6,5,11,11,11,11,6,10,10,5},
            new int[]{7,11,8,9,5,4,5,11,5,9,9,6,4,10,8},
            new int[]{8,10,9,5,10,2,11,8,6,3,9,6,11,1,11},
            new int[]{6,11,5,9,2,9,8,9,5,8,10,3,11,10,7},
            new int[]{5,6,11,5,3,8,4,4,10,11,6,7,10,7,11},
            new int[]{10,9,4,11,11,11,7,8,6,6,3,11,3,11,8},
            new int[]{9,10,9,7,9,8,11,8,9,11,7,5,1,5,2},
            new int[]{11,5,8,9,5,10,6,3,5,11,9,1,8,8,10},
            new int[]{4,9,10,10,10,9,7,6,7,5,10,11,7,4,11},
            new int[]{ 10, 7, 4, 6, 3, 11, 2, 10, 1, 11, 3, 11, 6, 10, 11 },
            new int[]{8,11,8,10,10,7,5,11,5,6,4,6,4,6,11},
            new int[]{8,4,8,7,11,6,10,1,9,7,9,11,10,5,10},
            new int[]{3,11,7,10,10,6,8,9,7,5,11,3,8,9,11},
            new int[]{5,11,4,3,10,8,10,10,11,6,6,11,6,6,11},
            new int[]{9,6,11,9,8,10,10,8,8,7,1,5,11,11,11},
            new int[]{2,5,4,6,6,9,6,8,8,9,5,1,3,9,11},
            new int[]{2,6,11,2,11,9,10,7,10,5,5,5,4,7,10},
            new int[]{7,3,9,6,11,4,10,11,8,5,9,11,8,9,10},
            new int[]{2,11,10,5,11,9,8,7,6,2,5,10,5,1,8},
            new int[]{9,8,11,4,1,8,3,2,8,9,7,10,9,6,7},
            new int[]{11,9,11,4,9,5,6,7,8,11,8,10,4,6,2},
            new int[]{11,7,7,8,7,6,2,9,10,10,9,11,8,4,6},
            new int[]{11,11,3,9,5,10,4,8,2,10,9,10,7,10,1},
            new int[]{ 9, 7, 10, 7, 11, 8, 11, 11, 9, 2, 5, 10, 9, 5, 8 },
            new int[]{11,8,7,6,5,6,3,10,11,11,9,10,9,10,10},
            new int[]{10,8,8,8,7,11,3,11,9,10,3,10,4,2,5},
            new int[]{2,11,9,8,9,9,8,11,9,7,5,3,10,2,10},
            new int[]{8,6,11,6,8,2,4,2,1,7,9,7,9,10,5},
            new int[]{9,11,10,7,7,8,5,11,4,10,5,6,7,8,6},
            new int[]{11,9,11,4,7,5,6,8,8,5,8,10,11,6,9},
            new int[]{10,7,7,6,5,9,2,9,11,11,11,11,8,10,10},
            new int[]{11,11,3,9,7,8,6,8,8,10,2,9,7,11,5},
            new int[]{8,10,11,7,9,7,11,11,9,8,4,6,9,5,10},
            new int[]{11,11,8,7,7,6,4,3,9,11,9,10,8,5,5},
            new int[]{2,7,11,8,5,9,11,7,11,9,5,4,4,3,8},
            new int[]{5,8,11,11,6,10,6,4,6,9,11,4,10,11,11},
            new int[]{7,6,9,5,11,4,10,10,8,5,9,5,11,10,10},
            new int[]{ 6, 10, 11, 8, 8, 9, 11, 7, 10, 7, 8, 8, 4, 4, 5 },
            new int[]{5,6,8,1,9,8,4,11,10,11,6,7,4,7,2},
            new int[]{2,10,8,6,8,9,8,7,10,10,5,6,10,7,4},
            new int[]{5,1,7,8,4,8,7,10,11,11,6,2,9,3,6},
            new int[]{11,4,9,8,11,3,10,11,11,10,6,11,8,3,8},
            new int[]{9,11,8,9,8,8,4,1,2,10,5,10,10,10,4},
            new int[]{8,3,11,5,10,7,10,8,6,1,4,11,11,1,9},
            new int[]{8,11,9,6,3,5,4,11,10,11,10,10,11,7,11},
            new int[]{9,10,10,10,10,8,5,7,7,8,7,9,5,9,6},
            new int[]{8,7,7,1,6,5,11,4,10,9,10,4,8,7,11},
            new int[]{5,11,6,8,7,10,5,7,10,10,11,6,9,4,6},
            new int[]{5,9,10,9,8,8,6,11,6,9,6,10,9,8,11},
            new int[]{10,4,4,10,11,9,7,8,7,10,11,11,3,9,8},
            new int[]{8,11,9,7,8,7,6,11,9,6,4,9,8,8,9},
            new int[]{ 11, 1, 4, 9, 10, 6, 7, 8, 2, 8, 9, 2, 3, 10, 6 },
            new int[]{9,4,8,5,11,8,10,1,6,6,7,11,10,1,8},
            new int[]{8,10,11,7,9,6,5,8,9,11,9,9,11,5,8},
            new int[]{3,10,11,10,8,6,5,7,7,7,11,9,4,4,11},
            new int[]{8,7,6,3,10,2,11,7,11,6,9,10,9,6,11},
            new int[]{8,4,10,8,8,2,10,11,6,7,9,11,9,10,5},
            new int[]{9,11,6,11,8,5,10,7,10,9,10,11,9,5,11},
            new int[]{6,8,11,4,5,9,3,4,8,11,8,10,10,6,10},
            new int[]{9,2,9,8,6,8,11,11,9,9,5,8,11,2,11},
            new int[]{7,6,9,7,10,4,10,8,9,1,9,5,11,8,9},
            new int[]{2,11,10,8,8,9,4,9,11,6,5,10,11,3,9},
            new int[]{8,2,9,7,8,7,11,11,4,7,4,8,11,8,5},
            new int[]{5,11,11,7,7,8,6,2,9,5,6,9,9,5,9},
            new int[]{11,6,8,5,8,6,10,11,10,7,9,5,2,7,11},
            new int[]{ 5, 4, 10, 10, 5, 10, 7, 6, 4, 11, 11, 11, 7, 9, 10 },
            new int[]{8,7,10,11,1,2,11,9,3,9,9,4,11,11,7},
            new int[]{8,10,9,9,7,7,11,11,5,5,4,6,8,8,9},
            new int[]{7,11,8,1,3,4,6,7,10,11,9,9,10,7,11},
            new int[]{11,5,11,4,11,3,9,11,9,8,6,7,9,6,7},
            new int[]{6,10,9,7,8,11,8,11,9,10,1,6,11,5,4},
            new int[]{8,4,6,6,9,6,10,7,10,11,9,11,9,7,8},
            new int[]{7,6,11,8,6,4,10,10,6,9,9,5,7,10,11},
            new int[]{10,7,8,8,11,1,11,3,9,5,11,10,8,2,10},
            new int[]{11,10,10,9,11,3,5,11,2,5,6,9,9,10,10},
            new int[]{2,3,4,2,11,9,10,10,10,7,5,11,6,7,10},
            new int[]{3,4,7,10,1,6,10,4,7,9,11,11,8,9,7},
            new int[]{4,10,2,5,11,9,11,9,10,10,10,8,11,7,8},
            new int[]{ 8, 6, 4, 10, 11, 2, 10, 10, 5, 7, 9, 5, 6, 6, 10 }
        };
        private IHubConnectionContext<dynamic> Clients;
        // class instance
        public static ThatTruyenGameHandler Instance => _instance.Value;

        private ThatTruyenGameHandler(ISlotMachine25Lines dbService, IGenerateSlotData slotsGeneService, IGenerateBonusData bonusGeneService)
        {
            _dbService = dbService;
            _slotsGeneService = slotsGeneService;
            _bonusGeneService = bonusGeneService;

            _testService = new TestService();
            Clients = GlobalHost.ConnectionManager.GetHubContext<ThatTruyenHub>().Clients;

            //jackpot
            _jackpotGoldList = new ConcurrentDictionary<long, long>();
            _jackpotCoinList = new ConcurrentDictionary<long, long>();
            _jackpotTimer = new Timer(UpdateJackpot, null, 0, 5000);
        }


        public AccountInfo GetAccountInfo(long accountId, string accountName, int roomId, MoneyType moneyType, out int response)
        {
            return _dbService.GetAccountInfo(accountId, accountName, roomId, moneyType, out response);
        }

        public SpinResult Spin(long accountId, string accountName, int roomId, MoneyType moneyType, string lineData)
        {
            var roomValue = GetBetValueByRoom(roomId);
            var totalBetValue = lineData.Split(',').Length * roomValue;
            var slotsData = _testService.IsTestAccount(accountName) ? _testService.GetTestData() : _slotsGeneService.GenerateSlotsData();

            //var newSlotsData = _slotsGeneService.HandleSlotsData(slotsData);
            var slotMachine = new ThatTruyenSlotMachine();
            var prizeLines = slotMachine.GetLinesPrize(slotsData, roomValue, lineData, out var isJackpot, out var payLinePrizeValue);
            var countBonus = slotsData.Count(x => x == 3); // Đếm biểu tượng bonus
            var countScatter = slotsData.Count(x => x == 2); // đếm biểu tượng freeSpins
            //if (payLinePrizeValue == 0 && countBonus < 3 && countScatter < 3 && !isJackpot)
            //{
            //    if (CacheHandler.AddAccountAction(accountName, "MissSpin", 300) > 10)
            //    {
            //        do
            //        {
            //            slotsData = _slotsGeneService.GenerateSlotsData();
            //            prizeLines = slotMachine.GetLinesPrize(slotsData, roomValue, lineData, out isJackpot, out payLinePrizeValue);
            //            countBonus = slotsData.Count(x => x == 3); // Đếm biểu tượng bonus
            //            countScatter = slotsData.Count(x => x == 2); // đếm biểu tượng freeSpins 
            //        } while (payLinePrizeValue == 0 && countBonus < 3 && countScatter < 3 && !isJackpot);
            //    }
            //}
            //else
            //{
            //    CacheHandler.RemoveAccountAction(accountName, "MissSpin");
            //}
           
            BonusGame bonusGame = null;
            // Tạo Bonus Game nếu có
            if (countBonus >= 3)
            {
                bonusGame = _bonusGeneService.GenerateBonusGame(totalBetValue, countBonus - 2);

            }

            var addFreeSpins = 0;
            if (countScatter >= 3)
            {
                addFreeSpins = (countScatter - 2) * 4;  // Thêm lượt FreeSpins
            }
            var inputSpinData = new InputSpinData()
            {
                AccountId = accountId,
                AccountName = accountName,
                MoneyType = moneyType,
                AddFreeSpins = addFreeSpins,
                RoomId = roomId,
                IsJackpot = isJackpot,
                SlotsData = string.Join(",", slotsData.Select(x => x.ToString())),
                TotalBonusValue = countBonus >= 3 ? bonusGame.TotalPrizeValue : 0,
                TotalPrizeValue = payLinePrizeValue,
                LineData = lineData,
                TotalBetValue = totalBetValue
            };
            var outputSpinData = _dbService.Spin(inputSpinData);
            if (outputSpinData.ResponseStatus < 0)
            {
                if (outputSpinData.ResponseStatus == -90) // Limit Fund
                {

                    var missData = _missData[RandomUtil.NextInt(0, _missData.Length)];
                    _dbService.UpdateSlotsData(outputSpinData.SpinId, string.Join(",", missData.Select(x => x.ToString()))); // cap nhat kq tap truot
                    return new SpinResult()
                    {
                        SpinId = outputSpinData.SpinId,
                        SlotsData = missData,
                        AddFreeSpin = 0,
                        PrizeLines = new List<PrizeLine>(),
                        Balance = outputSpinData.Balance,
                        FreeSpins = outputSpinData.FreeSpins,
                        Jackpot = outputSpinData.Jackpot,
                        ResponseStatus = 1
                    };
                }
                return new SpinResult()
                {
                    ResponseStatus = outputSpinData.ResponseStatus
                };
            }

            if (countBonus >= 3)
                _dbService.CreateBonusGame(moneyType, outputSpinData.SpinId, roomId, accountId, accountName, totalBetValue, bonusGame.BonusData, bonusGame.Mutiplier, bonusGame.TotalPrizeValue, out var bonusResponse);

            UpdateCacheJackpot(roomId, outputSpinData.Jackpot, moneyType); // Cập nhật jackpot cho cache
            var totalPrizeValue = payLinePrizeValue + outputSpinData.TotalJackpotValue;
            HonorHandler.Instance.SaveHonor(accountName, roomId, totalPrizeValue, inputSpinData.IsJackpot ? 1 : 2); // Luu vinh danh

            return new SpinResult()
            {
                SpinId = outputSpinData.SpinId,
                SlotsData = slotsData,
                AddFreeSpin = addFreeSpins,
                IsJackpot = isJackpot,
                PrizeLines = prizeLines,
                BonusGame = bonusGame,
                TotalPrizeValue = payLinePrizeValue + outputSpinData.TotalJackpotValue,
                TotalPaylinePrizeValue = payLinePrizeValue,
                TotalJackpotValue = outputSpinData.TotalJackpotValue,
                Balance = outputSpinData.Balance,
                FreeSpins = outputSpinData.FreeSpins,
                Jackpot = outputSpinData.Jackpot,
                ResponseStatus = outputSpinData.ResponseStatus
            };


        }

        public int FinishBonusGame(MoneyType moneyType, int spinId, out int prizeValue, out long balance)
        {
            _dbService.FinishBonusGame(moneyType, spinId, out prizeValue, out balance, out var response);
            return response;
        }

        private int GetBetValueByRoom(int roomId)
        {
            switch (roomId)
            {
                case 1:
                    return 100;
                case 2:
                    return 1000;
                case 3:
                    return 5000;
                default:
                    return 10000;
            }
        }

        public string GetJackpot(MoneyType moneyType)
        {
            if (_jackpotGoldList.Count == 0)
                UpdateJackpot(null);
            return moneyType == MoneyType.Gold ? JsonConvert.SerializeObject(_jackpotGoldList) : JsonConvert.SerializeObject(_jackpotCoinList);
        }

        public LuckyGame PlayLuckyGame(MoneyType moneyType, long accountId, string accountName, int roomId, X2Game step, int spinId)
        {
            var result = RandomUtil.NextInt(2);
            return _dbService.PlayLuckyGame(moneyType, accountId, accountName, roomId, spinId, step, result);
        }

        private void UpdateCacheJackpot(int roomId, long jackpotValue, MoneyType moneyType)
        {

            if (moneyType == MoneyType.Gold)
            {
                _jackpotGoldList.AddOrUpdate((long)roomId, jackpotValue, (key, value) => value);
            }
            else
            {
                _jackpotCoinList.AddOrUpdate((long)roomId, jackpotValue, (key, value) => value);
            }

        }

        void UpdateJackpot(object obj)
        {
            var goldJackpots = _dbService.GetJackpot(MoneyType.Gold);
            foreach (var jp in goldJackpots)
            {
                _jackpotGoldList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            var coinJackpots = _dbService.GetJackpot(MoneyType.Coin);
            foreach (var jp in coinJackpots)
            {
                _jackpotCoinList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            Clients.Group("Gold").UpdateJackpot(GetJackpot(MoneyType.Gold));
            Clients.Group("Coin").UpdateJackpot(GetJackpot(MoneyType.Coin));
        }
    }
}