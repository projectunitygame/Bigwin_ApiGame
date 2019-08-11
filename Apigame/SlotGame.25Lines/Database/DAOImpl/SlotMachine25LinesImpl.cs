using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using SlotGame._25Lines.Database.DAO;
using SlotGame._25Lines.Database.DTO;
using Utilities.ConfigHelper;
using Utilities.Database;
using Utilities.IP;
using Utilities.Log;

namespace SlotGame._25Lines.Database.DAOImpl
{
    public class SlotMachine25LinesImpl: ISlotMachine25Lines
    {
        public AccountInfo GetAccountInfo(long accountId, string accountName, int roomId, MoneyType moneyType, out int response)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[12];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_RoomId", roomId);
                pars[3] = new SqlParameter("@_MoneyType", moneyType);
                pars[4] = new SqlParameter("@_FreeSpins", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[5] = new SqlParameter("@_Status", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[6] = new SqlParameter("@_LastPrizeValue", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[7] = new SqlParameter("@_LastLineData", SqlDbType.VarChar, 100){Direction = ParameterDirection.Output};
                pars[8] = new SqlParameter("@_BonusSpinId", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[9] = new SqlParameter("@_BonusData", SqlDbType.VarChar, 500) {Direction = ParameterDirection.Output};
                pars[10] = new SqlParameter("@_TurnId", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_ResponseStatus", SqlDbType.Int){Direction = ParameterDirection.Output};

                db.ExecuteNonQuerySP("[dbo].[SP_Accounts_GetAccountInfo]", pars);
                var accountInfo = new AccountInfo()
                {
                    AccountId = accountId,
                    AccountName = accountName,
                    RoomId = roomId,
                    FreeSpins = pars[4].Value != DBNull.Value ? int.Parse(pars[4].Value.ToString()) : 0,
                    Status = pars[5].Value != DBNull.Value ? (PlayerStatus)int.Parse(pars[5].Value.ToString()) : PlayerStatus.None,
                    LastPrizeValue = pars[6].Value != DBNull.Value ? int.Parse(pars[6].Value.ToString()) : 0,
                    LastLineData = pars[7].Value != DBNull.Value ? pars[7].Value.ToString() : "",
                    BonusSpinId = pars[8].Value != DBNull.Value ? int.Parse(pars[8].Value.ToString()) : 0,
                    BonusData   = pars[9].Value != DBNull.Value ? pars[9].Value.ToString() : "",
                    TurnId = pars[10].Value != DBNull.Value ? int.Parse(pars[10].Value.ToString())  : 0,
                };
                response = pars[11].Value != DBNull.Value ? int.Parse(pars[11].Value.ToString()) : -98;
                return accountInfo;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                response = -99;
                return null;
            }
        }

        public OutputSpinData Spin(InputSpinData inputData)
        {
            try
            {
                string s = "[tamquoc] SpinInfo: " + JsonConvert.SerializeObject(inputData);
                var ipAddress = "";
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[18];
                pars[0] = new SqlParameter("@_AccountId", inputData.AccountId);
                pars[1] = new SqlParameter("@_AccountName", inputData.AccountName);
                pars[2] = new SqlParameter("@_RoomID", inputData.RoomId);
                pars[3] = new SqlParameter("@_MoneyType", inputData.MoneyType);
                pars[4] = new SqlParameter("@_LineData", inputData.LineData);
                pars[5] = new SqlParameter("@_ClientIP", ipAddress);
                pars[6] = new SqlParameter("@_TotalBetValue", inputData.TotalBetValue);
                pars[7] = new SqlParameter("@_SlotsData", inputData.SlotsData);
                pars[8] = new SqlParameter("@_IsJackpot", inputData.IsJackpot);
                pars[9] = new SqlParameter("@_AddFreeSpins", inputData.AddFreeSpins);
                pars[10] = new SqlParameter("@_TotalPrizeValue", inputData.TotalPrizeValue);
                pars[11] = new SqlParameter("@_TotalBonusValue", inputData.TotalBonusValue);
                pars[12] = new SqlParameter("@_SpinId", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[13] = new SqlParameter("@_TotalJackpotValue", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[14] = new SqlParameter("@_FreeSpins", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[15] = new SqlParameter("@_Jackpot", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[16] = new SqlParameter("@_Balance", SqlDbType.BigInt) {Direction = ParameterDirection.Output};
                pars[17] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction =  ParameterDirection.Output};
                var spName = inputData.MoneyType == MoneyType.Gold ? "[dbo].[SP_Spins_CreateTransaction]" : "[dbo].[SP_Spins_CreateTransaction_Coin]";
                db.ExecuteNonQuerySP(spName, pars);
                var outPutData = new OutputSpinData()
                {
                    SpinId = pars[12].Value != DBNull.Value ? int.Parse(pars[12].Value.ToString()) : 0,
                    TotalJackpotValue = pars[13].Value != DBNull.Value ? int.Parse(pars[13].Value.ToString()) : 0,
                    FreeSpins =  pars[14].Value != DBNull.Value ? int.Parse(pars[14].Value.ToString()) : 0,
                    Jackpot = pars[15].Value != DBNull.Value ? int.Parse(pars[15].Value.ToString()) : 0,
                    Balance = pars[16].Value != DBNull.Value ? long.Parse(pars[16].Value.ToString()) : 0,
                    ResponseStatus = pars[17].Value != DBNull.Value ? int.Parse(pars[17].Value.ToString()) : -98
                };
                s += "\r\nResult spin: " + JsonConvert.SerializeObject(pars) +
                   "\r\n" + JsonConvert.SerializeObject(pars.Select(x => x.Value).ToArray());
                //NLogManager.LogMessage($"Spin => AccountId:{inputData.AccountId}|AccountName:{inputData.AccountName}|RoomId:{inputData.RoomId}|MoneyType:{inputData.MoneyType}|LineData:{inputData.LineData}|Ip:{ipAddress}" +
                //                       $"ToTalBetValue:{inputData.TotalBetValue}|SlotsData:{inputData.SlotsData}|IsJackpot:{inputData.IsJackpot}|AddFreeSpins:{inputData.AddFreeSpins}|TotalPrizeValue:{inputData.TotalPrizeValue}" +
                //                       $"TotalBonusValue:{inputData.TotalBonusValue}|SpinId:{pars[12].Value}|TotalJackpotValue:{pars[13].Value}|FreeSpins:{pars[14].Value}|Jackpot:{pars[15].Value}|Balance:{pars[16].Value}|Response:{pars[17].Value}");
                s += "\r\nResponse data: " + JsonConvert.SerializeObject(outPutData);
                NLogManager.LogMessage(s);
                return outPutData;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public int UpdateSlotsData(int spinId, string slotsData)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_SpinId", spinId);
                pars[1] = new SqlParameter("@_SlotData", slotsData);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var spName = "SP_Spins_UpdateSlotsData";
                db.ExecuteNonQuerySP(spName, pars);
                int.TryParse(pars[2].Value.ToString(), out var response);

                NLogManager.LogMessage($"UpdateSlotsData => SpinId:{spinId}|SlotsData:{slotsData}|Response:{pars[2].Value}");
                return response;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return -99;
            }
        }

        public void CreateBonusGame(MoneyType money, int spinId, int roomId, long accountId, string accountName, int totalBetValue,
            string bonusGameData, int multi, int totalPrizeValue, out int response)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[10];
                pars[0] = new SqlParameter("@_MoneyType", money);
                pars[1] = new SqlParameter("@_SpinId", spinId);
                pars[2] = new SqlParameter("@_RoomId", roomId);
                pars[3] = new SqlParameter("@_AccountId", accountId);
                pars[4] = new SqlParameter("@_AccountName", accountName);
                pars[5] = new SqlParameter("@_BetValue", totalBetValue);
                pars[6] = new SqlParameter("@_BonusGameData", bonusGameData);
                pars[7] = new SqlParameter("@_Multiplier", multi);
                pars[8] = new SqlParameter("@_TotalPrizeValue", totalPrizeValue);
                pars[9] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction = ParameterDirection.Output};
                var spName =
                    "[dbo].[SP_BonusGame_CreateGame]";
                db.ExecuteNonQuerySP(spName, pars);
                int.TryParse(pars[9].Value.ToString(), out response);
                
                NLogManager.LogMessage($"Create_BonusGame => MoneyType:{money}|SpinId:{spinId}|RoomId:{roomId}" +
                                       $"|AccountId:{accountId}|AccountName:{accountName}|totalBetValue:{totalBetValue}|BonusData:{bonusGameData}|Mutil:{multi}|TotalPrizeValue:{totalPrizeValue}|Response:{pars[9].Value}");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                response = -99;
            }
        }

        public void FinishBonusGame(MoneyType money, int spinId, out int totalPrizeValue, out long balance, out int response)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_MoneyType", money);
                pars[1] = new SqlParameter("@_SpinId", spinId);
                pars[2] = new SqlParameter("@_TotalPrizeValue", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[3] = new SqlParameter("@_Balance", SqlDbType.BigInt) {Direction = ParameterDirection.Output};
                pars[4] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction = ParameterDirection.Output};
                var spName =
                    "[dbo].[SP_BonusGame_FinishGame]";
      
                db.ExecuteNonQuerySP(spName, pars);
                int.TryParse(pars[2].Value.ToString(), out totalPrizeValue);
                long.TryParse(pars[3].Value.ToString(), out balance);
                int.TryParse(pars[4].Value.ToString(), out response);

                NLogManager.LogMessage($"Finish_BonusGame => MoneyType:{money}|SpinId:{spinId}|TotalPrizeValue:{pars[2].Value}|Balance:{pars[3].Value}|Response:{pars[4].Value}");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                response = -99;
                balance = 0;
                totalPrizeValue = 0;
            }
        }

        public IEnumerable<Jackpot> GetJackpot(MoneyType moneyType)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                return db.GetListSP<Jackpot>("[dbo].[SP_Jackpot_GetAll]", pars);
            } 
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public LuckyGame PlayLuckyGame(MoneyType moneyType, long accountId, string accountName, int roomId, int spinId, X2Game step, int result)
        {
            try
            {
                var db = new DBHelper(Config.Game25LinesConnectionString);
                var pars = new SqlParameter[11];
                pars[0] = new SqlParameter("@_AccountId", accountId);
                pars[1] = new SqlParameter("@_AccountName", accountName);
                pars[2] = new SqlParameter("@_RoomId", roomId);
                pars[3] = new SqlParameter("@_Step", step);
                pars[4] = new SqlParameter("@_SpinId", spinId);
                pars[5] = new SqlParameter("@_Result", result);
                pars[6] = new SqlParameter("@_TurnId", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[7] = new SqlParameter("@_RemainTurn", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[8] = new SqlParameter("@_PrizeValue", SqlDbType.Int) {Direction = ParameterDirection.Output};
                pars[9] = new SqlParameter("@_Balance", SqlDbType.BigInt) {Direction = ParameterDirection.Output};
                pars[10] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction = ParameterDirection.Output};
                var spName = moneyType == MoneyType.Gold
                    ? "[dbo].[SP_LuckyGame_Play]"
                    : "[dbo].[SP_LuckyGame_Play_Coin]";
                db.ExecuteNonQuerySP(spName, pars);

                var gameResult = new LuckyGame()
                {
                    TurnId = pars[6].Value != DBNull.Value ? int.Parse(pars[6].Value.ToString()) : 0,
                    RemainTurn = pars[7].Value != DBNull.Value ? int.Parse(pars[7].Value.ToString()) : 0,
                    PrizeValue = pars[8].Value != DBNull.Value ? int.Parse(pars[8].Value.ToString()) : 0,
                    Balance = pars[9].Value != DBNull.Value ? long.Parse(pars[9].Value.ToString()) : 0,
                    ResponseStatus = pars[10].Value != DBNull.Value ? int.Parse(pars[10].Value.ToString()) : -80
                };

                NLogManager.LogMessage($"Play_LuckyGame => AccountId:{accountId}|AccountName:{accountName}|RoomId:{roomId}|Step:{step}|SpinId:{spinId}|Result:{result}" +
                                       $"|TurnID:{pars[6].Value}|RemainTurn:{pars[7].Value}|PrizeValue:{pars[8].Value}|Balance:{pars[9].Value}|Response:{pars[10].Value}");

                return gameResult;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }


        public DataTable GetHistory(MoneyType moneyType, long accountId, int topCount)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_MoneyType", moneyType);
                pars[1] = new SqlParameter("@_AccountId", accountId);
                pars[2] = new SqlParameter("@_Top", topCount);
                DBHelper db = new DBHelper(Config.Game25LinesConnectionString);
                return db.GetDataTableSP("[dbo].[SP_Accounts_GetHistory]", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
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
                    new DBHelper(Config.Game25LinesConnectionString).GetListSP<JackpotHistory>(
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

        public DataTable GetTop2Jackpot()
        {
            try
            {
                return new DBHelper(Config.Game25LinesConnectionString).GetDataTableSP("[dbo].[SP_Jackpot_GetLastestJackpot]");

            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
    }
}