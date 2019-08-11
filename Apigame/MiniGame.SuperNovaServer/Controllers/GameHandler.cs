using Intecom.Software.RDTech.SlotMachine.DataAccess.DAO;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using Newtonsoft.Json;
using Studio.WebGame.SupperNova.Hubs;
using Studio.WebGame.SupperNova.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using Intecom.Software.RDTech.SlotMachine.DataAccess.Factory;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MiniGame.SuperNovaServer.Models;
using Utilities.Session;
using Utilities.IP;
using Enums = Intecom.Software.RDTech.SlotMachine.DataAccess.DTO.Enums;

namespace Studio.WebGame.SupperNova.Controllers
{
    public class GameHandler : AbstractGameHandler
    {
        private static readonly Lazy<GameHandler> _instance = new Lazy<GameHandler>(() => new GameHandler(GlobalHost.ConnectionManager.GetHubContext<GameHub>().Clients));

        private readonly ISlotMachineDAO _SlotMachineDAO = Intecom.Software.RDTech.SlotMachine.DataAccess.Factory
            .AbstractDAOFactory.Instance().CreateSlotMachineDAO();


        public static GameHandler Instance => _instance.Value;

        public ConcurrentDictionary<int, long> _jackpotGoldList;
        public ConcurrentDictionary<int, long> _jackpotCoinList;
        private Timer _jackpotTimer;
        private IHubConnectionContext<dynamic> Clients;
        private GameHandler(IHubConnectionContext<dynamic> clients)
            : base()
        {

            _jackpotCoinList = new ConcurrentDictionary<int, long>();
            _jackpotGoldList = new ConcurrentDictionary<int, long>();
            _jackpotTimer = new Timer(UpdateJackpotTimer, null, 0, 5000);
            UpdateJackpotTimer(null);
            Clients = clients;
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
                _jackpotGoldList.AddOrUpdate(roomId, jackpotValue, (key, value) => value);
            }
            else
            {
                _jackpotCoinList.AddOrUpdate(roomId, jackpotValue, (key, value) => value);
            }

        }

        void UpdateJackpotTimer(object obj)
        {
            var goldJackpots = AbstractDAOFactory.Instance().CreateSlotMachineDAO().GetJackpot(MoneyType.Gold);
            foreach (var jp in goldJackpots)
            {
                _jackpotGoldList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            var coinJackpots = AbstractDAOFactory.Instance().CreateSlotMachineDAO().GetJackpot(MoneyType.Coin);
            foreach (var jp in coinJackpots)
            {
                _jackpotCoinList.AddOrUpdate(jp.RoomID, jp.JackpotFund, (key, value) => jp.JackpotFund);
            }

            if (Clients == null)
                return;

            Clients.Group("Gold").UpdateJackpot(GetJackpot(MoneyType.Gold));
            Clients.Group("Coin").UpdateJackpot(GetJackpot(MoneyType.Coin));
        }
        #endregion



        #region Join Game

        public override void PlayNow(GamePlayer player)
        {
            var accountId = AccountSession.AccountID;
            var username = AccountSession.AccountName;

            var spinData = new SlotMachineSpinData();
            const int betValue = 0;
            const long currentJackPort = 0;
            const int gameStatus = (int)Enums.GameStatus.SPIN;
            //  var _info = _SlotMachineDAO.GetAccountInfo(player.BetType, (int)accountId, username, player.RoomId);

            player.BetValue = betValue;
            player.GameStatus = gameStatus;
            player.CurrentJackPort = currentJackPort;

            var accountInfo = new Account
            {
                AccountID = accountId,
                UserName = username,
                TotalStar = 0
            };

            player.Account = accountInfo;
            player.AccountID = accountId;
            switch (player.RoomId)
            {
                case 1:
                    player.BetValue = 100;
                    break;

                case 2:
                    player.BetValue = 1000;
                    break;

                case 3:
                    player.BetValue = 10000;
                    break;
            }
            player.SpinData = spinData;
            player.Balance = 0;
            long jackpot = 0;
            player.SpinData.Jackpot = jackpot;
            ReturnJoinGame(player);
        }

        private void ReturnJoinGame(GamePlayer player)
        {
            var connections = ConnectionHandler.Instance.GetConnections(player.AccountID);
            if (connections != null && connections.Count > 0)
            {
                foreach (var conn in connections)
                {
                    ConnectionHandler.Instance.HubContext.Clients.Client(conn).joinGame(player);
                }
            }
        }

        #endregion Join Game

        #region Do Spin

        public override SlotMachineSpinData PlaySpin(int roomId, MoneyType moneyType)
        {

            var _ClientIP = IPAddressHelper.GetClientIP();
            var accountId = AccountSession.AccountID;
            var accountName = AccountSession.AccountName;
            //var sourceId = AccountSession.SourceID;
            
            SlotMachineSpinData spinData = new SlotMachineSpinData()
            {
                ResponseStatus = -99
            };

            //int sourceGameId = Config.GetIntegerAppSettings("SourceGameId", 1);
            //if (sourceId == 3)
            //{
            //    sourceGameId = Config.GetIntegerAppSettings("SourceGameId_IOS", 1);
            //}
            //if (sourceId == 4)
            //{
            //    sourceGameId = Config.GetIntegerAppSettings("SourceGameId_ANDROID", 1);
            //}
            spinData = _SlotMachineDAO.Spin(moneyType, 1, 1, "",
                (int)accountId, accountName, "", roomId, _ClientIP);


            if (spinData.ResponseStatus < 0)
            {
                CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 15, "SpinAm");
                var totalSecondFail = int.Parse(ConfigurationManager.AppSettings["TimmerFailRq"]);
                AddStatusFrequency(totalSecondFail, "SpinLoi");
                return spinData;
            }
            UpdateCacheJackpot(roomId, spinData.Jackpot, moneyType);                             
            
            return spinData;
        }

        //public FreezeStarResponse FreezeStar(string UserName, long TotalPrizeValue, long SpinID)
        //{
        //    string msg = string.Format("Chuc mung ban da thang {0} Sao, phien {1} trong game Bom Tan. De bao mat, he thong da tu dong dong bang giup ban", TotalPrizeValue, SpinID);            
        //    //string accessToken = AccountSession.AccessToken;
        //    //string token = !string.IsNullOrEmpty(accessToken)
        //    //    ? accessToken
        //    //    : ConfigurationManager.AppSettings["SSO_ACCESS_TOKEN"];
        //    string token = ConfigurationManager.AppSettings["SSO_ACCESS_TOKEN"];


        //    var freezeStarUrl =
        //        string.Format("http://phatloc.uwingame.vn/mobilelogin/Transaction/FreezeStarAutoRequestData?" +
        //                      "accountName={0}&amount={1}&msg={2}&serviceID={3}&token={4}",
        //            UserName,
        //            TotalPrizeValue,
        //            msg,
        //            ConfigurationManager.AppSettings["SSO_SERVICE_ID"],
        //            token);
            
        //    NLogManager.LogMessage("FreezeStarResponse URL:  " + freezeStarUrl);

        //    var httpClient = new HttpClient();
        //    var task = httpClient.GetAsync(freezeStarUrl);

        //    var freezeNoti = task.ContinueWith(t => t.Result.Content.ReadAsAsync<FreezeStarResponse>()).Unwrap().Result;

        //    NLogManager.LogMessage("FreezeStarResponse:  " + JsonConvert.SerializeObject(freezeNoti));

        //    return freezeNoti;
        //}

        //public bool AccountVerifyMobile(long accountId)
        //{
        //    var verifyMobileUrl = string.Format("http://phatloc.uwingame.vn/mobilelogin/Authen/GetAccountVerifyMobile?accountID={0}", accountId);

        //    NLogManager.LogMessage("FreezeStarResponse URL:  " + verifyMobileUrl);

        //    var httpClient = new HttpClient();
        //    var task = httpClient.GetAsync(verifyMobileUrl);

        //    var verifyMobileResponse = task.ContinueWith(t => t.Result.Content.ReadAsAsync<VerifyMobileResponse>()).Unwrap().Result;

        //    NLogManager.LogMessage("AccountVerifyMobile:  " + JsonConvert.SerializeObject(verifyMobileResponse));

        //    return !string.IsNullOrEmpty(verifyMobileResponse.Mobile);
        //}

        private void ReturnSpin(long AccountID, GamePlayer player)
        {
            var connections = ConnectionHandler.Instance.GetConnections(AccountID);
            if (connections != null && connections.Count > 0)
            {
                foreach (var conn in connections)
                {
                    ConnectionHandler.Instance.HubContext.Clients.Client(conn).resultSpin(player);
                }
            }
        }

        #endregion Do Spin

        #region BlackList

        /// <summary>
        /// Thêm số lượt ăn theo tài khoản
        /// </summary>
        /// <param name="totalSecond"></param>
        /// <param name="action"></param>
        private void AddStatusFrequency(int totalSecond, string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("@Post" + ip.ToLower() + AccountSession.AccountName + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("@Post" + ip.ToLower() + AccountSession.AccountName + "_" + action, 1, policy);
            }
            cache.Set("@Post" + ip.ToLower() + AccountSession.AccountName + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
        }

        #endregion BlackList
    }
}