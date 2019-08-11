using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.Events.Database.DAO;
using Game.Events.Database.DTO;
using Game.Events.Models;
using Utilities.Database;
using Utilities.Log;
using Newtonsoft.Json;

namespace Game.Events.Database.DAOImpl
{
    public class GateImpl : IGate
    {
        public List<BigWinPlayers> GetBigWinPlayers()
        {
            try
            {
                DBHelper db = new DBHelper(ConnectionString.SlotMachineReportConnectionString);
                return db.GetListSP<BigWinPlayers>("Report_GetBigWinPlayers");
            }
            catch(Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public List<BigWinPlayers> GeBigWinPlayersByID(int gameId, int topCount)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_TopCount", topCount);
                DBHelper db = new DBHelper(ConnectionString.SlotMachineReportConnectionString);
                return db.GetListSP<BigWinPlayers>("[dbo].[Report_GetBigWinPlayersByID]", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public List<Notification> GetNotification(string ip = "")
        {
            try
            {
                List<Notification> d = new List<Notification>();
                DBHelper db = new DBHelper(ConnectionString.SlotMachineReportConnectionString);
                if (ip == "113.23.109.154" || ip == "")
                {
                    //db = new DBHelper(ConnectionString.GamePortalConnectionString);
                    d = db.GetListSP<Notification>("API_GetNotificationByAdmin");
                    if (d.Count > 0)
                    {
                        NLogManager.LogMessage("All GetBigWinPlayers TEST: " + JsonConvert.SerializeObject(d));
                    }
                }
                return d;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
    }

    public class Notification
    {
        public string Message { get; set; }
    }
}