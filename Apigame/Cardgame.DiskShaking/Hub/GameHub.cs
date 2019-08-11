using Cardgame.DiskShaking.Container;
using Cardgame.DiskShaking.Controllers;
using Cardgame.DiskShaking.Models;
using Cardgame.DiskShaking.Models.Exceptions;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;
using Utilities.Session;

namespace Cardgame.DiskShaking.Hubs
{
    [HubName("diskShaking"), Authorize]
    public class GameHub : Hub
    {
        private GameManager _gameManager;
        PlayerManager _playerManager;
        ConnectionHandler _connectionHandler;

        public GameHub(GameManager gameManager, PlayerManager playerManager, ConnectionHandler connectionHandler)
        {
            _gameManager = gameManager;
            _playerManager = playerManager;
            _connectionHandler = connectionHandler;
        }

        public async Task Create(MoneyType moneyType, RoomType roomType)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Session session = _gameManager.CreateSession(accountId, roomType, moneyType);
                await Clients.Caller.join(session, session.GetPlayer(accountId), session.GetSittingPosition(accountId), session.GetSumaryBet(accountId));
                await Groups.Add(Context.ConnectionId, $"room_{session.Id}");
                session.TryToStart();
            }
            catch (PlayerNotFoundException)
            {
                await Clients.Caller.errorCode(-1);
            }
            catch (PlayerAlreadyInRoomException)
            {
                await Clients.Caller.errorCode(-2);
            }
            catch (NotEnoughMoneyException)
            {
                await Clients.Caller.errorCode(-4);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task BuyGate(Gate gate)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    session.BuyGate(accountId, AccountSession.AccountName, gate);
                }
            }
            catch (SellGateException)
            {
                await Clients.Caller.errorCode(-10);
            }
            catch (GateSoldOutException)
            {
                await Clients.Caller.errorCode(-11);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task SellGate(Gate gate)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    session.SellGate(accountId, gate);
                }
            }
            catch (SellGateException)
            {
                await Clients.Caller.errorCode(-10);
            }
            catch (GateSoldOutException)
            {
                await Clients.Caller.errorCode(-11);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task Bet( string gate)
        {
            try
            {
                List<BetGateData> gates = JsonConvert.DeserializeObject<List<BetGateData>>(gate);
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    session.Bet(accountId, AccountSession.AccountName, gates);
                }
            }
            catch (PlayerNotFoundException)
            {
                await Clients.Caller.errorCode(-1);
            }
            catch (NotInBettingStateException)
            {
                await Clients.Caller.errorCode(-9);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task GetReady()
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    session.Ready(accountId);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task RefreshLobby(MoneyType moneyType)
        {
            try
            {
                await Clients.Caller.lobby(_gameManager.GetLobbyRooms(moneyType));
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task EnterLobby(MoneyType moneyType)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                _connectionHandler.PlayerConnect(accountId, Context.ConnectionId);
                Player player = _playerManager.AddPlayer(accountId);
                if (player.RoomId < 0)
                {
                    await Clients.Caller.lobby(_gameManager.GetLobbyRooms(moneyType));
                    return;
                }
                else
                {
                    //rejoin current session
                    Session session = _gameManager.GetRoom(player.RoomId);
                    int status = session.GetSittingPosition(accountId);
                    await Clients.Caller.join(session, session.GetPlayer(accountId), status, session.GetSumaryBet(accountId));
                    await Groups.Add(Context.ConnectionId, $"room_{session.Id}");
                }
            }
            catch (PlayerNotFoundException)
            {
                await Clients.Caller.errorCode(-1);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task Sit(int position)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    var p = session.Sit(accountId, position);
                    await Clients.Group($"room_{session.Id}").sitting(position, p, session.TotalPlayer, session.MaxPlayer);
                }
            }
            catch (PlayerNotFoundException)
            {
                await Clients.Caller.errorCode(-1);
            }
            catch (AlreadySitException)
            {
                await Clients.Caller.errorCode(-12);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task Join(long sessionId)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                int status = 0;

                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session oldSession = _gameManager.GetRoom(player.RoomId);
                    oldSession.RemovePlayer(player, 1);
                    player.LeaveGame();
                }
                Session session = _gameManager.JoinSession(sessionId, accountId, out status);
                await Clients.Caller.join(session, session.GetPlayer(accountId), status, session.GetSumaryBet(accountId));
                if (status > 0)
                    await Clients.Group($"room_{session.Id}").sitting(status, session.GetPlayer(accountId), session.TotalPlayer, session.MaxPlayer);
                else await Clients.Group($"room_{session.Id}").ccu(session.TotalPlayer, session.MaxPlayer);
                await Groups.Add(Context.ConnectionId, $"room_{session.Id}");
                session.TryToStart();
            }
            catch (PlayerNotFoundException)
            {
                await Clients.Caller.errorCode(-1);
            }
            catch (PlayerAlreadyInRoomException)
            {
                await Clients.Caller.errorCode(-2);
            }
            catch (RoomHasBeenDeactiveException)
            {
                await Clients.Caller.errorCode(-3);
            }
            catch (NotEnoughMoneyException)
            {
                await Clients.Caller.errorCode(-4);
            }
            catch (RoomFullException)
            {
                await Clients.Caller.errorCode(-5);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
            }
        }

        public async Task Leave()
        {
            try
            {
                long accountId = AccountSession.AccountID;
                Player player = _playerManager.GetPlayer(accountId);
                if (player.RoomId > 0)
                {
                    Session session = _gameManager.GetRoom(player.RoomId);
                    session.RemovePlayer(player, 1);
                }
                else await Clients.Caller.playerLeave(accountId, 1, 0, 0);
            }
            catch (CantLeaveNowException)
            {
                await Clients.Caller.errorCode(-6);
            }
            catch (Exception ex)
            {
                await Clients.Caller.errorCode(-99);
                NLogManager.PublishException(ex);
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

            _connectionHandler.PlayerDisconnect(accountId, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }
    }
}