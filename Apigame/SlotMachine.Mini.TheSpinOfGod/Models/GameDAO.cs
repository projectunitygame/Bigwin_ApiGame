using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Database;

namespace SlotMachine.Mini.TheSpinOfGod.Models
{
    public class GameDAO
    {
        public static List<RoomConfig> GetRoomList()
        {
            DBHelper db = new DBHelper(ConnectionString.GameConnectionString);
            return db.GetList<RoomConfig>("select RoomID, BetType from Rooms");
        }

        public static List<RoomJackpot> GetJackpotList(int betType)
        {
            DBHelper db = new DBHelper(ConnectionString.GameConnectionString);
            if(betType == 1)
                return db.GetList<RoomJackpot>("select RoomID, JackpotFund from RoomFunds");
            else return db.GetList<RoomJackpot>("select RoomID, JackpotFund from RoomFunds_Coin");
        }
    }
}