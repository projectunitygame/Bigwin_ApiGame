using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Log;
using Dapper;
using Game.Events.Database.DTO;
using System.Threading.Tasks;
using Utilities.Util;

namespace Game.Events.Database.DAOImpl
{
    public class LuckyDiceEventDAO
    {
        private static string _connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["LuckyDiceConnectionString"].ConnectionString);

        public static bool checkEvent()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = connection.QueryFirstOrDefault("select [event].[BetKingCheck]() status");
                    if (query != null)
                    {
                        return Convert.ToBoolean(query.status);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return false;
        }

        public static dynamic getAccountEvent(string id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault($"select TotalWin, TotalLose, MaxWin, MaxLose from [event].[BetKing] where id = {id}");
            }
        }

        public static BetKingTime GetTimeEvent()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.QueryFirstOrDefault<BetKingTime>($"select * from [event].[BetKing.Config]");
            }
        }


        public static async Task ExecuteSql(string sql)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.ExecuteAsync(sql);
            }
        }

        public static IEnumerable<BetKingTime> GetSuccessRewardDay()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<BetKingTime>($"select * from [event].[SuccessRewardDay]");
            }
        }

        public static IEnumerable<Game.Events.Database.DTO.LuckydiceRank> getRankEvent(string id, int type)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<Game.Events.Database.DTO.LuckydiceRank>($"exec [event].[GetRank] @_day = {id}, @_type = {type}");
            }
        }
    }
}
