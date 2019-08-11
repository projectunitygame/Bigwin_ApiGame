using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dapper;
using Utilities.Log;
using Utilities.Util;

namespace Minigame.HooHeyHowServer.Models.Database
{
    public class GameDAO
    {
        private static readonly string _cons = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["DB"]?.ConnectionString);

        public static async Task ExecuteAsync(string query)
        {
            using (var sqlConnection = new SqlConnection(_cons))
            {
                await sqlConnection.ExecuteAsync(query);
            }
        }

        public static long GetFund(MoneyType moneyType)
        {
            using (var sqlConnection = new SqlConnection(_cons))
            {
                if (moneyType == MoneyType.GOLD)
                {
                    var queryResult = sqlConnection.QueryFirstOrDefault("select Fund from dbo.GoldFund");
                    return Convert.ToInt64(queryResult.Fund);
                }
                else
                {
                    var queryResult = sqlConnection.QueryFirstOrDefault("select Fund from dbo.CoinFund");
                    return Convert.ToInt64(queryResult.Fund);
                }
            }
        }

        public static long CreateSession()
        {
            try
            {
                using(var sqlConnection = new SqlConnection(_cons))
                {
                    var queryResult = sqlConnection.QueryFirstOrDefault("insert into [HooHeyHow].[dbo].[Session] (CreatedTime) VALUES (GETDATE()); SELECT @@IDENTITY SessionId;");
                    if (queryResult != null)
                        return Convert.ToInt64(queryResult.SessionId);
                }
            }catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        public static long Bet(long sessionId, long accountId, string accountName, string gate, long amount, int betType)
        {
            using (var sqlConnection = new SqlConnection(_cons))
            {
                var queryResult = sqlConnection.QueryFirstOrDefault($"exec sp_bet @_AccountId  = {accountId}, @_AccountName = N'{accountName}', @_Gate = N'{gate}', @_Amount = {amount}, @_BetType = {betType}, @_SessionId = {sessionId}");
                if(queryResult != null)
                {
                    return Convert.ToInt64(queryResult.Response);
                }
            }
            return -99;
        }
    }
}