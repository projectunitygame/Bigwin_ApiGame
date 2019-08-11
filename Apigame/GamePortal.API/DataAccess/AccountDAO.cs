using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Utilities.Log;
using Dapper;

namespace GamePortal.API.DataAccess
{
    public class AccountDAO
    {
        /// <summary>
        /// Chuyển tien từ Uwin qua game khác
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="reason"></param>
        /// <param name="amount"></param>
        /// <param name="ip"></param>
        /// <param name="gameID"></param>
        /// <param name="msg"></param>
        /// <param name="receiptID"></param>
        /// <returns></returns>
        public static int TransferSubMoneyGames(long accountID, string reason, long amount, string ip, int gameID, ref string msg, ref string receiptID)
        {
            int code = -1;
            string s = "TransferSubMoneyGames" +
                    "\r\nAccountId: " + accountID +
                    "\r\nReason: " + reason +
                    "\r\nAmount: " + amount +
                    "\r\nIp: " + ip +
                    "\r\nGameID: " + gameID;
            try
            {
                DBHelper db = new DBHelper(GateConfig.DbConfig);
                List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@AccountId", accountID),
                new SqlParameter("@Reason", reason),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@Ip", ip),
                new SqlParameter("@GameID", gameID),
                new SqlParameter("@Code", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@Msg", System.Data.SqlDbType.NVarChar, 500) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@ReceiptID", System.Data.SqlDbType.VarChar, 30) { Direction = System.Data.ParameterDirection.Output }
            };
                db.ExecuteNonQuerySP("API_Transfer_SubMoney_Games", pars.ToArray());
                s += "\r\nCode: " + pars[5].Value;
                s += "\r\nMsg: " + pars[6].Value;
                s += "\r\nReceiptID: " + pars[7].Value;
                code = Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                s += "\r\nERROR: " + ex;
                code = -99;
            }
            finally
            {
                NLogManager.LogMessage(s);
            }
            return code;
        }

        /// <summary>
        /// Chuyển tiền từ game khác sang Uwin
        /// </summary>
        /// <param name="transactionID"></param>
        /// <param name="accountID"></param>
        /// <param name="reason"></param>
        /// <param name="amount"></param>
        /// <param name="ip"></param>
        /// <param name="gameID"></param>
        /// <param name="msg"></param>
        /// <param name="receiptID"></param>
        /// <returns></returns>
        public static long TransferAddMoneyGames(string transactionID, int accountID, string reason, long amount, string ip, int gameID, ref string msg, ref string receiptID)
        {
            int code = -1;
            string s = "TransferAddMoneyGames" +
                    "\r\ntransactionID: " + transactionID +
                    "\r\nAccountId: " + accountID +
                    "\r\nReason: " + reason +
                    "\r\nAmount: " + amount +
                    "\r\nIp: " + ip +
                    "\r\nGameID: " + gameID;
            try
            {
                DBHelper db = new DBHelper(GateConfig.DbConfig);
                List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@TransactionID", transactionID),
                new SqlParameter("@AccountId", accountID),
                new SqlParameter("@Reason", reason),
                new SqlParameter("@Amount", amount),
                new SqlParameter("@Ip", ip),
                new SqlParameter("@GameID", gameID),
                new SqlParameter("@Code", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@Msg", System.Data.SqlDbType.NVarChar, 500) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@ReceiptID", System.Data.SqlDbType.VarChar, 30) { Direction = System.Data.ParameterDirection.Output }
            };
                db.ExecuteNonQuerySP("API_Transfer_AddMoney_Games", pars.ToArray());
                s += "\r\nCode: " + pars[5].Value;
                s += "\r\nMsg: " + pars[6].Value;
                s += "\r\nReceiptID: " + pars[7].Value;
                code = Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                s += "\r\nERROR: " + ex;
                code = -99;
            }
            finally
            {
                NLogManager.LogMessage(s);
            }
            return code;
        }

        public static long CreateNormalAccount(string username, string password, int avatarId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_username", username),
                new SqlParameter("@_password", password),
                new SqlParameter("@_accountId", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@_response", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@_avatarId", avatarId)
            };

            db.ExecuteNonQuerySP("SP_RegisterNormalAccount", pars.ToArray());

            int response = Convert.ToInt32(pars[3].Value);

            if (response < 0)
                return response;

            return Convert.ToInt64(pars[2].Value);
        }

        public static long CreateFacebookAccount(string userId, int avatarId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_username", userId),
                new SqlParameter("@_accountId", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
                new SqlParameter("@_avatarId", avatarId)
            };

            db.ExecuteNonQuerySP("SP_RegisterFacebookAccount", pars.ToArray());

            return Convert.ToInt64(pars[1].Value);
        }

        public static int UpdateDisplayName(long accountId, string displayName)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_accountId", accountId),
                new SqlParameter("@_displayName", displayName),
                new SqlParameter("@_response", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
            };
            db.ExecuteNonQuerySP("SP_UpdateDisplayName", pars.ToArray());

            int response = Convert.ToInt32(pars[2].Value);
            return response;
        }

        public static Account Login(string username, string password)
        {
            //NLogManager.LogMessage($"{GateConfig.DbConfig}");
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<Account>($"select * from dbo.Account where Username = '{username}' and Password = '{password}' and UserType IN (1, 3, 4, 5)");
        }

        public static Account GetAccountInfo(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<Account>($"select * from dbo.Account where AccountId = {accountId}");
        }

        public static Account LoginFB(string username)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<Account>($"select * from dbo.Account where Username = '{username}' and UserType = 2");
        }

        public static long CheckBussinessAccount(string facebookId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            try
            {
                List<SqlParameter> param = new List<SqlParameter>();
                param.Add(new SqlParameter("@_FacebookID", facebookId));

                SqlParameter response = new SqlParameter("@_AccountID", System.Data.SqlDbType.BigInt)
                {
                    Direction = System.Data.ParameterDirection.Output
                };

                param.Add(response);

                db.ExecuteNonQuerySP("[dbo].[SP_CheckBussiness]", param.ToArray());

                return Convert.ToInt64(response.Value);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

            return 0;

        }

        public static void UpdateAvatar(long accountId, int id)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            db.ExecuteNonQuery($"update dbo.Account set AvatarID = {id} where AccountID = {accountId}");
        }

        public static Account GetAccountById(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetInstance<Account>($"SELECT * from dbo.Account where AccountID = {accountId}");
        }

        public static Account GetAccountByAccountName(string name)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetInstance<Account>($"SELECT * from dbo.Account where [DisplayName] = '{name}'");
        }

        public static Account GetAccountByUsername(string name)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetInstance<Account>($"SELECT * from dbo.Account where [Username] = '{name}'");
        }

        public static dynamic GetAgencyInfo(long gameId)
        {
            using (var connection = new SqlConnection(GateConfig.DbConfig))
            {
                return connection.QueryFirstOrDefault($"select * from ag.Account where GameAccountId = {gameId}");
            }
        }

        public static List<VIPRewardLog> CheckVIP(long accountId, int vp, out int rank)
        {
            rank = 0;
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            try
            {
                List<SqlParameter> param = new List<SqlParameter>
                {
                    new SqlParameter("@_AccountID", accountId),
                    new SqlParameter("@_VP", vp)
                };

                SqlParameter response = new SqlParameter("@_Rank", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
                param.Add(response);

                var data = db.GetListSP<VIPRewardLog>("[dbo].[VIP_CheckVIP]", param.ToArray());
                rank = Convert.ToInt32(response.Value);
                return data;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
            return new List<VIPRewardLog>();
        }

        public static List<VIPRankConfig> GetVIPRankConfig()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<VIPRankConfig>("select * from [dbo].[VIPRankConfig]");
        }

        public static long ReceiveReward(int rank, long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            try
            {
                List<SqlParameter> param = new List<SqlParameter>
                {
                    new SqlParameter("@_AccountID", accountId),
                    new SqlParameter("@_Rank", rank)
                };

                SqlParameter response = new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.BigInt)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                param.Add(response);
                db.ExecuteNonQuerySP("[dbo].[SP_VIP_ReceiveReward]", param.ToArray());

                return Convert.ToInt64(response.Value);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }

            return -99;
        }
    }
}