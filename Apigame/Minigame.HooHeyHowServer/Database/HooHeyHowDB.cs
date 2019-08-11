using Minigame.HooHeyHowServer.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Utilities.Log;
using Utilities.Util;

namespace Minigame.HooHeyHowServer.Database
{
    public class HooHeyHowDB
    {
        private static Lazy<HooHeyHowDB> _instance = new Lazy<HooHeyHowDB>(() => new HooHeyHowDB());

        public static HooHeyHowDB Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private string connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);

        public DataTable GetTransactionHistory(long accountId, int moneyType)
        {

            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                List<SqlParameter> parsList = new List<SqlParameter>
                {
                    new SqlParameter("@_AccountID", accountId),
                    new SqlParameter("@_MoneyType", moneyType)
                };
                return db.GetDataTableSP("HHH_TransactionHistory", parsList.ToArray());
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public List<Rank> GetRank(int moneyType)
        {

            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                List<SqlParameter> parsList = new List<SqlParameter>
                {
                    new SqlParameter("@_MoneyType", moneyType)
                };
                return db.GetListSP<Rank>("HHH_Rank", parsList.ToArray());
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public List<GameResult> GetRecentResult()
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                return db.GetListSP<GameResult>("HHH_History");
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }
    }
}