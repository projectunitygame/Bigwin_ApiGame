using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API
{
    [HubName("lobbygame"), Authorize]
    public class LobbyHub : Hub
    {
        private Account accountInfo = new Account();
        public string connectionID = "";
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message);
        }

        public void EnterLobby(int gameID)
        {
            try
            {
                long accountId = AccountSession.AccountID;

                if (accountId > 0)
                {
                    accountInfo = AccountDAO.GetAccountByID(accountId);
                    connectionID = Context.ConnectionId;
                    NLogManager.LogMessage("EnterLobby: " + JsonConvert.SerializeObject(accountInfo));
                    if (accountInfo != null)
                    {
                        LobbyManage.Connect(accountId, this);
                        Clients.Caller.EnterLobby("connect success");
                        //test
                        NLogManager.LogMessage("connect success " + accountId + "(" + accountInfo.Username + ")," + connectionID + "," + gameID);
                        Clients.Caller.UpdateMoneyLobby(accountInfo.Gold  + 200000, "Đại lý nạp 200.000");
                    }
                    //int side = -1;
                    //Clients.Caller.EnterLobby(moneyType, LobbyManage.GetGameLoop(moneyType), totalBet, side);
                }
                else
                {
                    NLogManager.LogMessage("EnterLobby Accountid < 0");
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage("ERROR EnterLobby: " + ex);
            }

        }

        public void Disconnect()
        {
            Clients.Caller.KickUser("kick user");
            NLogManager.LogMessage("send KickUser: " + connectionID + "\r\n" + accountInfo.AccountID + "|" + accountInfo.Username);
        }

        /// <summary>
        /// Connected event
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            if (accountInfo.AccountID > 0)
                NLogManager.LogMessage(accountInfo.AccountID + "," + accountInfo.Username + " is connected");
            return base.OnConnected();
        }

        /// <summary>
        /// Reconnect event
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            if (accountInfo.AccountID > 0)
                NLogManager.LogMessage(accountInfo.AccountID + "," + accountInfo.Username + " is re-connected");
            return base.OnReconnected();
        }

        /// <summary>
        /// Disconnect event
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            if (accountInfo.AccountID > 0)
                NLogManager.LogMessage(accountInfo.AccountID + "," + accountInfo.Username + " is disconnected(" + connectionID + ")");
            LobbyManage.Disconnect(accountInfo.AccountID, connectionID);
            return base.OnDisconnected(stopCalled);
        }
    }
}