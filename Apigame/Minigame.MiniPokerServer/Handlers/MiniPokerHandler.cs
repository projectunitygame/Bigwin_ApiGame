using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Minigame.MiniPokerServer.Database.DAO;
using Minigame.MiniPokerServer.Database.DTO;
using Minigame.MiniPokerServer.Database.Factory;
using Newtonsoft.Json;
using Utilities.IP;
using Utilities.Log;

namespace MiniPoker.WebServer.Handlers
{
    public class MiniPokerHandler
    {
        private readonly static Lazy<MiniPokerHandler> _instance;

        private readonly IPokerDao _miniPokerDAO = AbstractDaoMinigame.Instance().CreateMiniPokerDao();

        private readonly Timer _MpJackpotTimer;

        private ConcurrentDictionary<string, JackpotData> JackpotMiniPoker = new ConcurrentDictionary<string, JackpotData>();

        #region update 10/03/2016

        private ConcurrentDictionary<long, AccountModel> ListAccount = new ConcurrentDictionary<long, AccountModel>();

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

        public static MiniPokerHandler Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        static MiniPokerHandler()
        {
            _instance = new Lazy<MiniPokerHandler>(() => new MiniPokerHandler());
        }

        private MiniPokerHandler()
        {
            this._MpJackpotTimer = new Timer(new TimerCallback(this.MpJackpotCallback), null, 0, 1000);
        }

        public void MpGetJackpot(byte betType, byte roomID, string connectionId)
        {
            string groupName = ConnectionHandler.Instance.GetGroupName(betType, roomID, "");
            this.MpHideSlot(connectionId);
            ConnectionHandler.Instance.AddGroup(connectionId, groupName);
            JackpotData jackpotDatum1 = null;
            this.JackpotMiniPoker.TryGetValue(groupName, out jackpotDatum1);
            if ((jackpotDatum1 == null || jackpotDatum1.Jackpot == (long)0))
            {
                if (jackpotDatum1 == null)
                {
                    jackpotDatum1 = new JackpotData();
                }
                jackpotDatum1.BetType = betType;
                jackpotDatum1.RoomID = roomID;
                jackpotDatum1.Jackpot = this._miniPokerDAO.GetJackpot((int)betType, (int)roomID);
                jackpotDatum1.LastUpdated = (DateTime.Now);
                this.JackpotMiniPoker.AddOrUpdate(groupName, jackpotDatum1, (string key, JackpotData oldValue) => jackpotDatum1);
            }
            this.MpUpdateJackpotToClient(connectionId, jackpotDatum1);
        }

        public void MpHideSlot(string connectionId)
        {
            ConnectionHandler.Instance.RemoveGroups(connectionId, this.JackpotMiniPoker.Keys);
        }

        private void MpJackpotCallback(object obj)
        {
            bool flag;
            try
            {
                foreach (JackpotData value in this.JackpotMiniPoker.Values)
                {
                    if (value.IsChanged)
                    {
                        flag = true;
                    }
                    else
                    {
                        DateTime lastUpdated = value.LastUpdated;
                        flag = !(lastUpdated.AddMinutes(1) < DateTime.Now);
                    }
                    if (!flag)
                    {
                        long jackpot = this._miniPokerDAO.GetJackpot((int)value.BetType, (int)value.RoomID);
                        if (jackpot != value.Jackpot)
                        {
                            value.Jackpot = jackpot;
                            value.IsChanged = (true);
                        }
                    }
                    if (value.IsChanged)
                    {
                        string groupName = ConnectionHandler.Instance.GetGroupName(value.BetType, value.RoomID, "");
                        this.MpUpdateJackpotToGroupClient(groupName, value);
                        value.IsChanged = (false);
                    }
                }
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
        }
        public MiniPokerSpinResponse BotSpin(
            long accountId, string accountName, int betType, int roomId,
            string ip, int sourceId, int merchantId, int mobilePl, bool isbot = false, bool nohu = false)
        {
            MiniPokerSpinResponse miniPokerSpinResponse =  _miniPokerDAO.Spin(accountId, accountName, (int)1, (int)roomId, "127.0.0.1", 1, 1, 1, isbot, nohu);

            string groupName = ConnectionHandler.Instance.GetGroupName((byte)betType, (byte)roomId, "");

            JackpotData jackpotDatum = null;
            this.JackpotMiniPoker.TryGetValue(groupName, out jackpotDatum);
            if (jackpotDatum == null)
            {
                jackpotDatum = new JackpotData();
                jackpotDatum.BetType = (byte)(betType);
                jackpotDatum.RoomID = (byte)(roomId);
                jackpotDatum.Jackpot = (miniPokerSpinResponse.Jackpot);
                jackpotDatum.LastUpdated = (DateTime.Now);
            }
            if (jackpotDatum.Jackpot != miniPokerSpinResponse.Jackpot)
            {
                jackpotDatum.Jackpot = (miniPokerSpinResponse.Jackpot);
                jackpotDatum.IsChanged = (true);
                this.JackpotMiniPoker.TryUpdate(groupName, jackpotDatum, jackpotDatum);
            }

            return miniPokerSpinResponse;
        }
        public int MpSpin(long accountId, string username, byte betType, byte roomID, string connectionId,int mobilePl)
        {
            int responseStatus;
            string clientIP = IPAddressHelper.GetClientIP();
            int sourceID = 1;
            int merchantID = 1;
            MiniPokerSpinResponse miniPokerSpinResponse = this._miniPokerDAO.Spin(accountId, username, (int)betType, (int)roomID, clientIP, sourceID, merchantID,mobilePl);
            try
            {
                if (miniPokerSpinResponse != null)
                {
                    this.MpSpinResultToClient(miniPokerSpinResponse, connectionId);
                    string groupName = ConnectionHandler.Instance.GetGroupName(betType, roomID, "");
                    ConnectionHandler.Instance.AddGroup(connectionId, groupName);
                    if (miniPokerSpinResponse.ResponseStatus > -1)
                    {                                              
                        JackpotData jackpotDatum = null;
                        this.JackpotMiniPoker.TryGetValue(groupName, out jackpotDatum);
                        if (jackpotDatum == null)
                        {
                            jackpotDatum = new JackpotData();
                            jackpotDatum.BetType = (betType);
                            jackpotDatum.RoomID = (roomID);
                            jackpotDatum.Jackpot = (miniPokerSpinResponse.Jackpot);
                            jackpotDatum.LastUpdated = (DateTime.Now);
                        }
                        if (jackpotDatum.Jackpot != miniPokerSpinResponse.Jackpot)
                        {
                            jackpotDatum.Jackpot = (miniPokerSpinResponse.Jackpot);
                            jackpotDatum.IsChanged = (true);
                            this.JackpotMiniPoker.TryUpdate(groupName, jackpotDatum, jackpotDatum);
                        }

                    }
                    responseStatus = miniPokerSpinResponse.ResponseStatus;
                    return responseStatus;
                }
            }
            catch (Exception exception1)
            {
                NLogManager.PublishException(exception1);
            }
            responseStatus = -99;
            return responseStatus;
        }

        private void MpSpinResultToClient(MiniPokerSpinResponse data, string connectionId)
        {
            ((dynamic)ConnectionHandler.Instance.Clients.Client(connectionId)).resultSpinMiniPoker(data);
        }

        private void MpUpdateJackpotToClient(string connectionId, JackpotData jackpot)
        {
            ((dynamic)ConnectionHandler.Instance.Clients.Client(connectionId)).jackpotMiniPoker(jackpot);
        }

        private void MpUpdateJackpotToGroupClient(string jackpotGroup, JackpotData jackpot)
        {
            ((dynamic)ConnectionHandler.Instance.Clients.Group(jackpotGroup, new string[0])).jackpotMiniPoker(jackpot);
        }

        private void MpUpdateJackpotToAll(string jackPot)
        {
            ConnectionHandler.Instance.Clients.All.UpdateJackpot(jackPot);
        }
    }
}