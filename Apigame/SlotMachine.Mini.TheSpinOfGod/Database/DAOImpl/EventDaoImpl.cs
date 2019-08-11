using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DataAccess.DAO;
using DataAccess.DTO;
using SlotMachine.Mini.TheSpinOfGod.Models;
using Utilities.Database;
using Utilities.Log;

namespace DataAccess.DAOImpl
{
    public class EventDaoImpl : IEventDao
    {
        public List<HistoryInfor> SP_SlotKingPocker_GetNotification(int betType)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);

                var oCommand = new SqlCommand("SP_SlotKingPocker_GetNotification") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_BetType", betType);

                var listReturn = new List<HistoryInfor>();
                listReturn = db.GetList<HistoryInfor>(oCommand);
                return listReturn;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new List<HistoryInfor>();
            }
        }

        public List<HistoryInfor> SP_SlotKingPocker_GetHisrorySpin(long accountId, int topCount, int betType)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);

                List<SqlParameter> pars = new List<SqlParameter>()
                {
                    new SqlParameter("@_AccountID", accountId),
                    new SqlParameter("@_BetType", betType),
                    new SqlParameter("@_Top", topCount),
                };

              
                return db.GetListSP<HistoryInfor>("[SP_SlotsGod_SpinHistory]", pars.ToArray());
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new List<HistoryInfor>();
            }
        }

        public SpinData SP_SlotsKingPoker_Spin(InputSpin valueSpin)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);

                var oCommand = new SqlCommand(valueSpin.BetType == 1 ? "SP_Spins_CreateTransaction" : "SP_Spins_CreateTransaction_Coin") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_AccountID", valueSpin.AccountId);
                oCommand.Parameters.AddWithValue("@_UserName", valueSpin.UserName);
                oCommand.Parameters.AddWithValue("@_RoomID", valueSpin.RoomId);
                oCommand.Parameters.AddWithValue("@_LinesData", valueSpin.LinesData);
                oCommand.Parameters.AddWithValue("@_ClientIP", valueSpin.ClientIp);

                var outSpinId = new SqlParameter("@_SpinID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outSpinId);

                var outSlotsData = new SqlParameter("@_SlotsData", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outSlotsData);

                var outPrizesData = new SqlParameter("@_PrizesData", SqlDbType.NVarChar, 500) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outPrizesData);

                var outTotalBetValue = new SqlParameter("@_TotalBetValue", SqlDbType.Int) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outTotalBetValue);

                var outTotalPrizeValue = new SqlParameter("@_TotalPrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outTotalPrizeValue);


                var outIsJackpot = new SqlParameter("@_IsJackpot", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outIsJackpot);

                var outJackpot = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outJackpot);

                var outBalance = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outBalance);

                var outResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outResponseStatus);

                var outLuckyData = new SqlParameter("@_LuckyData", SqlDbType.NVarChar) { Direction = ParameterDirection.Output, Size = 500 };
                oCommand.Parameters.Add(outLuckyData);

                var outTotalJP = new SqlParameter("@_TotalJackpot", SqlDbType.Int) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(outTotalJP);


                db.ExecuteNonQuery(oCommand);

                var spinData = new SpinData();
                spinData.SpinId = (long)outSpinId.Value;
                spinData.SlotData = outSlotsData.Value.ToString();
                spinData.PrizesData = outPrizesData.Value.ToString();
                spinData.TotalBetValue = (int)outTotalBetValue.Value;
                spinData.TotalPrizeValue = (long)outTotalPrizeValue.Value;
                spinData.Jackpot = (long)outJackpot.Value;
                spinData.Balance = (long)outBalance.Value;
                spinData.IsJackpot = (bool) outIsJackpot.Value;
                spinData.ResponseStatus = (int)outResponseStatus.Value;
                spinData.LuckyData = (string)outLuckyData.Value;
                spinData.TotalJackPot = (int)outTotalJP.Value;
                return spinData;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return new SpinData()
                {
                    ResponseStatus = -10000
                };
            }
        }
        public void SP_SlotsKingPoker_GetJackpot(int roomId, int betType, ref long jackpot)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);

                var oCommand = new SqlCommand("SP_RoomFunds_GetJackpot") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_RoomID", roomId);

                var p_Jackpot = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                oCommand.Parameters.Add(p_Jackpot);

                db.ExecuteNonQuery(oCommand);
                jackpot = (long)p_Jackpot.Value;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public DataTable GetHistory(long accountId, int top)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Top", top);
                return db.GetDataTableSP("[dbo].[SP_SlotsGod_SpinHistory]", pars);
            }
            catch(Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
        public DataTable GetSpinDetails(int spinId)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_SpinID", spinId);
                pars[1] = new SqlParameter("@_LineData", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output};
                return db.GetDataTableSP("[dbo].[SP_SlotsGod_SpinDetails]", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
        public DataTable GetJackpotHistory(int topCount)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_Top", topCount);
                return db.GetDataTableSP("[dbo].[SP_SlotsGod_JackpotHistory]", pars);
            }
            catch(Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public DataTable GetBigWinData(int topCount)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_Top", topCount);
                return db.GetDataTableSP("[dbo].[SP_SlotsGod_GetBigWinData]", pars);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }

        public string GetSlotsDataTest(string accountName)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_SlotsData", SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("[dbo].[SP_Spins_GetSlotsDataTest]", pars);
                return pars[1].Value != DBNull.Value ? pars[1].Value.ToString() : "";
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return null;
            }
        }
        public int SetSlotsDataTest(string accountName, string slotData)
        {
            try
            {
                var db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_SlotsData", slotData);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("[dbo].[SP_Spins_SetSlotsDataTest]", pars);
                return pars[2].Value != DBNull.Value ? int.Parse(pars[2].Value.ToString()) : -23;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return -24;
            }
        }

    }
}

