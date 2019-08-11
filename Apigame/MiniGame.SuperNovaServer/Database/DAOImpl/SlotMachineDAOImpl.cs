using Intecom.Software.RDTech.SlotMachine.DataAccess.DAO;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using MiniGame.SuperNovaServer.Models;
using Utilities.Database;
using Utilities.Log;
using Jackpot = MiniGame.SuperNovaServer.Database.DTO.Jackpot;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DAOImpl
{
    public class SlotMachineDAOImpl : ISlotMachineDAO
    {
        /// <summary>
        /// Lấy thông tin cá nhân của TK
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public SlotMachineAccountInfo GetAccountInfo(int inputType, int accountID, string username, int roomId)
        {
            DBHelper db = null;
            try
            {
                int balance = 0;
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_RoomID", roomId);
                pars[3] = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_LastLineData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_LastPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_BetValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("SP_Accounts_GetAccountInfo", pars);

                if (accountID <= 0 && pars[1].Value != null)
                {
                    accountID = Convert.ToInt32(pars[1].Value);
                    username = pars[2].Value.ToString();
                    balance = Convert.ToInt32(pars[8].Value);
                }
                return new SlotMachineAccountInfo
                {
                    AccountID = accountID,
                    UserName = username,
                    Jackpot = Convert.ToInt64(pars[3].Value),
                    LastLinesData = string.Format("{0}", pars[4].Value),
                    LastPrizeValue = Convert.ToInt64(pars[5].Value),
                    ResponseStatus = Convert.ToInt32(pars[8].Value),
                    Balance = Convert.ToInt64(pars[7].Value),
                    BetValue = (int)pars[6].Value
                };
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
            return new SlotMachineAccountInfo
            {
                AccountID = accountID,
                ResponseStatus = -1001
            };
        }

        /// <summary>
        /// Quay
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="accessToken"></param>
        /// <param name="accountId"></param>
        /// <param name="username"></param>
        /// <param name="linesData"></param>
        /// <param name="roomId"></param>
        /// <param name="sourceId"></param>
        /// <param name="clientIp"></param>
        /// <returns></returns>
        public SlotMachineSpinData Spin(MoneyType betType, int gameId,int sourceId, string accessToken, int accountId, string username, string linesData, int roomId,
            string clientIp)
        {
           
            try
            {
                var merchantId = 0;
                var db = new DBHelper(ConnectionString.GameConnectionString);

                var pars = new SqlParameter[19];
                pars[0] = new SqlParameter("@_GameID", gameId);
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_Username", username);
                pars[3] = new SqlParameter("@_RoomID", roomId);
                pars[4] = new SqlParameter("@_SourceID", sourceId);
                pars[5] = new SqlParameter("@_MerchantID", merchantId);
                pars[6] = new SqlParameter("@_ClientIP", clientIp);

                pars[7] = new SqlParameter("@_SpinID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_SlotData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_PrizeData", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_IsJackpot", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                pars[12] = new SqlParameter("@_PositionData", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };

                pars[13] = new SqlParameter("@_TotalBetValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_TotalPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[15] = new SqlParameter("@_PaylinePrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Tong so tien thang
                pars[16] = new SqlParameter("@_PrizeFund", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Quỹ
                pars[17] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//số dư
                pars[18] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };


                var watch = System.Diagnostics.Stopwatch.StartNew();
                string spName = betType == MoneyType.Gold ? "SP_Spins_CreateTransaction" : "SP_Spins_CreateTransaction_Coin";
                db.ExecuteNonQuerySP(spName, pars);
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                var data = new SlotMachineSpinData
                {
                    AccountID = accountId,
                    Balance = Convert.ToInt64(pars[17].Value),
                    IsJackpot = Convert.ToBoolean(pars[10].Value),
                    Jackpot = Convert.ToInt64(pars[11].Value),
                    ResponseStatus = Convert.ToInt32(pars[18].Value),

                    PrizeFund = Convert.ToInt64(pars[16].Value),
                    SpinID = Convert.ToInt64(pars[7].Value),
                    TotalBetValue = Convert.ToInt32(pars[13].Value),
                    TotalPrizeValue = Convert.ToInt64(pars[14].Value),
                    PaylinePrizeValue = Convert.ToInt64(pars[15].Value),
                    SlotsData = pars[8].Value.ToString(),
                    PositionData = pars[12].Value.ToString()
                };

                NLogManager.LogMessage(
                    $"Spins=> BetType: {betType}| Time:{elapsedMs}|Acc:{data.AccountID}|User:{username}|GameId:{gameId}|SourceId:{sourceId}|RoomId:{roomId}|SpinID:{data.SpinID}|Response:{data.ResponseStatus}|TotalPrize:{data.TotalPrizeValue}|IsJackpot:{data.IsJackpot}|Ip:{clientIp}");
                return data;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new SlotMachineSpinData
            {
                AccountID = accountId,
                ResponseStatus = -1001
            };
        }


        /// <summary>
        ///
        /// </summary>
        /// <param name="serviceID"></param>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        public SlotMachineAccountTransactions GetAccountTransactions(int serviceID, int accountID, string username, string accessToken)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_ServiceID", serviceID);
                pars[1] = new SqlParameter("@_AccountID", accountID);
                pars[2] = new SqlParameter("@_AccountName", username);
                pars[3] = new SqlParameter("@_AccessToken", accessToken);
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                var list = db.GetListSP<SlotMachineAccountTransaction>("SP_SlotsKing_GetAccountTransactions", pars);

                return new SlotMachineAccountTransactions
                {
                    AccountID = accountID,
                    Transactions = list,
                    ResponseStatus = Convert.ToInt64(pars[4].Value),
                };
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

            return new SlotMachineAccountTransactions
            {
                ResponseStatus = -1001
            };
        }

        /// <summary>
        /// Lưu các trường hợp Thắng lớn, jackport....
        /// </summary>
        /// <param name="spinId"></param>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="Username"></param>
        /// <param name="Message"></param>
        /// <param name="Icon"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public int CreateNotification(long spinId, long accountId, string accountName, string Username, string Message, string Icon, int Type)
        {
            return -1;
            //DBHelper db = null;
            //try
            //{
            //    int responseStatus = 0;
            //    db = new DBHelper(Config.NotificationConnectionString);
            //    var pars = new SqlParameter[9];
            //    pars[0] = new SqlParameter("@_Name", Username);
            //    pars[1] = new SqlParameter("@_Message", Message);
            //    pars[2] = new SqlParameter("@_Icon", Icon);
            //    pars[3] = new SqlParameter("@_Type", Type);
            //    pars[4] = new SqlParameter("@_Status", 0); // 0 = unread status
            //    pars[5] = new SqlParameter("@_CreatedById", accountId); // 0 = unread status
            //    pars[6] = new SqlParameter("@_CreatedByName", accountName); // 0 = unread status
            //    pars[7] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
            //    pars[8] = new SqlParameter("@_SpinId", spinId);

            //    db.ExecuteNonQuerySP("[SP_Notifications_CreateSystemNotify]", pars);
            //    responseStatus = Int32.Parse(pars[5].Value.ToString());

            //    if (responseStatus < 0)
            //    {
            //        NLogManager.LogError("Execute SP_Notifications_CreateSystemNotify responseStatus = " + responseStatus);
            //    }
            //    return responseStatus;
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.PublishException(ex);
            //}
            //finally
            //{
            //    if (db != null)
            //    {
            //        db.Close();
            //    }
            //}
            //return -999;
        }

        public List<AccountHistory> GetTransactionHistory(long accountid, MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountid);
                pars[1] = new SqlParameter("@_MoneyType", moneyType);
                return db.GetListSP<AccountHistory>("SP_LSGD", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new List<AccountHistory>();
            }
        }

        public List<HonorHistory> GetHonorHistory(MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                var res = db.GetListSP<HonorHistory>("SP_Honor_History", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new List<HonorHistory>();
            }
        }

        public List<HonorHistory> GetJackpotHistory(MoneyType moneyType)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                var res = db.GetListSP<HonorHistory>("SP_Jackpot_History", pars);
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new List<HonorHistory>();
            }
            finally
            {
                if (db != null)
                {
                    db.Close();
                }
            }
        }

        public IEnumerable<Jackpot> GetJackpot(MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[1];

                pars[0] = new SqlParameter("@_MoneyType", moneyType);

                return db.GetListSP<Jackpot>("[dbo].[SP_RoomFunds_GetAllJackpot]", pars);

            }
            catch (Exception ex)
            {
                NLogManager.LogError(ex.Message);
                return null;
            }
        }

        public int InsertSample(string username, string slotsdata)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_SlotData", slotsdata);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var res = db.ExecuteNonQuerySP("SP_Spins_SetSlotsData", pars);
                return Convert.ToInt32(pars[2].Value);
            }
            catch (Exception ex)
            {
                NLogManager.LogError("SP_Spins_SetSlotsData:" + ex.Message);
                return -1;
            }
        }

        // event thu 2 than tai
        public BigJackpotCount GetBigJackpotCount(int roomId)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_RoomID", roomId);
                pars[1] = new SqlParameter("@_Count", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_Event", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_CountBig", SqlDbType.Int) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("BigJackpot_Count", pars);

                return new BigJackpotCount
                {
                    JackpotCount = Convert.ToInt32(pars[3].Value),
                    InEvent = Convert.ToInt32(pars[2].Value)
                };
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
            return new BigJackpotCount
            {
                JackpotCount = -1,
                InEvent = 0
            };
        }
        public ListJackPortHistory GetBigJackpotHistory(int currentPage, int pageSize)
        {
            var data = new ListJackPortHistory();
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_PageIndex", currentPage);
                pars[1] = new SqlParameter("@_PageSize", pageSize);
                pars[2] = new SqlParameter("@_Total", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var res = new DBHelper(ConnectionString.GameConnectionString).GetListSP<JackPortHistory>("BigJackpot_History", pars);

                if (res == null || res.Count <= 0)
                    return data;

                data.ListJackPort = res;
                data.ToTal = Convert.ToInt32(pars[2].Value);
                return data;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return data;
            }
        }

        #region logging

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="parentSessionID"></param>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="lineData"></param>
        /// <param name="totalLine"></param>
        /// <param name="betType"></param>
        /// <param name="betValue"></param>
        /// <param name="totalBetValue"></param>
        /// <param name="totalPrizeValue"></param>
        /// <param name="totalJackpotValue"></param>
        /// <param name="refundValue"></param>
        /// <param name="totalFreeSpins"></param>
        /// <param name="gameType"></param>
        /// <param name="slotsData"></param>
        /// <param name="prizesData"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public int CreateHistory(long sessionID, long parentSessionID, long accountID, string username,
            string lineData, int totalLine, int betType, long betValue, long totalBetValue, long totalPrizeValue, long totalJackpotValue,
            long refundValue, int totalFreeSpins, int gameType, string slotsData, string prizesData, string description)
        {
            DBHelper db = null;
            try
            {
                int responseStatus = 0;
                db = new DBHelper(ConnectionString.GameConnectionString);

                var pars = new SqlParameter[18];
                pars[0] = new SqlParameter("@_SessionID", sessionID);
                pars[1] = new SqlParameter("@_ParentSessionID", parentSessionID);
                pars[2] = new SqlParameter("@_AccountID", accountID);
                pars[3] = new SqlParameter("@_Username", username);
                pars[4] = new SqlParameter("@_TotalLines", totalLine);
                pars[5] = new SqlParameter("@_BetType", betType);
                pars[6] = new SqlParameter("@_BetValue", betValue);
                pars[7] = new SqlParameter("@_TotalBetValue", totalBetValue);
                pars[8] = new SqlParameter("@_TotalPrizeValue", totalPrizeValue);
                pars[9] = new SqlParameter("@_TotalJackpotValue", totalJackpotValue);
                pars[10] = new SqlParameter("@_RefundValue", refundValue);
                pars[11] = new SqlParameter("@_TotalFreeSpins", totalFreeSpins);
                pars[12] = new SqlParameter("@_GameType", gameType);
                pars[13] = new SqlParameter("@_SlotsData", slotsData);
                pars[14] = new SqlParameter("@_PrizesData", prizesData);
                pars[15] = new SqlParameter("@_Description", description);
                pars[16] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[17] = new SqlParameter("@_LineData", lineData);

                db.ExecuteNonQuerySP("[SP_SlotMachine_CreateHistory]", pars);
                responseStatus = Convert.ToInt32(pars[16].Value);

                if (responseStatus < 0)
                {
                    NLogManager.LogError(string.Format("Execute {0} responseStatus={1}", "CreateHistory", responseStatus));
                }
                return responseStatus;
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
            return -999;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="sessionId"></param>
        /// <param name="totalBonusValue"></param>
        /// <returns></returns>
        public int UpdateBonusHistory(long accountId, long sessionId, long totalBonusValue)
        {
            DBHelper db = null;
            try
            {
                int responseStatus = 0;
                db = new DBHelper(ConnectionString.GameConnectionString);

                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_SessionID", sessionId);
                pars[1] = new SqlParameter("@_AccountID", accountId);
                pars[2] = new SqlParameter("@_TotalBonusValue", totalBonusValue);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                db.ExecuteNonQuerySP("[Sp_SlotMachine_UpdateBonusHistory]", pars);
                responseStatus = Convert.ToInt32(pars[3].Value);

                if (responseStatus < 0)
                {
                    NLogManager.LogError(string.Format("Execute {0} responseStatus={1}", "UpdateBonusHistory", responseStatus));
                }
                return responseStatus;
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
            return -999;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionID"></param>
        /// <param name="gameType"></param>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="totalLines"></param>
        /// <param name="betType"></param>
        /// <param name="betValue"></param>
        /// <param name="prizeValue"></param>
        /// <returns></returns>
        public int CreateRank(long sessionID, int gameType, long accountID, string username,
            int totalLines, int betType, long betValue, long prizeValue)
        {
            DBHelper db = null;
            try
            {
                int responseStatus = 0;
                db = new DBHelper(ConnectionString.GameConnectionString);

                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_SessionID", sessionID);
                pars[1] = new SqlParameter("@_GameType", gameType);
                pars[2] = new SqlParameter("@_AccountID", accountID);
                pars[3] = new SqlParameter("@_Username", username);
                pars[4] = new SqlParameter("@_TotalLines", totalLines);
                pars[5] = new SqlParameter("@_BetType", betType);
                pars[6] = new SqlParameter("@_BetValue", betValue);
                pars[7] = new SqlParameter("@_PrizeValue", prizeValue);
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                db.ExecuteNonQuerySP("[SP_SlotMachine_CreateRank]", pars);
                responseStatus = Convert.ToInt32(pars[8].Value);
                if (responseStatus < 0)
                {
                    NLogManager.LogError(string.Format("Execute {0} responseStatus={1}", "CreateRank", responseStatus));
                }
                return responseStatus;
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
            return -999;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sessionID"></param>
        /// <returns></returns>
        public SlotMachineHistoryDetailData GetHistoryDetail(long sessionID)
        {
            DBHelper db = null;
            SlotMachineHistoryDetailData result = null;
            try
            {
                // SlotMachineLogConnectionString
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_SessionID", sessionID);
                pars[1] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                result = db.GetInstanceSP<SlotMachineHistoryDetailData>("SP_SlotMachine_GetHistoryDetail", pars);
                return result;
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
            return result;
        }

        public List<SlotDiamondDetailSpin> GetSpinDetail(int InputType, long SpinID, out string LineData)
        {
            DBHelper db = null;
            List<SlotDiamondDetailSpin> result = null;
            LineData = string.Empty;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_InputType", InputType);
                pars[1] = new SqlParameter("@_SpinID", SpinID);
                pars[2] = new SqlParameter("@_LineData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

                result = db.GetListSP<SlotDiamondDetailSpin>("SP_Spin_Detail", pars);
                LineData = pars[2].Value.ToString();
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
            return result;
        }

        public ListJackPortHistory GetJackPortHistory(int inputType, int currentPage, int pageSize)
        {
            var data = new ListJackPortHistory();
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_InputType", inputType);
                pars[1] = new SqlParameter("@_PageIndex", currentPage);
                pars[2] = new SqlParameter("@_PageSize", pageSize);
                pars[3] = new SqlParameter("@_Total", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var result =
                    new DBHelper(ConnectionString.GameConnectionString).GetListSP<JackPortHistory>(
                        "SP_Jackpot_History", pars);
                if (result == null || result.Count <= 0)
                    return data;

                data.ListJackPort = result;
                data.ToTal = Convert.ToInt32(pars[3].Value);
                return data;
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return data;
            }
        }

        #endregion logging

        #region SetData

        /// <summary>
        /// Set dữ liệu test.
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="slotData"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public int SetDataTest(int inputType, string slotData, string userName)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_InputType", inputType);
                pars[1] = new SqlParameter("@_SlotData", slotData);
                pars[2] = new SqlParameter("@_Username", userName);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                new DBHelper(ConnectionString.GameConnectionString).ExecuteNonQuerySP("SP_SlotDiamond_SetData", pars);
                return (int)pars[3].Value;
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return -99;
            }
        }

        /// <summary>
        /// Lấy dữ liệu test
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetDataTest(int inputType, string userName)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_InputType", inputType);
                pars[1] = new SqlParameter("@_Username", userName);
                pars[2] = new SqlParameter("@_SlotData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                new DBHelper(ConnectionString.GameConnectionString).ExecuteNonQuerySP("SP_SlotDiamond_SetData", pars);
                return pars[2].Value.ToString();
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return string.Empty;
            }
        }

        #endregion SetData
    }
}