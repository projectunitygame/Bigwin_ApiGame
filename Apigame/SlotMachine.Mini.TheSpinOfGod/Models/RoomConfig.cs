using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SlotMachine.Mini.TheSpinOfGod.Models
{
    public class RoomConfig
    {
        public int RoomID { get; set; }
        public int BetType { get; set; }
    }

    public static class Room
    {
        public static List<RoomConfig> RoomList { get; private set; }
        public static void Init()
        {
            RoomList = GameDAO.GetRoomList();
        }
    }
}