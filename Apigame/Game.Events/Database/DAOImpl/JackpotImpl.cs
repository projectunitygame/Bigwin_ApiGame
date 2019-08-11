using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.Events.Database.DAO;
using Game.Events.Database.DTO;
using Game.Events.Models;
using Utilities.Database;
using Utilities.Log;

namespace Game.Events.Database.DAOImpl
{
    public class JackpotImpl : IJackpot
    {
        public List<Jackpot> GetAllJackpot()
        {
           try
           {
               var db = new DBHelper(ConnectionString.SlotMachineReportConnectionString);
               return db.GetListSP<Jackpot>("Report_GetAllJackpot");
           }
           catch(Exception ex)
           {
               NLogManager.PublishException(ex);
               return null;
           }
        }
    }
}