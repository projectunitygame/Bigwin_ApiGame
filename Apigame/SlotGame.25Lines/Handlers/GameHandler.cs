using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
using Utilities.Log;

namespace SlotGame._25Lines.Handlers
{
    public interface IGameHandler
    {
        AccountInfo GetAccountInfo(long accountId, string accountName, int roomId, MoneyType moneyType, out int response);
        SpinResult Spin(long accountId, string accountName, int roomId, MoneyType moneyType, string lineData);
        int FinishBonusGame(MoneyType moneyType, int spinId, out int prizeValue, out long balance);
        string GetJackpot(MoneyType type);

        LuckyGame PlayLuckyGame(MoneyType moneyType, long accountId, string accountName, int roomId, X2Game step, int spinId);
    }

    public class GameHandler : IGameHandler
    {
        //local var
        private static readonly Lazy<GameHandler> _instance = new Lazy<GameHandler>(() => new GameHandler(new SlotMachine25LinesImpl(), new GenerateSlotData(), new GenerateBonusData()));
        private readonly ISlotMachine25Lines _dbService;
        private readonly IGenerateSlotData _slotsGeneService;
        private readonly IGenerateBonusData _bonusGeneService;
        private readonly ITestService _testService;

        private readonly ConcurrentDictionary<long, long> _jackpotGoldList;
        private readonly ConcurrentDictionary<long, long> _jackpotCoinList;
        private Timer _jackpotTimer;

        public static int[][] _missData =
        {
            new int[]{9,11,11,9,7,7,8,11,4,9,2,11,10,10,10},
            new int[]{11,8,8,10,10,4,10,10,8,2,11,6,9,10,9},
            new int[]{7,7,11,2,11,2,11,8,5,10,9,4,10,9,11},
            new int[]{11,2,5,4,9,9,8,11,11,5,7,10,4,10,8},
            new int[]{4,10,8,4,3,10,6,11,10,9,9,1,11,6,11},
            new int[]{7,9,7,11,8,8,4,8,9,11,10,11,10,8,6},
            new int[]{2,8,11,11,11,9,10,11,9,3,11,4,8,8,6},
            new int[]{6,8,9,9,9,11,10,10,4,5,8,4,9,11,11},
            new int[]{8,10,8,5,8,10,3,11,11,9,9,7,11,10,5},
            new int[]{9,6,8,8,3,7,5,10,6,6,2,2,1,3,11},
            new int[]{9,2,4,9,10,11,8,10,11,8,4,10,11,11,9},
            new int[]{2,3,11,6,10,10,11,11,11,2,7,8,10,9,9},
            new int[]{4,4,8,10,7,8,11,10,7,9,11,10,9,11,8},
            new int[]{ 4, 8, 7, 10, 7, 9, 10, 8, 8, 11, 6, 4, 10, 9, 10 },
            new int[]{ 10,5,9,10,10,6,8,8,6,9,11,6,11,9,10},
            new int[]{ 5,4,8,9,3,2,11,10,8,6,10,7,9,6,11 },
            new int[]{ 9,2,11,4,4,4,11,11,10,7,8,9,10,9,9 },
            new int[]{9,8,11,2,7,6,10,6,10,9,11,4,7,7,8},
            new int[]{ 2,6,10,9,7,9,5,1,8,9,11,2,8,4,10 }
        };
        private IHubConnectionContext<dynamic> Clients;
        // class instance
        public static GameHandler Instance => _instance.Value;

        private GameHandler(ISlotMachine25Lines dbService, IGenerateSlotData slotsGeneService, IGenerateBonusData bonusGeneService)
        {
            _dbService = dbService;
            _slotsGeneService = slotsGeneService;
            _bonusGeneService = bonusGeneService;

            _testService = new TestService();
            Clients = GlobalHost.ConnectionManager.GetHubContext<GameHub>().Clients;

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
            String s = "[tamquoc] Spin play: " +
                "\r\naccountId: " + accountId +
                "\r\naccountName: " + accountName +
                "\r\nroomId: " + roomId +
                "\r\nmoneyType: " + moneyType +
                "\r\nlineData: " + lineData;
            var roomValue = GetBetValueByRoom(roomId);
            var totalBetValue = lineData.Split(',').Length * roomValue;
            var slotsData = _testService.IsTestAccount(accountName) ? _testService.GetTestData() : _slotsGeneService.GenerateSlotsData();

            var newSlotsData = _slotsGeneService.HandleSlotsData(slotsData);
            var slotMachine = new SlotMachine();
            var prizeLines = slotMachine.GetLinesPrize(newSlotsData, roomValue, lineData, out var isJackpot, out var payLinePrizeValue);
            var countBonus = slotsData.Count(x => x == 3); // Đếm biểu tượng bonus
            var countScatter = slotsData.Count(x => x == 2); // đếm biểu tượng freeSpins
            BonusGame bonusGame = null;
            // Tạo Bonus Game nếu có
            if (countBonus >= 3)
            {
                bonusGame = _bonusGeneService.GenerateBonusGame(totalBetValue, countBonus - 2);
                s += "\r\nWin bonus: " + JsonConvert.SerializeObject(bonusGame);
            }

            var addFreeSpins = 0;
            if (countScatter >= 3)
            {
                addFreeSpins = (countScatter - 2) * 4;  // Thêm lượt FreeSpins
                s += "\r\nWin Free spin: " + addFreeSpins;
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
                TotalBonusValue = countBonus > 3 ? bonusGame.TotalPrizeValue : 0,
                TotalPrizeValue = payLinePrizeValue,
                LineData = lineData,
                TotalBetValue = totalBetValue
            };
            var outputSpinData = _dbService.Spin(inputSpinData);
            if (outputSpinData.ResponseStatus < 0)
            {
                if (outputSpinData.ResponseStatus == -90) // Limit Fund
                {
                    return new SpinResult()
                    {
                        SpinId = outputSpinData.SpinId,
                        SlotsData = _missData[RandomUtil.NextInt(0, _missData.Length)],
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

            var d = new SpinResult()
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
            s += "\r\nResponse: " + JsonConvert.SerializeObject(d);
            NLogManager.LogMessage(s);
            return d;
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