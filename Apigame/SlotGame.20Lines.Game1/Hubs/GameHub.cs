
using SlotMachine.TheThreeKingdoms.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SlotGame._20Lines.Game1.Models;
using Utilities.Cache;
using Utilities.ConfigHelper;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace SlotGame._20Lines.Game1.Hubs
{
    [HubName("Game1Hub")]
    public class GameHub : Hub
    {
        #region Game Play
        [HubMethodName("PlayNow")]
        public async Task<long> PlayNow(MoneyType moneyType, int roomId)
        {

            if ((int)moneyType != 1 && (int)moneyType != 2)
            {
                return -999;
            }

            if (roomId < 1 || roomId > 4)
                return -888;
            
            try
            {
                var accountId = AccountSession.AccountID;
                var accountName = AccountSession.AccountName;
                if (accountId < 1 || string.IsNullOrEmpty(accountName))             
                    return -1001;
                //var connections = ConnectionHandler.Instance.GetConnections(accountId);
                //if (connections.Count > 1)
                //{
                //    var currentConnection = Context.ConnectionId;
                //    foreach(var other in connections.Where(con => !con.Equals(currentConnection)))
                //    {
                //        if(currentConnection != other)
                //            Clients.Client(other).message(-1002, "Tài khoản của bạn đang chơi trên thiết bị khác");
                //    }
                    
                //}                   
                if (CacheCounter.CheckAccountActionFrequency(accountId.ToString(), 60, "PlayNow") >
                    Config.GetIntegerAppSettings("TotalAllowPlayNow", 60))
                {
                    NLogManager.LogMessage($"BlockPlayNow=>Acc:{accountId}|User:{accountName}|Room:{roomId}|Ip:{IPAddressHelper.GetClientIP()}");
                    return -1002;
                }

                var gameInfo = GameHandler.Instance.PlayNow(moneyType, roomId, accountId, accountName);
                if (gameInfo.ResponseStatus >= 0)
                {
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
                    Clients.Caller.joinGame(gameInfo);
                }                  
                return accountId;
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
        public int Spin(MoneyType moneyType, int roomId, string lines)
        {

            long accountId = AccountSession.AccountID;
            string accountName = AccountSession.AccountName;

            try
            {
                if (accountId < 1 || String.IsNullOrEmpty(accountName))
                {
                    return (int)Enums.ErrorCode.NotAuthen;
                }

                if (CacheCounter.AccountActionCounter(accountId.ToString(), "BlockSpamSpin") > 0)
                {
                    NLogManager.LogMessage($"BlockSpinAm=>Acc:{accountId}|User:{accountName}|Lines:{lines}|Ip:{IPAddressHelper.GetClientIP()}");
                    return -1002;
                }

                var spinData = GameHandler.Instance.Spin(accountId, accountName, lines, moneyType, roomId);
                if (spinData != null && spinData.ResponseStatus < 0)
                {
                    if (CacheCounter.CheckAccountActionFrequency(accountName, 10, "InvalidSpin") > 5)
                    {
                        //Block Spin 10s
                        CacheCounter.CheckAccountActionFrequency(accountName, 10, "BlockSpamSpin");
                    }
                    string message = "Rất tiếc hệ thống của chúng tôi đang bận!Mời bạn thử lại sau!";
                    if (spinData.ResponseStatus == -51)
                    {
                        message = "Số dư của bạn không đủ";
                    }
                    else if (spinData.ResponseStatus == -232)
                    {
                        message = "Số line chọn không hợp lệ";
                    }
                    Clients.Caller.message(spinData.ResponseStatus, message);
                }
                Clients.Caller.resultSpin(spinData);

                return (int)Enums.ErrorCode.SuccessRequest;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return (int)Enums.ErrorCode.Exception;
            }
        }

        [HubMethodName("FinishBonusGame")]
        public int FinishBonusGame(MoneyType moneyType, int spinId)
        {         
            try
            {
                long accountId = AccountSession.AccountID;
                string username = AccountSession.AccountName;
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    return (int)Enums.ErrorCode.NotAuthen;
                }

                long prizeValue = 0, balance = 0;
                long response = GameHandler.Instance.FinishBonusGame(moneyType, spinId, ref prizeValue, ref balance);
                Clients.Caller.BonusGameResult(response, prizeValue, balance);
                return (int)Enums.ErrorCode.SuccessRequest;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return (int)Enums.ErrorCode.Exception;
            }
        }
        #endregion

        #region persistance
        [HubMethodName("PingPong")]
        public void PingPong()
        {
            return;
        }

        public override Task OnConnected()
        {
            Clients.Caller.UpdateJackpot(GameHandler.Instance.GetJackpot(MoneyType.Gold));
            var accountId = AccountSession.AccountID;
            var accountName = AccountSession.AccountName;
            if (accountId > 0)
            {
                NLogManager.LogMessage($"Player connected => AccountId:{accountId}|AccountName:{accountName}");
                ConnectionHandler.Instance.PlayerConnect(accountId, Context.ConnectionId);
            }           
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var accountId = AccountSession.AccountID;
            var accountName = AccountSession.AccountName;
            if (accountId > 0)
            {
                NLogManager.LogMessage($"Player disconnected => AccountId:{accountId}|AccountName:{accountName}");
                ConnectionHandler.Instance.PlayerDisconnect(Context.ConnectionId);
            }       
            return base.OnDisconnected(true);
        }

        // ReSharper disable once RedundantOverridenMember
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        #endregion persistance
    }
}