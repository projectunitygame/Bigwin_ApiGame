using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Minigames.DataAccess.DTO;
using Minigames.DataAccess.Factory;
using MiniHilo.WebServer.Handlers;
using Newtonsoft.Json;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace MiniHilo.WebServer.Controllers
{
    [HubName("Hilo")]
    public class HiloHub : Hub
    {
        private readonly static string INVALID_DATA_MESSAGE = "Dữ liệu không đúng. Mời bạn thử lại.";

        private readonly static string NOT_LOGIN_MESSAGE = "Bạn chưa đăng nhập. Mời thử lại.";

        [Authorize]
        [HubMethodName("GetAccountInfoHiLo")]
        public void GetAccountInfoHiLo()
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if ((accountId < 1 || string.IsNullOrEmpty(accountName)))
                {
                    throw new NotAuthorizedException(NOT_LOGIN_MESSAGE);
                }
                if (CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 15, "GetAccInfo") > 6)
                {
                    NLogManager.LogMessage(string.Format("BlockGetAccInfo=>Acc:{0}|User:{1}",
                        accountId, accountName));
                    return;
                }
                var resultAccInfo = AbstractDaoMinigame.Instance().CreateMiniHiloDao()
                    .GetAccountInfo((int)accountId, accountName);
                NLogManager.LogMessage("resultAccInfo: " + JsonConvert.SerializeObject(resultAccInfo));
                if (resultAccInfo.currentTurnId > 0)
                {
                    if (resultAccInfo.remainTime <= 0)
                    {
                        NLogManager.LogMessage(string.Format("AutoFinishSession:Acc:{0}|User:{1}|Turn:{2}",
                            accountId, accountName, resultAccInfo.currentTurnId));
                        if (HiLoHandler.Instance.AutoFinishSession(resultAccInfo, accountId, accountName, IPAddressHelper.GetClientIP()))
                        {
                            HiLoHandler.Instance.FinishAccSession(resultAccInfo.currentTurnId);
                            return;
                        }
                    }
                    Clients.Caller.resultHiLoAccountInfo(resultAccInfo);
                }
            }
            catch (Exception exception)
            {
                NLogManager.LogError("ERROR GetAccountInfoHiLo: " + exception);
                NLogManager.PublishException(exception);
            }
        }

        [Authorize]
        [HubMethodName("SetBetHiLo")]
        public int SetBetHiLo(int betType, int stepType, int locationId, int roomId)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if (accountId < 1 || string.IsNullOrEmpty(accountName))
                {
                    NLogManager.LogMessage(string.Format("Chưa đăng nhập"));
                    return -1001;
                }
                if (betType < 1 || betType > 2 || locationId < 0 || locationId > 1 || roomId < 1 || roomId > 5)
                {
                    Logout();
                    NLogManager.LogMessage(string.Format("BlockAcc=> {0} ({1}) Wrong Input.", accountName, accountId));
                    return -212;
                }

                if (CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 1, "SetBet") > 2)
                {
                    NLogManager.LogMessage(string.Format("BlockAccFast => {0} ({1}) bet liên tục 1 giây 2 lần.", accountName, accountId));
                    return -1001;
                }
                if (CacheCounter.AccountActionCounter(accountId.ToString(), "SetBetAm") > 5)
                {
                    NLogManager.LogMessage(string.Format("BlockAccAm => {0} ({1}) bắn âm > 5 lần.", accountName, accountId));
                    return -1003;
                }
                var result = AbstractDaoMinigame.Instance().CreateMiniHiloDao().
                    SetBet((int)accountId, accountName, roomId, betType, stepType, locationId, IPAddressHelper.GetClientIP(),
                        1, 1);

                if (stepType == 2 || result.prizeValue == 0 || result.responseStatus < 0)
                {
                    //Kết thúc phiên hoặc thua cuộc
                    HiLoHandler.Instance.FinishAccSession(result.turnId);
                }
                else
                {
                    if (result.responseStatus < 0)
                    {
                        //Add vào list để duyệt check finish phiên
                        var accInfo = new HiLoGetAccountInfoResponse()
                        {
                            currentTurnId = result.turnId,
                            currentRoomId = roomId,
                            currentBetType = betType,
                            AccountId = accountId,
                            AccountName = accountName,
                            currentStep = result.step
                        };
                        HiLoHandler.Instance.AddOrUpdateSession(accInfo);
                    }
                }
                if (result.responseStatus < 0)
                {
                    CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 15, "SetBetAm");
                }
                Clients.Caller.resultHiLoSetBet(result);
                return result.responseStatus;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return -99;
        }

        public void GetJackpotHiLo(byte betType, byte roomId)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if ((accountId < 1 || string.IsNullOrEmpty(accountName)))
                {
                    throw new NotAuthorizedException(NOT_LOGIN_MESSAGE);
                }
                if ((betType < 1 || betType > 2 || roomId < 1 || roomId > 5))
                {
                    throw new InvalidOperationException(INVALID_DATA_MESSAGE);
                }
                HiLoHandler.Instance.AddPlayer(accountId);
                HiLoHandler.Instance.HiLoGetJackpot(betType, roomId, base.Context.ConnectionId);
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
                long accountID = AccountSession.AccountID;
                HiLoHandler.Instance.RemovePlayer(accountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return base.OnDisconnected(stopCalled);
        }

        private void Logout()
        {
            HiLoHandler.Instance.RemovePlayer(AccountSession.AccountID);
            FormsAuthentication.SignOut();
            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
        }
    }
}