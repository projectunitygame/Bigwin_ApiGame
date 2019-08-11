using GamePortal.API.Models;
using GamePortal.API.Models.Topup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Dapper;
using Utilities.Log;

namespace GamePortal.API.DataAccess
{
    public class TransactionDAO
    {
        #region giftcode
        public static GiftcodeBalance InputCode(long accountId, string accountName, string code)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_AccountName", accountName),
                new SqlParameter("@_Code", code)
            };
            return db.GetInstanceSP<GiftcodeBalance>("SP_InputGiftcode", pars.ToArray());
        }
        #endregion
        #region exchange coin
        public static AccountBalance ExchangeCoin(long accountId, long amount)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@_AccountId", accountId));
            pars.Add(new SqlParameter("@_Amount", amount));
            return db.GetInstanceSP<AccountBalance>("SP_ExchangeCoin", pars.ToArray());
        }
        #endregion
        #region Topup

        public static long TopupCard(string transactionId, long accountId, string accountName, long amount, long exchangeBalance, 
            int status, string pin, string serial, int deviceType, string description, int cardType)
        {
            var dbHelper = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@_transactionId", transactionId));
            pars.Add(new SqlParameter("@_accountId", accountId));
            pars.Add(new SqlParameter("@_accountName", accountName));
            pars.Add(new SqlParameter("@_amount", amount));
            pars.Add(new SqlParameter("@_exchangeBalance", exchangeBalance));
            pars.Add(new SqlParameter("@_status", status));
            pars.Add(new SqlParameter("@_pin", pin));
            pars.Add(new SqlParameter("@_serial", serial));
            pars.Add(new SqlParameter("@_deviceType", deviceType));
            pars.Add(new SqlParameter("@_description", description));
            pars.Add(new SqlParameter("@_cardType", cardType));
            pars.Add(new SqlParameter("@_balance", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output});

            dbHelper.ExecuteNonQuerySP("SP_TopupCard", pars.ToArray());

            return Convert.ToInt64(pars[11].Value);
        }

        public static long RetopupCard(string transactionId, long amount, long exchangeBalance,
    int status)
        {
            var dbHelper = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@_transactionId", transactionId));
            pars.Add(new SqlParameter("@_amount", amount));
            pars.Add(new SqlParameter("@_exchangeBalance", exchangeBalance));
            pars.Add(new SqlParameter("@_status", status));
            pars.Add(new SqlParameter("@_balance", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output });

            dbHelper.ExecuteNonQuerySP("SP_Retopup", pars.ToArray());

            return Convert.ToInt64(pars[4].Value);
        }

        public static List<TopupType> GetTopupTypes()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetListSP<TopupType>("select TopupType Type, Name from cfg.TopupType where Enable = 1");
        }

        public static List<CardType> GetCardTypes()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetListSP<CardType>("select CardType Type, Name, ShortCode from cfg.CardType where Enable = 1");
        }

        public static List<CardTopup> GetListCardPrices()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetListSP<CardTopup>("select CardType, Value, Bonus, GoldValue from cfg.CardTopup where Enable = 1");
        }
        #endregion
        #region History
        //public static List<GameGoldTransaction> GetGameGoldTransaction(long accountId, int records)
        //{
        //    DBHelper db = new DBHelper(GateConfig.DbConfig);
        //    return db.GetList<GameGoldTransaction>($"SELECT TOP({records}) * FROM log.V_GameGoldTransaction where AccountId = {accountId} ORDER BY CreatedTime DESC");
        //}

        public static List<GameGoldTransaction> GetGameGoldTransaction_v1(long accountId, int records)
        {
            NLogManager.LogMessage("GetGameGoldTransaction_v1: " + accountId + ", " + records);
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<GameGoldTransaction>($"SELECT TOP({records}) * FROM log.V_GameGoldTransaction_v1 where AccountId = {accountId} ORDER BY CreatedTime DESC");
        }

        public static List<TopupGold> GetTopupGold(long accountId, int records)
        {
            NLogManager.LogMessage("GetTopupGold: " + accountId + ", " + records);
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<TopupGold>($"SELECT TOP({records}) * FROM log.V_TopupGold where AccountId = {accountId} ORDER BY CreatedTime DESC");
        }

        public static List<DeductGold> GetDeductGold(long accountId, int records)
        {
            NLogManager.LogMessage("GetDeductGold: " + accountId + ", " + records);
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<DeductGold>($"SELECT TOP({records}) * FROM log.V_DeductGold where AccountId = {accountId} ORDER BY CreatedTime DESC");
        }
        #endregion
        #region send gold
        public static long SendGold(long sendId, long recvId, string sendName, string recvName,
            bool isAgency, long amount, long recvAmount, string reason)
        {
            int agency = isAgency ? 1 : 0;
            using (var db = new SqlConnection(GateConfig.DbConfig))
            {
                var query = db.QueryFirstOrDefault($"exec [dbo].[SP_SendGold] @_sendId = {sendId}, " +
                    $"@_recvId = {recvId}, " +
                    $"@_sendName = N'{sendName}', " +
                    $"@_recvName = N'{recvName}', " +
                    $"@_isAgency = {agency}, " +
                    $"@_amount = {amount}, " +
                    $"@_recvAmount = {recvAmount}, " +
                    $"@_reason = N'{reason}'");

                return Convert.ToInt64(query.response);
            }
        }

        /// <summary>
        /// Chuyển tiền cho đại lý - truongdien
        /// </summary>
        /// <param name="sendId"></param>
        /// <param name="recvId"></param>
        /// <param name="sendName"></param>
        /// <param name="recvName"></param>
        /// <param name="isAgency"></param>
        /// <param name="amount"></param>
        /// <param name="recvAmount"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static ResultTransferToAgency SendGold_v1(long sendId, string recvName,
            decimal amount, string reason, string ip,ref int code, ref string msg, ref string phone)
        {
            try
            {
                using (var db = new SqlConnection(GateConfig.DbConfig))
                {
                    string key = "48dc695d5f6203fd54b85c93b022ffe5";
                    var dbHelper = new DBHelper(GateConfig.DbConfig);
                    List<SqlParameter> pars = new List<SqlParameter>();
                    pars.Add(new SqlParameter("@SenderID", sendId));
                    pars.Add(new SqlParameter("@UwinID", recvName));
                    pars.Add(new SqlParameter("@Reason", reason));
                    pars.Add(new SqlParameter("@Amount", amount));
                    pars.Add(new SqlParameter("@IP", ip));
                    pars.Add(new SqlParameter("@Key", key));
                    pars.Add(new SqlParameter("@Code", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output });
                    pars.Add(new SqlParameter("@Msg", System.Data.SqlDbType.NVarChar, 500) { Direction = System.Data.ParameterDirection.Output });
                    pars.Add(new SqlParameter("@PhoneAgency", System.Data.SqlDbType.VarChar, 10) { Direction = System.Data.ParameterDirection.Output });
                    var response = dbHelper.GetInstanceSP<ResultTransferToAgency>("[dbo].[API_TransferMoneyUserToAgency]", pars.ToArray());
                    code = Convert.ToInt32(pars[6].Value);
                    msg = pars[7].Value.ToString();
                    phone = pars[8].Value.ToString();
                    return response;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public static long Transfer(int id, long gameAccountId, string gameAccountName,
            long amount, long fee, long level, string description,
            long recvGameAccountId, string recvAccountName, bool recvIsAgency)
                {
                    int agency = recvIsAgency ? 1 : 0;

                    //NLogManager.LogMessage(($"exec ag.CreateTransaction @_id = {id}, @_gameAccountId = {gameAccountId}, " +
                    //        $"@_gameAccountName = '{gameAccountName}', @_amount = {amount}, " +
                    //        $"@_fee = {fee}, @_level = {level}, @_description = N'{description}', " +
                    //        $"@_recvGameAccountName = '{recvAccountName}', @_recvGameAccountId = {recvGameAccountId}, " +
                    //        $"@_recvIsAgency = {agency}"));

                    using (var connection = new SqlConnection(GateConfig.DbConfig))
                    {
                        var result = connection.QueryFirstOrDefault
                            ($"exec ag.CreateTransaction @_id = {id}, @_gameAccountId = {gameAccountId}, " +
                            $"@_gameAccountName = '{gameAccountName}', @_amount = {amount}, " +
                            $"@_fee = {fee}, @_level = {level}, @_description = N'{description}', " +
                            $"@_recvGameAccountName = '{recvAccountName}', @_recvGameAccountId = {recvGameAccountId}, " +
                            $"@_recvIsAgency = {agency}");

                        return Convert.ToInt64(result.balance);
                    }
                }
        #endregion

        public static List<TransferLog> GetTransferLog(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            var query = $"SELECT ID, CreatedTime, RecvName AccountName, SendAmount Amount, 1 Type FROM [log].[TransferTransaction] WHERE SendID = {accountId}"
            + " UNION"
            + $" SELECT ID, CreatedTime, SendName AccountName, RecvAmount Amount, 2 Type FROM [log].[TransferTransaction] WHERE RecvID = {accountId}"
            + " ORDER BY CreatedTime DESC";

            return db.GetList<TransferLog>(query).ToList();
        }

        public static bool DeductGold(long accountId, long amount, string description, int type)
        {
            try
            {
                NLogManager.LogMessage("DeductGold: " + accountId + ", description=" + description + ", type=" + type);
                using (var db = new SqlConnection(GateConfig.DbConfig))
                {
                    var query = db.QueryFirstOrDefault($"declare @_ResponseStatus int;exec [dbo].[SP_DeductGold_OTP] @_AccountId = {accountId}, " +
                        $"@_ServiceId = 0, " +
                        $"@_Amount = {amount}, " +
                        $"@_Description = '{description}', " +
                        $"@_Type = {type}, " +
                        $"@_ResponseStatus = @_ResponseStatus OUTPUT;" +
                        $"select @_ResponseStatus ResponseStatus");

                    return Convert.ToInt64(query.ResponseStatus) > 0;
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogError("ERROR DeductGold: " + ex);
                throw new Exception(ex.Message);
            }
            
        }

        public static IEnumerable<CardConfig> GetCardConfigs()
        {
            using (var db = new SqlConnection(GateConfig.DbConfig))
            {
                return db.Query<CardConfig>("select * from [dbo].[CardConfig]");
            }
        }

        public static IEnumerable<CashoutHistory> GetCashoutHistories(long accountId)
        {
            using (var db = new SqlConnection(GateConfig.DbConfig))
            {
                return db.Query<CashoutHistory>($"select top 100 * from [log].[CashoutCard] where AccountId = {accountId} order by createdTime desc");
            }
        }
        public static TopupHistory GetTopupHistory(string id)
        {
            using (var db = new SqlConnection(GateConfig.DbConfig))
            {
                return db.QueryFirstOrDefault<TopupHistory>($"select top 100 * from [log].[TopupCard] where TransactionID = '{id}' order by createdTime desc");
            }
        }


        public static IEnumerable<TopupHistory> GetTopupHistories(long accountId)
        {
            using (var db = new SqlConnection(GateConfig.DbConfig))
            {
                return db.Query<TopupHistory>($"select top 100 * from [log].[TopupCard] where AccountId = {accountId} order by createdTime desc");
            }
        }


        public static CashoutModel Cashout(long accountId, string accountName, int cardType, int cardAmount, long deductAmount)
        {
            var dbHelper = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("@_AccountId", accountId));
            pars.Add(new SqlParameter("@_AccountName", accountName));
            pars.Add(new SqlParameter("@_CardType", cardType));
            pars.Add(new SqlParameter("@_DeductAmount", deductAmount));
            pars.Add(new SqlParameter("@_Balance", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output});
            pars.Add(new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output});
            pars.Add(new SqlParameter("@_CardAmount", cardAmount));
            var card = dbHelper.GetInstanceSP< MobileCard>("[dbo].[SP_Cashout]", pars.ToArray());
            long response = Convert.ToInt64(pars[5].Value);
            long balance = Convert.ToInt64(pars[4].Value);
            
            return new CashoutModel()
            {
                Status = response, 
                Balance = balance,
                CashoutCard = card
            };
        }


        public static void InsertPayLog(string transactionId, string serial, string cardCode, int status, int cardType, 
            long accountId, string userName, int amount, int payId)
        {
            try
            {
                using (var sql = new SqlConnection(GateConfig.DbConfig))
                {
                    sql.Execute("insert into [dbo].[PayLogDetail] ([TransactionId],[Serial],[CardCode],[Status],[CardType],[AccountId],[AccountName],[CreatedTime],[Amount],[PayId]) " +
                        $"values ('{transactionId}', '{serial}', '{cardCode}', {status}, {cardType}, {accountId}, '{userName}', getdate(), {amount}, {payId})");
                }
            }catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public static void UpdatePayResult(string id, int status, int amount)
        {
            try
            {
                using (var sql = new SqlConnection(GateConfig.DbConfig))
                {
                    sql.Execute($"update [dbo].[PayLogDetail] set status = {status}, amount = {amount} where transactionid = '{id}'");
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
        /*
         SP_IAP_IOS
	@_TransactionID NVARCHAR(150),
	@_AccountID BIGINT,
	@_AccountName NVARCHAR(50)*/
        public static long IAP(string transactionId, long accountId, string accountName)
        {
            try
            {
                using (var sql = new SqlConnection(GateConfig.DbConfig))
                {
                    var response = sql.QueryFirstOrDefault($"exec SP_IAP_IOS @_TransactionID = '{transactionId}', @_AccountID = {accountId}, @_AccountName = '{accountName}'").ResponseStatus;
                }
            }
            catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return -99;
        }
    }
}