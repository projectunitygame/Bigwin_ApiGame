using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Database;

namespace GamePortal.API.DataAccess
{
    public class GameDAO
    {
        public static List<Game> GameList(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetList<Game>($"select B.ID, B.Name, isnull(A.Disabled, convert(bit, 0)) Disabled from game.Lock A right join game.Game B on A.GameId = B.ID and AccountId = {accountId}");
        }

        public static void ExecuteLockCommand(string cmd)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            db.ExecuteNonQuery(cmd);
        }
    }
}