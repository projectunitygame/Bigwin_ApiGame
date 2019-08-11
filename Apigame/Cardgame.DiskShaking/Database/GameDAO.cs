using Cardgame.DiskShaking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using System.Data.SqlClient;
using Utilities.Log;
using System.Threading.Tasks;

namespace Cardgame.DiskShaking.Database
{
    public class GameDAO
    {

        public static async Task ExecuteAsync(string query)
        {
            using (var sqlConnection = new SqlConnection(ConfigDB.GameCons))
            {
                await sqlConnection.ExecuteAsync(query);
            }
        }

        public static Player GetPlayerInfo(long accountId)
        {
            using (var db = new SqlConnection(ConfigDB.PortalCons))
            {
                var queryInfo = db.QueryFirstOrDefault($"select Gold, Coin, DisplayName, AvatarID from dbo.Account where AccountID = {accountId}");
                if(queryInfo != null)
                {
                    return new Player(accountId, Convert.ToString(queryInfo.DisplayName), Convert.ToInt64(queryInfo.Gold), Convert.ToInt64(queryInfo.Coin), Convert.ToInt32(queryInfo.AvatarID));
                }
            }

            return null;
        }

        public dynamic BatchBalanceInfo(List<long> accountId)
        {
            string agr = accountId.Select(x => x.ToString()).Aggregate((i, j) => i + ", " + j);
            using(var db = new SqlConnection(ConfigDB.PortalCons))
            {
                var queryInfo = db.Query($"select AccountID, Gold, Coin from dbo.Account where AccountID in ({agr})");
                return queryInfo;
            }
        }

        public static long CreateSession()
        {
            try
            {
                using (var sqlConnection = new SqlConnection(ConfigDB.GameCons))
                {
                    var queryResult = sqlConnection.QueryFirstOrDefault("insert into [dbo].[Session] (Status, CreatedTime) VALUES (0, GETDATE()); SELECT @@IDENTITY SessionId;");
                    if (queryResult != null)
                        return Convert.ToInt64(queryResult.SessionId);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        public static long Bet(long sessionId, long accountId, string accountName, string desciption, long amount, int betType)
        {
            using (var sqlConnection = new SqlConnection(ConfigDB.GameCons))
            {
                NLogManager.LogMessage($"exec sp_bet @_AccountId = {accountId}, @_AccountName = N'{accountName}', @_Description = N'{desciption}', @_Amount = {amount}, @_BetType = {betType}, @_SessionId = {sessionId}");
                var queryResult = sqlConnection.QueryFirstOrDefault($"exec sp_bet @_AccountId = {accountId}, @_AccountName = N'{accountName}', @_Description = N'{desciption}', @_Amount = {amount}, @_BetType = {betType}, @_SessionId = {sessionId}");
                if (queryResult != null)
                {
                    return Convert.ToInt64(queryResult.Response);
                }
            }
            return -99;
        }

        public static long Refund(long sessionId, long accountId, string accountName, string desciption, long amount, int betType)
        {
            using (var sqlConnection = new SqlConnection(ConfigDB.GameCons))
            {
                var queryResult = sqlConnection.QueryFirstOrDefault($"exec SP_Refund @_AccountId = {accountId}, @_AccountName = N'{accountName}', @_Description = N'{desciption}', @_Amount = {amount}, @_BetType = {betType}, @_SessionId = {sessionId}");
                if (queryResult != null)
                {
                    return Convert.ToInt64(queryResult.Response);
                }
            }
            return -99;
        }

        public static long BetWithBanker(long sessionId, long accountId, string accountName, string desciption, long amount, int betType, long bankerId, long bankerLock, string bankerName, string descriptionBanker)
        {
            using (var sqlConnection = new SqlConnection(ConfigDB.GameCons))
            {

                NLogManager.LogMessage($"exec SP_BetSediesWithBanker " +
                    $"@_AccountId  = {accountId}, " +
                    $"@_AccountName = N'{accountName}', " +
                    $"@_Description = N'{desciption}', " +
                    $"@_Amount = {amount}, " +
                    $"@_BetType = {betType}, " +
                    $"@_SessionId = {sessionId}, " +
                    $"@_BankerId = {bankerId}, " +
                    $"@_BankerLock = {bankerLock}, " +
                    $"@_BankerName = '{bankerName}', " +
                    $"@_DescriptionBanker = N'{descriptionBanker}'");


                var queryResult = sqlConnection.QueryFirstOrDefault($"exec SP_BetSediesWithBanker " +
                    $"@_AccountId  = {accountId}, " +
                    $"@_AccountName = N'{accountName}', " +
                    $"@_Description = N'{desciption}', " +
                    $"@_Amount = {amount}, " +
                    $"@_BetType = {betType}, " +
                    $"@_SessionId = {sessionId}, " +
                    $"@_BankerId = {bankerId}, " +
                    $"@_BankerLock = {bankerLock}, " +
                    $"@_BankerName = '{bankerName}', " +
                    $"@_DescriptionBanker = N'{descriptionBanker}'");
                if (queryResult != null)
                {
                    return Convert.ToInt64(queryResult.ResponseStatus);
                }
            }
            return -99;
        }
    }
}