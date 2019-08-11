using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR.Hubs;
using Minigames.DataAccess.DAO;
using Minigames.DataAccess.DTO;
using Minigames.DataAccess.Factory;
using Utilities.Log;

namespace MiniHilo.WebServer.Handlers
{
    public class HiLoHandler
    {
        private const string GAMENAME = "HiLo";

        private readonly static Lazy<HiLoHandler> _instance;

        private ConcurrentDictionary<string, JackpotData> JackpotHiLo = new ConcurrentDictionary<string, JackpotData>();

        private readonly IHiloDao _HiLoDAO = AbstractDaoMinigame.Instance().CreateMiniHiloDao();

        private readonly Timer _HiLoJackpotTimer;
        #region update 10/03/2016

        private ConcurrentDictionary<long, AccountModel> ListAccount = new ConcurrentDictionary<long, AccountModel>();

        private ConcurrentDictionary<long, HiLoGetAccountInfoResponse> _listSession = new ConcurrentDictionary<long, HiLoGetAccountInfoResponse>();


        public void AddOrUpdateSession(HiLoGetAccountInfoResponse request)
        {
            request.CurrentTime = DateTime.Now;
            if (request.currentTurnId > 0)
            {
                _listSession.AddOrUpdate(request.currentTurnId, request, (k, v) => request);
            }
        }

        public void FinishAccSession(long turnId)
        {
            if (turnId > 0)
            {
                var result = new HiLoGetAccountInfoResponse();
                _listSession.TryRemove(turnId, out result);
            }
        }

        public void AutoFinishExpriedSession()
        {
            if (_listSession != null && _listSession.Count > 0)
            {
                foreach (var item in _listSession)
                {
                    if (item.Value.CurrentTime.AddMinutes(2) < DateTime.Now)
                    {
                        NLogManager.LogMessage(string.Format("AutoFinishSessionCompute:Acc:{0}|User:{1}|Turn:{2}",
                            item.Value.AccountId, item.Value.AccountName, item.Value.currentTurnId));
                        AutoFinishSession(item.Value, item.Value.AccountId, item.Value.AccountName, "127.0.0.1");
                        FinishAccSession(item.Key);
                    }
                }
            }
        }

        public bool AutoFinishSession(HiLoGetAccountInfoResponse resultAccInfo, 
            long accountId, string accountName,string clientIp)
        {
            try
            {
                //Đã hết thời gian chơi
                if (resultAccInfo.currentStep > 1)
                {
                    //Đã chơi có thể tự finish
                    var result = AbstractDaoMinigame.Instance().CreateMiniHiloDao().
                        SetBet((int) accountId, accountName, resultAccInfo.currentRoomId,
                            resultAccInfo.currentBetType, 2, 1, clientIp,
                            1, 1);
                    if (result.responseStatus > 0)
                    {
                        //Đã finish thành công
                        //Chuyển ván mới
                        return true;
                    }
                    return false;
                }
                //Chơi random 1 bước sau đó finish
                var randomLocation = new Random().Next(0, 1);
                var resultStep1 = AbstractDaoMinigame.Instance().CreateMiniHiloDao().
                    SetBet((int) accountId, accountName, resultAccInfo.currentRoomId,
                        resultAccInfo.currentBetType, 1, randomLocation, clientIp,
                        1, 1);
                if (resultStep1.responseStatus > 0)
                {
                    //Finish Phiên
                    var resultStep2 = AbstractDaoMinigame.Instance().CreateMiniHiloDao().
                        SetBet((int) accountId, accountName, resultAccInfo.currentRoomId,
                            resultAccInfo.currentBetType, 2, 1, clientIp,
                            1, 1);

                    if (resultStep2.responseStatus > 0)
                    {
                        //Đã finish thành công
                        //Chuyển ván mới
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return false;
            }
        }

        //private static List<>  

        public bool AddPlayer(long accountid)
        {
            if (accountid < 1) return false;
            AccountModel player = GetPlayer(accountid);
            if (player == null)
            {
                player = new AccountModel(accountid);
                return ListAccount.TryAdd(accountid, player);
            }
            return true;
        }



        public AccountModel GetPlayer(long accountid)
        {
            if (accountid < 1) return null;
            AccountModel player = null;
            if (ListAccount.ContainsKey(accountid))
                ListAccount.TryGetValue(accountid, out player);
            return player;
        }

        public bool RemovePlayer(long accountid)
        {
            AccountModel player = GetPlayer(accountid);
            if (player == null)
            {
                return true;
            }
            return ListAccount.TryRemove(accountid, out player);
        }

        public void UpdatePlayer(long accountid)
        {
            AccountModel player = GetPlayer(accountid);
            if (player == null)
            {
                return;
            }
            AccountModel newplayer = new AccountModel(accountid);
            ListAccount.TryUpdate(accountid, newplayer, player);
        }

        /// <summary>
        /// cập nhật số lần hết tiền
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="isFailue"></param>
        public void UpdatePlayer(long accountid, bool isFailue)
        {
            AccountModel player = GetPlayer(accountid);
            if (player == null)
            {
                return;
            }
            AccountModel newplayer = player.Copy();
            newplayer.count++;
            ListAccount.TryUpdate(accountid, newplayer, player);
        }

        #endregion update 10/03/2016

        public static HiLoHandler Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        static HiLoHandler()
        {
            _instance = new Lazy<HiLoHandler>(() => new HiLoHandler());
        }

        private HiLoHandler()
        {
            this._HiLoJackpotTimer = new Timer(new TimerCallback(this.HiLoJackpotCallback), null, 0, 1000);
        }

        private void HiLoAccountInfoToClient(HiLoGetAccountInfoResponse data, string connectionId)
        {
            ((dynamic)ConnectionHandler.Instance.Clients.Client(connectionId)).resultHiLoAccountInfo(data);
        }

        private void HiLoAddOrUpdateJackpotGroup(string connectionId, string jackpotGroup)
        {
            ConnectionHandler.Instance.RemoveGroups(connectionId, this.JackpotHiLo.Keys);
            ConnectionHandler.Instance.AddGroup(connectionId, jackpotGroup);
        }

        public void HiLoGetAccountInfo(long accountId, string username, string connectionId)
        {
            HiLoGetAccountInfoResponse accountInfo = this._HiLoDAO.GetAccountInfo((int)accountId, username);
            HiLoAccountInfoToClient(accountInfo, connectionId);
        }

        public void HiLoGetJackpot(byte betType, byte roomId, string connectionId)
        {
            string groupName = ConnectionHandler.Instance.GetGroupName(betType, roomId, "HiLo");
            this.HiLoAddOrUpdateJackpotGroup(connectionId, groupName);
            JackpotData jackpotDatum1 = null;
            this.JackpotHiLo.TryGetValue(groupName, out jackpotDatum1);
            if ((jackpotDatum1 == null || jackpotDatum1.Jackpot == (long)0))
            {
                jackpotDatum1 = new JackpotData();
                jackpotDatum1.BetType = (betType);
                jackpotDatum1.RoomID = (roomId);
                jackpotDatum1.Jackpot = (this._HiLoDAO.GetJackpot((int)betType, (int)roomId));
                jackpotDatum1.LastUpdated = (DateTime.Now);
                this.JackpotHiLo.AddOrUpdate(groupName, jackpotDatum1, (string key, JackpotData oldValue) => jackpotDatum1);
            }
            this.HiLoUpdateJackpotToClient(connectionId, jackpotDatum1);
        }

        public void HiLoHideSlot(string connectionId)
        {
            ConnectionHandler.Instance.RemoveGroups(connectionId, this.JackpotHiLo.Keys);
        }

        private void HiLoJackpotCallback(object obj)
        {
            try
            {
                foreach (JackpotData value in this.JackpotHiLo.Values)
                {
                    long jackpot = this._HiLoDAO.GetJackpot((int)value.BetType, (int)value.RoomID);
                    if (jackpot != value.Jackpot)
                    {
                        value.Jackpot = (jackpot);
                        value.IsChanged = (true);
                    }
                    if (value.IsChanged)
                    {
                        string groupName = ConnectionHandler.Instance.GetGroupName(value.BetType, value.RoomID, "HiLo");
                        this.HiLoUpdateJackpotToGroupClient(groupName, value, null);
                        value.IsChanged = (false);
                    }
                }
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
        }

        private void HiLoUpdateJackpotToClient(string connectionId, JackpotData jackpot)
        {
            ((dynamic)ConnectionHandler.Instance.Clients.Client(connectionId)).jackpotHiLo(jackpot);
        }

        private void HiLoUpdateJackpotToGroupClient(string jackpotGroup, JackpotData jackpot, string connectionId = null)
        {
            IHubConnectionContext<dynamic> clients = ConnectionHandler.Instance.Clients;
            string[] strArrays = new string[] { connectionId };
            ((dynamic)clients.Group(jackpotGroup, strArrays)).jackpotHiLo(jackpot);
        }
    }
}