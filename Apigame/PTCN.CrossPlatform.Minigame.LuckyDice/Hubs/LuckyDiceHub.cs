using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat;
using System;
using System.Threading.Tasks;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Hubs
{
    [HubName("luckyDice"), Authorize]
    public class LuckyDiceHub : Hub
    {
        public void Text(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            bool flag = false;
            var msg = ChatFilter.RemoveBadWords(message, out flag);
            ChatMessage msgChat;
            var account = Lddb.Instance.GetAccountInfo(AccountSession.AccountID);
            int result = GameManager.CheckEnableChat(account.AccountID, account.DisplayName, AccountSession.UserType, msg, out msgChat);
            NLogManager.LogMessage($"{account.AccountID}|{ account.DisplayName}|{AccountSession.UserType} => {JsonConvert.SerializeObject(msgChat)}");
            if (result == -1)
            {
                msgChat = new ChatMessage
                {
                    T = 1,
                    //M = "Tổng đặt trong 5 ngày ít nhất 50.000 vàng mới có thể chat", Chanh
                    M = "Tổng tiền đặt trong 5 ngày của bạn phải tối thiểu 20.000 vàng chat",                   
                    U = "System Admin"
                };
                Clients.Caller.Msg(msgChat);
            }
            else if (result == -2)
            {
                msgChat = new ChatMessage
                {
                    T = 1,
                    M = "Bạn đã bị chặn chat, vui lòng liên hệ admin",
                    U = "System Admin"
                };
                Clients.Caller.Msg(msgChat);
            }
            else if (result == 0)
            {
                Clients.All.Msg(msgChat);
            }
        }

        public void GetMessage()
        {
            Clients.Caller.LstMsg(GameManager.GetRecentMessage());
        }

        public void Bet(int moneyType, long betAmount, int betSide)
        {
            try
            {
                if (betSide != 0 && betSide != 1)
                {
                    Clients.Caller.Error("Sai cửa đặt cược!");
                    return;
                }

                long accountId = AccountSession.AccountID;

                if (accountId <= 0)
                {
                    return;
                }

                long outNewBalance;
                long sumaryBet = 0;
                string msgError = string.Empty;

                int response = GameManager.Bet(moneyType, Context.ConnectionId, accountId, AccountSession.AccountName, IPAddressHelper.GetClientIP(), (Models.BetSide)betSide, betAmount, out sumaryBet, out outNewBalance, out msgError);

                if (response < 0)
                {
                    Clients.Caller.Error(msgError);
                    return;
                }

                Clients.Caller.BetSuccess(moneyType, betSide, sumaryBet, outNewBalance);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                Clients.Caller.Error(ex.Message);
            }
        }

        public void EnterLobby(int moneyType)
        {
            long accountId = AccountSession.AccountID;

            if (accountId > 0)
            {
                GameManager.Connect(moneyType, accountId, Context.ConnectionId);
                int side = -1;
                long totalBet = GameManager.GetTotalBet(moneyType, accountId, out side);
                Clients.Caller.EnterLobby(moneyType, GameManager.GetGameLoop(moneyType), totalBet, side);
            }
            else
            {
                NLogManager.LogMessage("Accountid < 0");
            }
        }
        /// <summary>
        /// Connected event
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {

            return base.OnConnected();
        }

        /// <summary>
        /// Reconnect event
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {

            return base.OnReconnected();
        }

        /// <summary>
        /// Disconnect event
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            long accountId = AccountSession.AccountID;

            if (accountId < 1)
                accountId = AccountSession.GetAccountID(Context);

            if (accountId > 0)
            {
                GameManager.Disconnect(accountId, Context.ConnectionId);
            }
            return base.OnDisconnected(stopCalled);
        }
    }
}