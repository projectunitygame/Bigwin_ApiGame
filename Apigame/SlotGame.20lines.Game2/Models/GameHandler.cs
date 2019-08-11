using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SlotGame._20lines.Game2.Hubs;
using SlotGame._20lines.Game2.Models;
using SlotGame._20lines.Game2.Database.DAO;
using SlotGame._20lines.Game2.Database.DTO;
using SlotGame._20lines.Game2.Database.Factory;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;


namespace SlotGame._20lines.Game2.Models
{
    public class GameHandler : IGameHandler
    {
        private static readonly Lazy<GameHandler> _instance = new Lazy<GameHandler>(() => new GameHandler(GlobalHost.ConnectionManager.GetHubContext<GameHub>().Clients));

        public static GameHandler Instance => _instance.Value;
        private static Timer _jackpotTimer;
        private ConcurrentDictionary<long, long> _jackpotGoldList;
        private readonly ConcurrentDictionary<long, long> _jackpotCoinList;
        private readonly ITreaSureIslandDao _slotMachineDAO = AbstractFactory.Instance().CreateTreaSureIslandDao();
        private IHubConnectionContext<dynamic> Clients;
        private GameHandler(IHubConnectionContext<dynamic> clients)
        {
            _jackpotTimer = new Timer(UpdateJackpotTimer, null, 2000, 5000);
            _jackpotGoldList = new ConcurrentDictionary<long, long>();
            _jackpotCoinList = new ConcurrentDictionary<long, long>();
            Clients = clients;
        }


        public AccountInfo PlayNow(MoneyType moneyType, int roomId, long accountId, string accountName)
        {
            var _info = _slotMachineDAO.GetAccountInfo((int)accountId, accountName, roomId, moneyType);

            NLogManager.LogMessage(
                $"PlayNow=>Acc:{accountId}|User:{accountName}|RoomId:{roomId}|=>accInfo:{JsonConvert.SerializeObject(_info)}");
            return _info;
        }

        public SpinData Spin(long accountId, string accountName, string lines, MoneyType montype, int roomId)
        {

            var spinData = _slotMachineDAO.Spin((int)accountId, accountName, lines, roomId, IPAddressHelper.GetClientIP(), montype);

            // Thông tin vinh danh bổ sung sau

            UpdateCacheJackpot(roomId, spinData.Jackpot, montype);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                GameLogHandler.Instance.LogSpin(accountName, roomId, spinData.TotalPrizeValue, spinData.IsJackpot ? 1 : 2);
            });

            return spinData;
        }

        public long FinishBonusGame(MoneyType moneyType, int spinId, ref long prizeValue, ref long balance)
        {

            var responeStatus = _slotMachineDAO.FinisBonusGame(moneyType, spinId, ref prizeValue, ref balance);
            return responeStatus;
        }

        #region Client Jackpot

        public string GetJackpot(MoneyType moneyType)
        {
            if (_jackpotGoldList.Count == 0)
                UpdateJackpotTimer(null);
            return moneyType == MoneyType.Gold ? JsonConvert.SerializeObject(_jackpotGoldList) : JsonConvert.SerializeObject(_jackpotCoinList);
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

        void UpdateJackpotTimer(object obj)
        {
            var goldJackpots = _slotMachineDAO.GetJackpot(MoneyType.Gold);
            foreach (var jp in goldJackpots)
            {
                _jackpotGoldList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            var coinJackpots = _slotMachineDAO.GetJackpot(MoneyType.Coin);
            foreach (var jp in coinJackpots)
            {
                _jackpotCoinList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            Clients.Group("Gold").UpdateJackpot(GetJackpot(MoneyType.Gold));
            Clients.Group("Coin").UpdateJackpot(GetJackpot(MoneyType.Coin));
        }
        #endregion
    }
}