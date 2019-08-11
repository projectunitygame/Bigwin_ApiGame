using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Utilities.Database;
using Utilities.Log;
using Dapper;
using PTCN.CrossPlatform.Minigame.LuckyDice.Handlers.BotHandler;
using Utilities.Util;
using Newtonsoft.Json;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Database
{
    public class Lddb
	{
		private static Lazy<Lddb> _instance = new Lazy<Lddb>(() => new Lddb());

		public static Lddb Instance
		{
			get
			{
				return _instance.Value;
			}
		}

		private string connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["LuckyDiceDB"].ConnectionString);
	    private string botConnectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["LuckyDiceBotDB"].ConnectionString);
        public CreatedSession CreateSession(int moneyType)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                List<SqlParameter> parsList = new List<SqlParameter>();

                parsList.Add(new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                });

                parsList.Add(new SqlParameter("@_SessionId", System.Data.SqlDbType.BigInt)
                {
                    Direction = System.Data.ParameterDirection.Output
                });

                parsList.Add(new SqlParameter("@_DateStatus", System.Data.SqlDbType.Bit)
                {
                    Direction = System.Data.ParameterDirection.Output
                });

                parsList.Add(new SqlParameter("@_MoneyType", moneyType));

                db.ExecuteNonQuerySP("LD_CreateSession", parsList.ToArray());

                if (Convert.ToInt32(parsList[0].Value) < 0)
                {
                    return null;
                }

                return new CreatedSession
                {
                    SessionId = Convert.ToInt64(parsList[1].Value),
                    DateStatus = Convert.ToBoolean(parsList[2].Value),
                };

            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }


        public int Bet(long sessionId, long accountId, string accountName, string clientIp, int betSide, long amount, out long newBalance, out long logId, bool addLogSumary, ref long logSumId, bool botBet = false, long betKingId = 0)
		{

			//   @_SessionID BIGINT,
			//   @_AccountID BIGINT,
			//   @_ClientIP NVARCHAR(20),
			//   @_BetSide INT,
			//   @_Amount BIGINT,
			//   @_ResponseStatus INT OUTPUT,
			//   @_Balance BIGINT OUTPUT
			newBalance = 0;
            logId = -1;

            DBHelper db = null;
			try
			{
				db = new DBHelper(connectionString);
				List<SqlParameter> parsList = new List<SqlParameter>();


				parsList.Add(new SqlParameter("@_SessionID", sessionId));
				parsList.Add(new SqlParameter("@_AccountID", accountId));
				parsList.Add(new SqlParameter("@_ClientIP", clientIp));
				parsList.Add(new SqlParameter("@_BetSide", betSide));
				parsList.Add(new SqlParameter("@_Amount", amount));
                parsList.Add(new SqlParameter("@_AccountName", accountName));
                parsList.Add(new SqlParameter("@_BetKingId", betKingId));

                var paramResponse = new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.Int)
				{
					Direction = System.Data.ParameterDirection.Output
				};

				parsList.Add(paramResponse);

				var paramBalance = new SqlParameter("@_Balance", System.Data.SqlDbType.BigInt)
				{
					Direction = System.Data.ParameterDirection.Output
				};

				parsList.Add(paramBalance);

                parsList.Add(new SqlParameter("@_BotBet", botBet));

                var paramLog = new SqlParameter("@_LogReference", System.Data.SqlDbType.BigInt)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                parsList.Add(paramLog);

                parsList.Add(new SqlParameter("@_InsertLogSumary", addLogSumary));

                var paramLog1 = new SqlParameter("@_LogSummaryId", System.Data.SqlDbType.BigInt)
                {
                    Direction = System.Data.ParameterDirection.InputOutput,
                    Value = logSumId
                };

                parsList.Add(paramLog1);

                db.ExecuteNonQuerySP("LD_Bet", parsList.ToArray());

				if (Convert.ToInt32(paramResponse.Value) < 0)
				{
					return Convert.ToInt32(paramResponse.Value);
				}else
				{
					newBalance = Convert.ToInt64(paramBalance.Value);
                    logId = Convert.ToInt64(paramLog.Value);
                    if(addLogSumary)
                        logSumId = Convert.ToInt64(paramLog1.Value);
                }

				return Convert.ToInt32(paramResponse.Value);
			}
			catch (Exception e)
			{
				NLogManager.PublishException(e);
				return -999;    //error
			}
			finally
			{
				if (db != null)
				{
					db.Close();
				}
			}
		}

	 //   [dbo].[LD_FinishSession]
	 //   @_SessionID BIGINT,
	 //   @_FirstDice INT,
		//@_SecondDice INT,
	 //   @_ThirdDice INT,
		//@_ResponseStatus INT OUTPUT

        
        public bool FinishSession(string query)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                db.ExecuteNonQuery(query);
                return true;
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return false;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public List<DiceResult> GetHistory(int moneyType)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                return db.GetListSP<DiceResult>("LD_History", new SqlParameter("@_MoneyType", moneyType));
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
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

                return db.GetListSP<Rank>("LD_Rank", new SqlParameter("@_MoneyType", moneyType));
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public DiceResult GetSessionResultInfo(long sessionId)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                List<SqlParameter> parsList = new List<SqlParameter>();


                parsList.Add(new SqlParameter("@_SessionID", sessionId));

                return db.GetInstanceSP<DiceResult>("LD_SessionResultInfo", parsList.ToArray());
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public List<BetResultInfo> GetSessionBetInfo(long sessionId)
        {

            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                List<SqlParameter> parsList = new List<SqlParameter>();


                parsList.Add(new SqlParameter("@_SessionID", sessionId));

                return db.GetListSP<BetResultInfo>("LD_SessionInfo", parsList.ToArray());
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public DataTable GetTransactionHistory(long accountId, int moneyType)
        {

            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                List<SqlParameter> parsList = new List<SqlParameter>();


                parsList.Add(new SqlParameter("@_AccountID", accountId));
                parsList.Add(new SqlParameter("@_MoneyType", moneyType));
                return db.GetDataTableSP("LD_TransactionHistory", parsList.ToArray());
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public List<BotConfig> GetBotConfig()
        {

            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);

                return db.GetListSP<BotConfig>("LDB_GetConfig");
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return null;    //error
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public long AddRecentBetting(long amount, long accountId)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                List<SqlParameter> pars = new List<SqlParameter>();
                pars.Add(new SqlParameter("@_amount", amount));
                pars.Add(new SqlParameter("@_AccountId", accountId));
                pars.Add(new SqlParameter("@_recentBetting", SqlDbType.BigInt) { Direction = ParameterDirection.Output });
                db.ExecuteNonQuerySP("LD_AddRecentBetting", pars.ToArray());
                return Convert.ToInt64(pars[2].Value);
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

            return -1;
        }

        public long GetRecentBetting(long accountId)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                List<SqlParameter> pars = new List<SqlParameter>();
                pars.Add(new SqlParameter("@_recentBetting", SqlDbType.BigInt) { Direction = ParameterDirection.Output, Value = 0 });
                db.ExecuteNonQuery($"select @_recentBetting = bet from dbo.RecentBetting where accountId = {accountId}", pars.ToArray());
                return Convert.ToInt64(pars[0].Value);
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

            return -1;
        }

        public List<BannedUser> GetBannedUser()
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                return  db.GetList<BannedUser>("select * from dbo.ChatBanned where Locked = 1");
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

            return new List<BannedUser>();
        }

        public void BanUser(string username)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                db.ExecuteNonQuerySP("SP_BanUser", new SqlParameter("@_AccountName", username));
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

        }

        public void UnbanUser(string username)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(connectionString);
                db.ExecuteNonQuery($"update dbo.ChatBanned set Locked = 0 where AccountName = '{username}'");
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public bool IsEventBetKing()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = connection.QueryFirstOrDefault("select [event].[BetKingCheck]() status");
                    if(query != null)
                    {
                        return Convert.ToBoolean(query.status);
                    }
                }
            }catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return false;
        }

	    public List<BotData> GetAllBotName()
	    {
	        try
	        {
	            var _botList = new List<BotData>();
                var db = new DBHelper(botConnectionString);
	            return db.GetListSP<BotData>("SP_GetAllBotName");
	        }
	        catch (Exception ex)
	        {
	            NLogManager.PublishException(ex);
                return new List<BotData>();
	        }
	    }

	    public int UpdateFund(long sessionId, long amount, out long fund)
	    {
	        try
	        {
                var db = new DBHelper(botConnectionString);
	            var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_SessionId", sessionId);
                pars[1] = new SqlParameter("@_Amount", amount);
                pars[2] = new SqlParameter("@_Fund", SqlDbType.BigInt) {Direction = ParameterDirection.Output};
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction = ParameterDirection.Output};
	            db.ExecuteNonQuerySP("[dbo].[LDB_FundChange]", pars);
	            long.TryParse(pars[2].Value.ToString(), out fund);
                NLogManager.LogMessage($"DBFundChange: SessionId:{sessionId}|Amount:{amount}|Fund:{pars[2].Value}|Response:{pars[3].Value}");
	            return int.Parse(pars[3].Value.ToString());
	        }
	        catch (Exception e)
	        {
	            NLogManager.PublishException(e);
	            fund = -1;
	            return -88;
	        }
	    }

	    public FundConfig GetFund()
	    {
	        try
	        {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_BotProfit", SqlDbType.Int) {Direction =  ParameterDirection.Output};
	            var db = new DBHelper(botConnectionString);
	            return db.GetInstanceSP<FundConfig>("SP_GetFundInfo", pars);
	        }
	        catch (Exception e)
	        {
	            NLogManager.PublishException(e);
	            return null;
	        }
        }

	    public BotConfiguration GetBotConfigs()
	    {
	        try
	        {
	            var db = new DBHelper(botConnectionString);
	            return db.GetInstanceSP<BotConfiguration>("SP_GetBotConfig");
	        }
	        catch (Exception e)
	        {
	            NLogManager.PublishException(e);
	            return null;
	        }
        }

	    public List<BetData> GetBetData()
	    {
	        try
	        {
	            var db = new DBHelper(botConnectionString);
	            return db.GetListSP<BetData>("SP_GetBetData");
	        }
	        catch (Exception e)
	        {
	            NLogManager.PublishException(e);
	            return null;
	        }
        }

        public Account GetAccountInfo(long accountId)
        {
            DBHelper db = new DBHelper(connectionString);

            return db.GetInstance<Account>($"select AccountID, Username, DisplayName, IsUpdateAccountName, AvatarID, Gold, Coin, CreatedTime, Tel, IsOTP from GamePortal.dbo.Account where AccountId = {accountId}");
        }
    }

    public class Account
    {
        public long AccountID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int AvatarID { get; set; }
        public long Gold { get; set; }
        public long Coin { get; set; }
        public bool IsUpdateAccountName
        {
            get;
            set;
        }
        public string Tel { get; set; }
        public bool IsOTP { get; set; }
        public DateTime CreatedTime { get; set; }
        [JsonIgnore]
        public bool IsBlocked { get; set; }
        [JsonIgnore]
        public bool IsAgency { get; set; }
    }
}