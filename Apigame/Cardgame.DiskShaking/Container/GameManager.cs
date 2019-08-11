using Cardgame.DiskShaking.Models;
using Cardgame.DiskShaking.Models.Exceptions;
using Cardgame.DiskShaking.Models.Lobby;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Log;

namespace Cardgame.DiskShaking.Container
{
    public class GameManager
    {
        private int _index;
        private object _lockerIndexRoom;
        private ConcurrentDictionary<long, Session> _rooms;
        PlayerManager _playerManager;

        public GameManager(PlayerManager playerManager)
        {
            _index = 0;
            _lockerIndexRoom = new object();
            _rooms = new ConcurrentDictionary<long, Session>();
            _playerManager = playerManager;
        }

        public Session GetRoom(long id)
        {
            Session session;
            _rooms.TryGetValue(id, out session);
            return session;
        }

        public Session CreateSession(long accountId, RoomType type, MoneyType moneyType, long betValue = 1000)
        {
            Player player = _playerManager.GetPlayer(accountId);
            if (player == null)
                throw new PlayerNotFoundException();
            if (player.RoomId > 0)
                throw new PlayerAlreadyInRoomException();
            long balance = moneyType == MoneyType.GOLD ? player.Gold : player.Coin;
            if (balance < betValue)
                throw new NotEnoughMoneyException();
            Session session = null;
            lock (_lockerIndexRoom)
            {
                session = new Session(++_index, moneyType, type, this, betValue);
                session.AddPlayer(player);
                _rooms.AddOrUpdate(session.Id, session, (k, v) => v = session);
            }
            return session;
        }

        public List<LobbyRoom> GetLobbyRooms(MoneyType moneyType)
        {
            int roomCount = _rooms.Where(x => x.Value.TotalPlayer == 0 && x.Value.MoneyType == moneyType && x.Value.RoomType == RoomType.FIFTY).Count();

            if (roomCount == 0)
            {
                lock (_lockerIndexRoom)
                {
                    var session = new Session(++_index, moneyType, RoomType.FIFTY, this, 1000);
                    _rooms.AddOrUpdate(session.Id, session, (k, v) => v = session);
                }
            }

            roomCount = _rooms.Where(x => x.Value.TotalPlayer == 0 && x.Value.MoneyType == moneyType && x.Value.RoomType == RoomType.TWELVE).Count();

            if(roomCount < 9)
            {
                lock (_lockerIndexRoom)
                {

                    for (int i = 0; i < 10 - roomCount; i++)
                    {
                        var session = new Session(++_index, moneyType, RoomType.TWELVE, this, 1000);
                        _rooms.AddOrUpdate(session.Id, session, (k, v) => v = session);
                    }
                }
            }

            return _rooms.Values.Where(x => x.MoneyType == moneyType).Select(x => new LobbyRoom {
                MaxPlayer = x.MaxPlayer,
                TotalPlayer = x.TotalPlayer,
                RoomID = x.Id, 
                State = GetRoomState(x.CurrentState, x.TotalPlayer, x.MaxPlayer),
            }).ToList();
        }

        private RoomState GetRoomState(State state, int totalPlayer, int maxPlayer)
        {
            if (totalPlayer == maxPlayer)
                return RoomState.FULL;
            if (state != State.WAITING)
                return RoomState.PLAYING;
            return RoomState.WAITING;
        }

        public Session JoinSession(long sessionId, long accountId, out int status)
        {
            status = 0;
            Player player = _playerManager.GetPlayer(accountId);
            if (player == null)
                throw new PlayerNotFoundException();
            if (player.RoomId >= 0)
                throw new PlayerAlreadyInRoomException();
            Session session;
            if (_rooms.TryGetValue(sessionId, out session))
            {
                status = session.AddPlayer(player);
                return session;
            }
            throw new RoomHasBeenDeactiveException();
        }

        public void DeactiveSession(long roomId)
        {
            Session session;
            _rooms.TryRemove(roomId, out session);
        }
    }
}