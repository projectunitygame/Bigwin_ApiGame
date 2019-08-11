using SlotGame._20Lines.Game1.Database.DAO;
using SlotGame._20Lines.Game1.Database.DTO;
using SlotMachine.TheThreeKingdoms.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Transports;
using SlotGame._20Lines.Game1.Models;
using Utilities.ConfigHelper;
using Utilities.Database;
using Utilities.Log;
using Newtonsoft.Json;

namespace SlotGame._20Lines.Game1.Database.DAOImpl
{
    public class SlotMachineDAOImpl : ISlotMachineDAO
    {
        /// <summary>
        /// Lấy thông tin người chơi trong game // dark magician
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public AccountInfo GetAccountInfo(int accountID, string username, int roomId, MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(Config.Game1ConnectionString);
                var pars = new SqlParameter[9];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_BetType", moneyType);
                pars[2] = new SqlParameter("@_Username", username);
                pars[3] = new SqlParameter("@_RoomID", roomId);
                pars[4] = new SqlParameter("@_FreeSpins", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_LastLineData", SqlDbType.VarChar) { Direction = ParameterDirection.Output, Size = 100 };
                pars[6] = new SqlParameter("@_LastPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_BonusID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("SP_Accounts_GetAccountInfo", pars);

                return new AccountInfo
                {
                    AccountID = accountID,
                    AccountName = username,
                    FreeSpin = pars[4].Value != DBNull.Value ? int.Parse(pars[4].Value.ToString()) : 0,
                    LastLineData = pars[5].Value != DBNull.Value ? pars[5].Value.ToString() : "",
                    LastPrizeValue = pars[6].Value != DBNull.Value ? long.Parse(pars[6].Value.ToString()) : 0,
                    BonusID = pars[7].Value != DBNull.Value ? long.Parse(pars[7].Value.ToString()) : 0,
                    ResponseStatus = pars[8].Value != DBNull.Value ? int.Parse(pars[8].Value.ToString()) : -99
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new AccountInfo
            {
                ResponseStatus = -1001
            };
        }

        /// <summary>
        /// Lấy thong tin quy
        /// </summary>
        /// <param name="roomTypeID"></param>
        /// <param name="betValue"></param>
        /// <returns></returns>
        public IEnumerable<Jackpot> GetJackpot(MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(Config.Game1ConnectionString);
    
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                return db.GetListSP<Jackpot>("[dbo].[SP_RoomFunds_GetAllJackpot]", pars);
                
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

        /// <summary>
        /// Quay
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="username"></param>
        /// <param name="linesData"></param>
        /// <param name="roomTypeID"></param>
        /// <param name="clientIP"></param>
        /// <param name="sourceId"></param>
        /// <param name="merchantId"></param>
        /// <param name="bonusGame"></param>
        /// <returns></returns>
        public SpinData Spin(int accountID, string username,
            string linesData, int roomId, string clientIP, MoneyType moneyType)
        {
            string s = "Spin Kungfu Panda: " +
                "\r\nAccountID: " + accountID +
                "\r\nUserName: " + username +
                "\r\nLinesData: " + linesData +
                "\r\nRoomId: " + roomId +
                "\r\nclientIP: " + clientIP +
                "\r\nmoneyType: " + moneyType;
            try
            {
                var db = new DBHelper(Config.Game1ConnectionString);
                var pars = new SqlParameter[21];
                pars[0] = new SqlParameter("@_AccountID", accountID);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_RoomID", roomId);
                pars[3] = new SqlParameter("@_LineData", linesData);
                pars[4] = new SqlParameter("@_ClientIP", clientIP);
                pars[5] = new SqlParameter("@_SpinID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_SlotData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_PrizeData", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_IsJackpot", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_BonusGameData", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_PositionData", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_FreeSpins", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_TotalBetValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_PaylinePrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Tong so tien thang
                pars[15] = new SqlParameter("@_PrizeFund", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Quỹ
                pars[16] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//số dư gold hoac coin
                pars[17] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[18] = new SqlParameter("@_BonusMultiplier", SqlDbType.Int) { Direction = ParameterDirection.Output };// Hệ số nhân Bonus khởi điểm
                pars[19] = new SqlParameter("@_BonusPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Tiền thắng Bonus
                pars[20] = new SqlParameter("@_TotalJackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };//Tiền thắng jackpot
                db.ExecuteNonQuerySP(
                    moneyType == MoneyType.Gold ? "SP_Spins_CreateTransaction" : "SP_Spins_CreateTransaction_Coin",
                    pars);
                s += "\r\nResult Spin Kungfu Panda:" + JsonConvert.SerializeObject(pars) + 
                    "\r\n" + JsonConvert.SerializeObject(pars.Select(x=>x.Value).ToArray()); 
                var data = new SpinData
                {
                    AccountID = accountID,
                    Balance = long.Parse(pars[16].Value.ToString()),
                    IsJackpot = bool.Parse(pars[8].Value.ToString()),
                    Jackpot = long.Parse(pars[9].Value.ToString()),
                    ResponseStatus = int.Parse(pars[17].Value.ToString()),
                    PrizeFund = Convert.ToInt64(pars[15].Value.ToString()),
                    SpinID = long.Parse(pars[5].Value.ToString()),
                    TotalBetValue = int.Parse(pars[13].Value.ToString()),
                    TotalPrizeValue = long.Parse(pars[14].Value.ToString()),
                    TotalFreeSpin = int.Parse(pars[12].Value.ToString()),
                    TotalJackpot = int.Parse(pars[20].Value.ToString()),
                    BonusGame = new BonusGameData()
                    {
                        StartBonus = (int)pars[18].Value,
                        GoldMinerData = pars[10].Value.ToString(),
                        PrizeValue = Convert.ToInt64(pars[19].Value.ToString())
                    }
                };
                if (!string.IsNullOrEmpty(pars[7].Value.ToString()))
                {
                    NLogManager.LogMessage(
                        $"SlotMachineSpinData-Spin-PrizesData: prizeData:{pars[7].Value}|posionData:{pars[11].Value}");
                    data.PrizesData = SpinData.SetPrizeLines($"{pars[7].Value}",
                        $"{pars[11].Value}");
                }
                if (!string.IsNullOrEmpty(pars[13].Value.ToString()))
                {
                    data.SlotsData = SpinData.SetSlots($"{pars[6].Value}");
                }
                if(moneyType == MoneyType.Gold)
                    NLogManager.LogMessage( $"SP_SlotDiamond_Spin=>Acc:{data.AccountID}|User:{username}|Room:{roomId}|SpinID:{data.SpinID}|SlotData:{data.SlotsData}|ResponseStatus:{data.ResponseStatus}|IsJackpot:{data.IsJackpot}|TotalPrizeValue:{data.TotalPrizeValue}|linesData:{linesData}|IP:{clientIP}");
                s += "\r\nResponse Data: " + JsonConvert.SerializeObject(data);
                NLogManager.LogMessage(s);
                return data;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new SpinData
            {
                AccountID = accountID,
                ResponseStatus = -1001
            };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="spinId"></param>
        /// <param name="totalPrizeValue"></param>
        /// <returns></returns>
        public long FinishBonusGame(MoneyType moneyType, long spinId, ref long totalPrizeValue, ref long balance)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                pars[1] = new SqlParameter("@_SpinID", spinId);
                pars[2] = new SqlParameter("@_TotalPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                new DBHelper(Config.Game1ConnectionString).ExecuteNonQuerySP("SP_BonusGameSpins_Finish", pars);
                if(moneyType == MoneyType.Gold)
                    NLogManager.LogMessage($"FinisBonusGame=>SpinId:{spinId}|PrizeValue:{pars[2].Value}|Respone:{pars[3].Value}");
                totalPrizeValue = pars[2].Value != DBNull.Value ? long.Parse(pars[2].Value.ToString()) : 0;
                balance = pars[4].Value != DBNull.Value ? long.Parse(pars[4].Value.ToString()) : 0;
                return pars[3].Value != DBNull.Value ? long.Parse(pars[3].Value.ToString()) : -88; ;
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                totalPrizeValue = 0;
                return -99;
            }

        }


        /// <summary>
        /// Lấy lịch sử giao dịch
        /// </summary>
        /// <returns></returns>
        public DataTable GetHistory(MoneyType moneyType, long accountId, int top)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                pars[1] = new SqlParameter("@_AccountId", accountId);
                pars[2] = new SqlParameter("@_Top", top);
                DBHelper db = new DBHelper(Config.Game1ConnectionString);
                return db.GetDataTableSP("[dbo].[SP_Accounts_GetHistory]", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }


        public List<SpinDetail> GetSpinDetail(MoneyType moneyType, long spinId, out string lineData)
        {
            try
            {
                List<SpinDetail> result = null;
                lineData = string.Empty;
                var db = new DBHelper(string.Empty);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                pars[1] = new SqlParameter("@_SpinID", spinId);
                pars[2] = new SqlParameter("@_LineData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                result = db.GetListSP<SpinDetail>("[dbo].[SP_Spin_Detail]", pars);
                lineData = pars[2].Value.ToString();
                return result;

            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                lineData = string.Empty;
            }
            return null;
        }


        public JackpotHistoryList GetJackpotHistory(MoneyType moneyType, int currentPage, int pageSize)
        {
            var data = new JackpotHistoryList();
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                pars[1] = new SqlParameter("@_PageIndex", currentPage);
                pars[2] = new SqlParameter("@_PageSize", pageSize);
                pars[3] = new SqlParameter("@_Total", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var result =
                    new DBHelper(Config.Game1ConnectionString).GetListSP<JackpotHistory>(
                        "SP_Jackpot_History", pars);
                if (result == null || result.Count <= 0)
                    return data;

                data.JackpotsHistory = result;
                data.TotalRecord = Convert.ToInt32(pars[3].Value);
                return data;
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return data;
            }
        }

        #region SetData


        public int SetTestData(string accountName, string slotsData)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_SlotsData", slotsData);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                new DBHelper(Config.Game1ConnectionString).ExecuteNonQuerySP("SP_Spins_SetSlotsDataTest", pars);
                return (int)pars[2].Value;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return -99;
            }
        }

        public string GetTestData(string accountName)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_SlotsData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };
                new DBHelper(Config.Game1ConnectionString).ExecuteNonQuerySP("SP_Spins_GetSlotsDataTest", pars);
                return pars[1].Value.ToString();
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                return string.Empty;
            }
        }

        public DataTable GetTop2Jackpot()
        {
            try
            {
                return new DBHelper(Config.Game1ConnectionString).GetDataTableSP("[dbo].[SP_SlotDiamond_GetLastestJackpot]");

            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }


        #endregion

    }
}
