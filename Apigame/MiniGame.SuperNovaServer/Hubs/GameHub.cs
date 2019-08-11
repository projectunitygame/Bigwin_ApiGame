using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Studio.WebGame.SupperNova.Controllers;
using Studio.WebGame.SupperNova.Models;
using System;
using System.CodeDom;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MiniGame.SuperNovaServer.Models;
using Utilities.Session;
using Utilities.Log;
using Utilities.ConfigHelper;
using Utilities.IP;
using Enums = Intecom.Software.RDTech.SlotMachine.DataAccess.DTO.Enums;

namespace Studio.WebGame.SupperNova.Hubs
{
    [HubName("MiniSlot2")]
    public class GameHub : Hub
    {
        [HubMethodName("PlayNow")]
        public async Task<int> PlayNow()
        {
            try
            {
                var moneyType = MoneyType.Gold;
                var accountId = AccountSession.AccountID;
                var username = AccountSession.AccountName;
                if (accountId < 1 || String.IsNullOrEmpty(username))
                {
                    throw new NotAuthorizedException(Resources.Message.NotAuthen);
                }
                if (moneyType == MoneyType.Gold)
                {
                    await Groups.Remove(Context.ConnectionId, "Coin");
                    await Groups.Add(Context.ConnectionId, "Gold");
                }
                else
                {
                    await Groups.Remove(Context.ConnectionId, "Gold");
                    await Groups.Add(Context.ConnectionId, "Coin");
                }
 
                Clients.Caller.UpdateJackpot(GameHandler.Instance.GetJackpot(moneyType));
                return 1;
            }
            catch (NotAuthorizedException nig)
            {
                NLogManager.PublishException(nig);
            }
            catch (Exception ex)
            {
               
                NLogManager.PublishException(ex);
               
            }
            return -1;
        }

        [HubMethodName("Spin")]
        public int Spin(int roomId)
        {
            try
            {
                var moneyType = MoneyType.Gold;

                if (roomId < 1 || roomId > 4)
                    return -303;
                //Kiểm tra xem tài khoản vi phạm bắn số lần respone âm
                var totalFailRequest = int.Parse(ConfigurationManager.AppSettings["TotalFailRq"]);
                if (CheckStatusFrequency("SpinLoi") >= totalFailRequest)
                {
                    var totalSecondFail = int.Parse(ConfigurationManager.AppSettings["TimmerFailRq"]);
                    AddStatusFrequency(totalSecondFail, "SpinLoi");
                    return -1999;
                }
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;

             
                if (accountId < 1 || String.IsNullOrEmpty(accountName))
                {
                    return (int)Enums.ErrorCode.NotAuthen;
                }

                float maxSpinPerSecond =
                    Config.GetIntegerAppSettings("MAX_SPIN_PER_SECOND", 5);
                if (maxSpinPerSecond % 2 > 0)
                {
                    maxSpinPerSecond += 1;
                }
                if (CacheCounter.CheckAccountActionFrequencyMiliSecond(accountId.ToString(), 1000 / maxSpinPerSecond, "Spin") > 1)
                {
                    NLogManager.LogMessage(string.Format("BlockAccFast=> {0} ({1}) quay liên tục 1 giây {2} lần.",
                        accountName, accountId, maxSpinPerSecond));
                    return -1001;
                }
                if (CacheCounter.AccountActionCounter(accountId.ToString(), "SpinAm") > 5)
                {
                    NLogManager.LogMessage(string.Format("BlockAccAm=> {0} ({1}) bắn âm > 5 lần.", accountName, accountId));
                    return -1003;
                }

                var spinData = GameHandler.Instance.PlaySpin(roomId, moneyType);

                if(spinData.ResponseStatus == -48)
                {
                    Clients.Caller.message("Error-48", 3);
                    return -48;
                }

                if (spinData.ResponseStatus >= 0)
                {
                    Clients.Caller.ResultSpin(spinData);
                    return (int)Enums.ErrorCode.Success;
                }
                    
                else return spinData.ResponseStatus;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return (int)Enums.ErrorCode.Exception;
            }
        }

        #region persistance

        public override Task OnConnected()
        {
            Clients.Caller.UpdateJackpot(GameHandler.Instance.GetJackpot(MoneyType.Gold));
            var name = Context.QueryString["authorize"];
            NLogManager.LogMessage("authorize" + name);
            return base.OnConnected();
        }


        public override Task OnDisconnected(bool stopCalled)
        {

            return base.OnDisconnected(true);
        }

        // ReSharper disable once RedundantOverridenMember
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        #endregion persistance

        #region BlackList

        /// <summary>
        /// Kiểm tra ip thực hiện 1 hành động trong số giây (tự cộng số lượt mỗi lần gọi hàm check)
        /// Không ăn theo tài khoản
        /// </summary>
        /// <param name="totalSecond">Số giây kiểm tra</param>
        /// <param name="action">tên hành động</param>
        /// <returns>số lượt gọi hành động</returns>
        private int CheckIpPostFrequency(int totalSecond, string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("@Post" + ip.ToLower() + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("@Post" + ip.ToLower() + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("@Post" + ip.ToLower() + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Chỉ đếm số lượt trong cache
        /// </summary>
        /// <param name="action"></param>
        /// <returns>số lượt</returns>
        private int CheckStatusFrequency(string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("@Post" + ip.ToLower() + AccountSession.AccountName + "_" + action);
            if (cacheCounter == null)
            {
                return 0;
            }
            return Convert.ToInt32(cacheCounter);
        }

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