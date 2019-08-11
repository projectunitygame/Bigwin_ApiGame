using Cardgame.DiskShaking.Database;
using Cardgame.DiskShaking.Models;
using Cardgame.DiskShaking.Models.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Container
{
    public class PlayerManager
    {
        private ConcurrentDictionary<long, Player> _players;
        public PlayerManager()
        {
            _players = new ConcurrentDictionary<long, Player>();
        }

        public Player AddPlayer(long id)
        {
                var player = GetPlayer(id);
                if (player != null)
                {
                    if (player.RoomId < 0)
                    {
                        goto update;
                    }
                    return player;
                }
            update:
                player = GameDAO.GetPlayerInfo(id);
                if (player == null)
                    throw new PlayerNotFoundException();
                _players.AddOrUpdate(id, player, (k, v) => player);
            return player;
        }

        public Player GetPlayer(long id)
        {
            Player player;
            if (_players.TryGetValue(id, out player))
            {
                return player;
            }
            return null;
        }
    }
}