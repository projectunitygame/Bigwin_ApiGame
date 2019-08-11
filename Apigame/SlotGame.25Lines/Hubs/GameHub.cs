using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using SlotGame._25Lines.Database.DTO;
using SlotGame._25Lines.Handlers;
using SlotGame._25Lines.Models;
using SlotGame._25Lines.Models.Configs;
using SlotGame._25Lines.Models.SlotMachine;
using Utilities.Log;
using Utilities.Session;

namespace SlotGame._25Lines.Hubs
{
    [HubName("Slot25Lines")]
    public class GameHub : Hub
    {
        private IChecker _checker;
        public GameHub(IChecker checker)
        {
            _checker = checker;
        }

        [HubMethodName("PingPong")]
        public void PingPong()
        {
          
        }

        [HubMethodName("PlayNow")]
        public async Task PlayNow(MoneyType moneyType, int roomId)
        {
            if (!_checker.CheckMoneyType(moneyType))
                return;

            if(!_checker.CheckRoom(roomId))
                return;

            try
            {
                long accountId = AccountSession.AccountID;
                string accountName = AccountSession.AccountName;
                if (accountId < 1)
                {
                    return;
                }

                var connections = ConnectionHandler.Instance.GetConnections(accountId);
                if (connections.Count > 1)
                {
                    var currentConnection = Context.ConnectionId;
                    foreach (var other in connections.Where(con => !con.Equals(currentConnection)))
                    {
                        if (currentConnection != other)
                            Clients.Client(other).message(GameMessage.OtherDevices, MessageFactory.GetMessage(GameMessage.OtherDevices));
                    }
                }
                var accountInfo = GameHandler.Instance.GetAccountInfo(accountId, accountName, roomId, moneyType, out var response);
                if (response > 0)
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
                    Clients.Caller.JoinRoom(accountInfo);
                }                 
                // bổ sung đoạn chặn gọi quá nhiều thông tin info / khoảng thời gian

                // Xử lý lấy thông tin của người chơi

           
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
        }

        [HubMethodName("Spin")]
        public void Spin(MoneyType moneyType, int roomId, string lines)
        {
            if (!_checker.CheckMoneyType(moneyType))
                return;

            if (!_checker.CheckRoom(roomId))
                return;

            if(!_checker.CheckLines(lines))
                return;

            try
            {
                var accountId = AccountSession.AccountID;
                var accountName = AccountSession.AccountName;
                if(accountId < 1)
                    return;
                var spinResult = GameHandler.Instance.Spin(accountId, accountName, roomId, moneyType, lines);
                if (spinResult.ResponseStatus > 0)
                    Clients.Caller.SpinResult(spinResult);


            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
        }

        [HubMethodName("PlayLuckyGame")]
        public void PlayLuckyGame(MoneyType moneyType, X2Game step, int roomId, int spinId)
        {
            if(!_checker.CheckMoneyType(moneyType))
                return;

            if (!_checker.CheckRoom(roomId))
                return;
            try
            {
                var accountId = AccountSession.AccountID;
                var accountName = AccountSession.AccountName;

                if (accountId < 1)
                {
                    return;
                }

                var gameResult = GameHandler.Instance.PlayLuckyGame(moneyType, accountId, accountName, roomId, step, spinId);
                Clients.Caller.LuckyGameResult(gameResult);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        [HubMethodName("FinishBonusGame")]
        public void PlayBonusGame(MoneyType moneyType, int spinId)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                string username = AccountSession.AccountName;
                if (accountId < 1 || string.IsNullOrEmpty(username))
                {
                    return;
                }


                long response = GameHandler.Instance.FinishBonusGame(moneyType, spinId, out var prizeValue, out var balance);
                Clients.Caller.BonusGameResult(response, prizeValue, balance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
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

            Groups.Remove(Context.ConnectionId, "Coin");
            Groups.Remove(Context.ConnectionId, "Gold");
            return base.OnDisconnected(stopCalled);
        }
    }
}