using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using DataAccess.DTO;
using MinigameVuabai.SignalR.Models;
using DataAccess.Factory;
using Utilities.Session;
using Utilities.Log;
using SlotMachine.Mini.TheSpinOfGod.Models;

namespace MinigameVuabai.SignalR.Controllers
{
    [Microsoft.AspNet.SignalR.Authorize]
    [HubName("HubThanQuay")]
    public class HubVuabai : Hub
    {

        private int _serviceId = int.Parse(ConfigurationManager.AppSettings["ServiceId"]);
        private int _sourceId = int.Parse(ConfigurationManager.AppSettings["SourceId"]);
        private int _merchantId = int.Parse(ConfigurationManager.AppSettings["MerchantId"]);

        #region GamePlay
        /// <summary>
        /// Hàm joint room
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        [HubMethodName("PlayNow")]
        public int JointRoom(int roomId, int betType)
        {
            
            if (betType != 1 & betType != 2)
            {
                // Loại tiền không đúng
                return -2;
            }

            //var roomLst = Room.RoomList.Where(x => x.BetType == betType).ToList();

            //if(roomId <= 0 && roomId > roomLst.Count)
            //{
            //    return -1;
            //}

            //var currentRoom = roomLst.ElementAt(roomId - 1);

            if (CheckIpPostFrequency(30, "jointroom") > 30)
            {
                return -1;
            }

            for(int i = 1; i <= 4; i++)
            {
                Groups.Remove(Context.ConnectionId, $"room_1_{i}");
                Groups.Remove(Context.ConnectionId, $"room_2_{i}");
            }

            Groups.Add(Context.ConnectionId, $"room_{betType}_{roomId}");

            var jackport = ConnectionHandler.Instance.GetJackPot(roomId, betType);
            Clients.Caller.UpdateJackPot(jackport);

            return 1;
        }

        /// <summary>
        /// Hàm spin
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="betType"></param>
        /// <param name="linesData"></param>
        /// <returns></returns>
        [HubMethodName("UserSpin")]
        public int VuaBai_Spins(int id, int betType, string linesData)
        {
            if (betType != 1 & betType != 2)
            {
                // Loại tiền không đúng
                return -2;
            }

            //var roomLst = Room.RoomList.Where(x => x.BetType == betType).ToList();

            //if (id <= 0 && id > roomLst.Count)
            //{
            //    return -1;
            //}

            //var currentRoom = roomLst.ElementAt(id - 1);

            if (!CheckLinesInput(linesData))
            {
                return -3;
            }

            var inputSpin = new InputSpin();
            inputSpin.ServiceId = _serviceId;
            inputSpin.AccessToken = string.Empty;
            inputSpin.AccountId = AccountSession.AccountID;
            inputSpin.UserName = AccountSession.AccountName;
            inputSpin.BetType = betType;
            inputSpin.RoomId = id;
            inputSpin.LinesData = linesData;
            inputSpin.ClientIp = Utils.GetIp();
            inputSpin.SourceId = _sourceId;
            inputSpin.MerchantId = _merchantId;
            var objReturn = new SpinsInfo();
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if (accountId > 0)
                {
                    var blockBet = false;
                    int countInvalidSpin = CachingHandler.CheckAccountAction(accountName, "InvalidSpin");
                    if (countInvalidSpin > 4)
                    {
                        if(countInvalidSpin == 5)
                            NLogManager.LogMessage(string.Format("Blocked_InvalidSpin => accId: {0} | accName: {1} | ip: {2}", inputSpin.AccountId, inputSpin.UserName, inputSpin.ClientIp));
                        CachingHandler.AddAccountAction(accountName, "InvalidSpin", 120);
                        blockBet = true;
                    }
                    int countNumOfPlays = CachingHandler.CheckAccountAction(accountName, "UserSpin");
                    if (countNumOfPlays > 0)
                    {
                        if(countNumOfPlays == 1)
                            NLogManager.LogMessage(string.Format("Blocked_FastSpin => accId: {0} | accName: {1} | ip: {2}", inputSpin.AccountId, inputSpin.UserName, inputSpin.ClientIp));
                        blockBet = true;
                    }
                    if (!blockBet)
                    {

                        var spinData = AbstractDaoFactory.Instance().CreateEventDao()
                            .SP_SlotsKingPoker_Spin(inputSpin);
                        objReturn._SpinID = spinData.SpinId;
                        objReturn._SlotsData = spinData.SlotData;
                        objReturn._PrizesData = spinData.PrizesData;
                        objReturn._TotalBetValue = spinData.TotalBetValue;
                        objReturn._TotalPrizeValue = spinData.TotalPrizeValue;
                        objReturn._IsJackpot = spinData.IsJackpot;
                        objReturn._Jackpot = spinData.Jackpot;
                        objReturn._Balance = spinData.Balance;
                        objReturn._ResponseStatus = spinData.ResponseStatus;
                        objReturn.LuckyData = spinData.LuckyData;
                        objReturn.TotalJackPot = spinData.TotalJackPot;
                        NLogManager.LogMessage(string.Format("Spin => accId: {0} | accname: {1} | ip: {2} | SpinId: {7} | Balance: {8}| response: {3} | TotalPrize: {4}|PrizeValue: {5} | lineBet: {6}|TotalSo10:{9}|IsX2:{10}", 
                            inputSpin.AccountId, inputSpin.UserName, inputSpin.ClientIp, spinData.ResponseStatus, spinData.TotalPrizeValue, spinData.PrizesData, linesData, spinData.SpinId, spinData.Balance, spinData.LuckyData, spinData.TotalJackPot));
                        Clients.Caller.ResultSpin(objReturn);
                        if (spinData.ResponseStatus < 0)
                        {
                            CachingHandler.AddAccountAction(accountName, "InvalidSpin", 1);
                            return spinData.ResponseStatus;
                        }
                        else
                        {
                            CachingHandler.AddAccountAction(accountName, "UserSpin", 1);                         
                        }
                        ConnectionHandler.Instance.UpdateJackpot(id, betType, spinData.Jackpot);
                        //Clients.Group("room" + (roomId + 4 * (betType - 1))).UpdateJackPot(spinData.Jackpot);
                        
                    }
                    else
                    {
                        objReturn._ResponseStatus = -10002;
                        return -10002;
                    }
                }
                else
                {
                    objReturn._ResponseStatus = -999;//Chua dang nhap
                    return -999;
                }
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return -10000; //Loi he thong web
            }

        }

        private string FormatMoney(long totalValue)
        {
            return totalValue.ToString("C0", CultureInfo.CurrentCulture).Replace("$", "").Replace(",", ".");
        }
        private bool CheckLinesInput(string line)
        {
            try
            {
                string[] lines = line.Split(',');
                if (lines.Length < 1 || lines.Length > 20)
                {
                    return false;
                }
                if (ContainsDuplicates(lines))
                {
                    return false;
                }
                var value = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!int.TryParse(lines[i].ToString(), out value))
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage(ex.Message);
                return false;
            }
        }
        private bool ContainsDuplicates(string[] arrayToCheck)
        {
            var duplicates = arrayToCheck
             .GroupBy(s => s)
             .Where(g => g.Count() > 1)
             .Select(g => g.Key);

            return (duplicates.Any());
        }
        #endregion

        #region các sự kiện connect disconnect hub

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                ConnectionHandler.Instance.PlayerDisconnect(Context.ConnectionId);
                return base.OnDisconnected(true);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return base.OnDisconnected(true);
        }


        #endregion

        #region Black List

        private int CheckIpPostFrequency(int totalSecond,string action)
        {
            string ip = Utils.GetIp();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("Post" + ip.ToLower() + AccountSession.AccountName + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("Post" + ip.ToLower() + AccountSession.AccountName + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("Post" + ip.ToLower() + AccountSession.AccountName + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }

        private int CountPostFrequency(string action)
        {
            string ip = Utils.GetIp();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("Post" + ip.ToLower() + AccountSession.AccountName + "_" + action);
            if (cacheCounter == null)
            {
                return 0;
            }
            return Convert.ToInt32(cacheCounter);
        }
        #endregion
    }
}