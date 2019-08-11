using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models.Lobby
{
    public class LobbyRoom
    {
        public RoomState State { get; set; }
        public long RoomID { get; set; }
        public int Bet
        {
            get
            {
                return 1000;
            }
        }
        public int TotalPlayer { get; set; }
        public int MaxPlayer { get; set; }
    }

    public enum RoomState
    {
        WAITING = 0,
        PLAYING = 1,
        FULL = 2
    }
}