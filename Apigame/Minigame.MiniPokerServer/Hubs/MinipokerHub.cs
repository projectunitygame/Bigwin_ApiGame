using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Minigame.MiniPokerServer.Database.DTO;
using MiniPoker.WebServer.Handlers;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace MiniPoker.WebServer.Controllers
{
    [HubName("Minipoker")]
    [Authorize]
    public class MinipokerHub : Hub
    {
        private readonly static string INVALID_DATA_MESSAGE;

        private readonly static string NOT_LOGIN_MESSAGE;
        private static int _isMobilePl = 0;

        static MinipokerHub()
        {
            INVALID_DATA_MESSAGE = "Dữ liệu không đúng. Mời bạn thử lại.";
            NOT_LOGIN_MESSAGE = "Bạn chưa đăng nhập. Mời thử lại.";
        }


        public void GetJackpot(byte betType, byte roomID)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;

                if (CacheCounter.CheckIpPostFrequency(10, "GetJackpot") > 50)
                {
                    NLogManager.LogMessage(string.Format("BlockGetJackpot=>{0},{1}", accountId, IPAddressHelper.GetClientIP()));
                    return;
                }
                if ((accountId < 1 || string.IsNullOrEmpty(accountName)))
                {
                    throw new NotAuthorizedException(NOT_LOGIN_MESSAGE);
                }
                if ((betType < 1 || betType > 2 || roomID < 1 || roomID > 4))
                {
                    throw new InvalidOperationException(INVALID_DATA_MESSAGE);
                }
                MiniPokerHandler.Instance.AddPlayer(accountId);
                MiniPokerHandler.Instance.MpGetJackpot(betType, roomID, base.Context.ConnectionId);
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
        }

        public void HideSlot()
        {
            try
            {

                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;

                if (CacheCounter.CheckIpPostFrequency(10, "HideSlot") > 30)
                {
                    NLogManager.LogMessage(string.Format("BlockHideSlot=>{0},{1}", accountId, IPAddressHelper.GetClientIP()));
                    return;
                }
                if ((accountId < 1 || string.IsNullOrEmpty(accountName)))
                {
                    throw new NotAuthorizedException(NOT_LOGIN_MESSAGE);
                }
                MiniPokerHandler.Instance.RemovePlayer(accountId);
                MiniPokerHandler.Instance.MpHideSlot(base.Context.ConnectionId);
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
        }

        public override Task OnConnected()
        {
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                MiniPokerHandler.Instance.RemovePlayer(accountId);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return base.OnDisconnected(stopCalled);
        }
        [Authorize]
        public long Spin(byte betType, byte roomID)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if (accountId < 1 || string.IsNullOrEmpty(accountName))
                {
                    return -1001;
                }
                if ((betType < 1 || betType > 2 || roomID < 1 || roomID > 4))
                {
                    Logout();
                    NLogManager.LogMessage(string.Format("BlockAcc=> {0} ({1}) Wrong Input.", accountName, accountId));
                    return -1001;
                }
                float maxSpinPerSecond =
                    int.Parse(System.Configuration.ConfigurationManager.AppSettings["MAX_SPIN_PER_SECOND"]);
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
                if (CacheCounter.AccountActionCounter(accountId.ToString(),"SpinAm") > 5)
                {
                    NLogManager.LogMessage(string.Format("BlockAccAm=> {0} ({1}) bắn âm > 5 lần.", accountName, accountId));
                    return -1003;
                }
                MiniPokerHandler.Instance.UpdatePlayer(accountId);
                int res = MiniPokerHandler.Instance.MpSpin(accountId, accountName, betType, roomID, base.Context.ConnectionId, _isMobilePl);
                if (res < 0)
                {
                    CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 15, "SpinAm");
                }
                return res;
            }
            catch (NotAuthorizedException notAuthorizedException)
            {
                NLogManager.PublishException(notAuthorizedException);
                return -51;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return -99;
        }

        private void Logout()
        {
            long accountID = AccountSession.AccountID;
            MiniPokerHandler.Instance.RemovePlayer(AccountSession.AccountID);
            FormsAuthentication.SignOut();
            //CookieManager.RemoveAllCookies(true);
        }
    }
}