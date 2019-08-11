using Dapper;
using LuckySpinSanh.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Utilities.Log;
using Utilities.Util;

namespace LuckySpinSanh.Database
{
    public class SpinDAO
    {
        public static string conPortalString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["GamePortal"].ConnectionString);
        public static string connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["LuckySpin"].ConnectionString);
        public static int defaultChance = Convert.ToInt32(ConfigurationManager.AppSettings["DefaultChance"]);

        public static List<SmallSpinConfig> GetSmallSpinConfig(byte flow)
        {
            var db = new DBHelper(connectionString);

            return db.GetList<SmallSpinConfig>($"SELECT Id, Price, Code, StartValue{flow} StartValue, EndValue{flow} EndValue, Quantity{flow} Quantity, Description FROM [dbo].[SmallSpinConfig] WITH (NOLOCK)");
        }

        public static List<BigSpinConfig> GetBigSpinConfig(byte flow)
        {
            var db = new DBHelper(connectionString);

            return db.GetList<BigSpinConfig>($"SELECT Id, Price, Code, StartValue{flow} StartValue, EndValue{flow} EndValue, Quantity{flow} Quantity, Description FROM [dbo].[BigSpinConfig] WITH (NOLOCK)");
        }

        public static int PrizeCounterByDate(int prizeCode, int date, bool isCoinPrize)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                if (isCoinPrize)
                {
                    var query = connection.QueryFirstOrDefault($"SELECT [Total] FROM [dbo].[BigExported] where Code = {prizeCode} and DateInt = {date}");
                    if (query == null)
                        return 0;
                    else return Convert.ToInt32(query.Total);
                }
                else
                {
                    var query = connection.QueryFirstOrDefault($"SELECT [Total] FROM [dbo].[SmallExported] where Code = {prizeCode} and DateInt = {date}");
                    if (query == null)
                        return 0;
                    else return Convert.ToInt32(query.Total);
                }
            }
        }

        public static int GetSpinChancePerDay(int date)
        {
            var db = new DBHelper(connectionString);
            var extra = db.GetInstance<ExtraSpin>($"SELECT SpinChance FROM [dbo].[ExtraSpin] WHERE FromDate <= {date} AND ToDate >= {date}");

            if (extra == null)
                return defaultChance;

            return extra.SpinChance;
        }

        /// <summary>
        /// Số lần đã quay trong ngày
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetSpin(long accountId, int date, int spinChance, bool getOnly = true)
        {
            var db = new DBHelper(connectionString);

            var pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@_AccountId", accountId));
            pars.Add(new SqlParameter("@_Time", date));
            pars.Add(new SqlParameter("@_SpinChance", spinChance));
            pars.Add(new SqlParameter("@_Response", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output });
            pars.Add(new SqlParameter("@_GetOnly", getOnly));

            db.ExecuteNonQuerySP("[SP_GetSpin]", pars.ToArray());

            return Convert.ToInt32(pars[3].Value);
        }

        public static bool Award(long accountId, int smallResult, int bigResult, long sessionId, out long gold)
        {
            try
            {
                var db = new DBHelper(connectionString);

                var _params = new List<SqlParameter>
                {
                    new SqlParameter("@_AccountId", accountId),
                    new SqlParameter("@_SmallResult", smallResult),
                    new SqlParameter("@_BigResult", bigResult),
                    new SqlParameter("@_SessionId", sessionId),
                    new SqlParameter("@_Gold", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
                    new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
                };

                db.ExecuteNonQuerySP("[dbo].[SP_Award]", _params.ToArray());
                gold = Convert.ToInt64(_params[4].Value);
                return Convert.ToInt32(_params[5].Value) > 0;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            gold = -1;
            return false;
        }

        public static bool LogSession(long accountId, int smallResult, int bigResult, out long sessionId)
        {
            sessionId = 0;

            try
            {
                var db = new DBHelper(connectionString);

                var _params = new List<SqlParameter>
                {
                    new SqlParameter("@_AccountId", accountId),
                    new SqlParameter("@_SmallResult", smallResult),
                    new SqlParameter("@_BigResult", bigResult),
                    new SqlParameter("@_SessionId", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
                    new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
                };
                db.ExecuteNonQuerySP("[dbo].[LP_LogSession]", _params.ToArray());

                sessionId = Convert.ToInt64(_params[3].Value);
                return Convert.ToInt32(_params[4].Value) > 0;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        public static List<SpinLog> GetHistory(long accountId, int page, int itemPerPage)
        {
            try
            {
                var db = new DBHelper(connectionString);
                return db.GetList<SpinLog>($"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY [CreatedTime] DESC) AS RowNum, *"
               + " FROM [LuckySpin].[dbo].[LuckySpin.Log]) AS RowConstrainedResult"
               + $" WHERE RowNum >= {(page - 1) * itemPerPage + 1} AND RowNum <= {(page * itemPerPage)} AND AccountId = {accountId}"
               + " ORDER BY RowNum");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return new List<SpinLog>();
        }

        public static int GetRecentTopupCard(long accountId)
        {
            var db = new DBHelper(conPortalString);

            var pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_Amount", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
            };

            db.ExecuteNonQuerySP("[SP_GetRecentTopupCard]", pars.ToArray());

            return Convert.ToInt32(pars[1].Value);
        }
    }
}