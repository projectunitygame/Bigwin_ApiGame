using Microsoft.AspNet.SignalR.Hubs;
using Minigame.HooHeyHowServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities.Session;

namespace Minigame.HooHeyHowServer.Hubs
{
    [HubName("HooHeyHow"), Authorize]
    public class HooHeyHowHub : Microsoft.AspNet.SignalR.Hub
    {
        public void SetBetType(MoneyType betType)
        {
            GameSession.Session.Connect(betType, AccountSession.AccountID, Context.ConnectionId);
            var gameLogic = GameSession.Session.GetLogic(betType);
            Clients.Caller.SessionInfo(betType, GameSession.Session, new {
                BetInfo = gameLogic.BetGates,
                BetCount = gameLogic.BetGateCount
            }, gameLogic.GetPlayerBettingInfo(AccountSession.AccountID),
            GameSession.Session.GetSessionReward(AccountSession.AccountID, betType));
        }

        public void Bet(MoneyType betType, string betData)
        {
            GameSession.Session.Bet(betType, AccountSession.AccountID, AccountSession.AccountName, betData);
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

            GameSession.Session.Disconnect(accountId, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
    }
}