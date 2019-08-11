using Game.Events.Database.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Game.Events.Database.DAO;
using System.Data;
using Game.Events.Models;
using Utilities.Database;
using Utilities.Log;

namespace Game.Events.Database.DAOImpl
{
    public class VoLamBigJackpotImpl : IBigJackpotEvent
    {
        public BigJackpotInfo GetBigJackpotInfo()
        {
            try
            {

                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_IsEvent", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                DBHelper db = new DBHelper(ConnectionString.VoLamConnectionString);
                BigJackpotInfo obj = new BigJackpotInfo();
                obj.List = db.GetListSP<BigJackpot>("[dbo].[SP_Event_BigJackpot_GetInfo]", pars);
                obj.IsEvent = pars[0].Value != DBNull.Value ? bool.Parse(pars[0].Value.ToString()) : false;
                return obj;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
        public List<BigJackpotHistory> GetBigJackpotHistory()
        {
            try
            {
                DBHelper db = new DBHelper(ConnectionString.VoLamConnectionString);
                return db.GetListSP<BigJackpotHistory>("[dbo].[SP_Event_BigJackpot_GetEventHistory]");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }

        }
    }
}