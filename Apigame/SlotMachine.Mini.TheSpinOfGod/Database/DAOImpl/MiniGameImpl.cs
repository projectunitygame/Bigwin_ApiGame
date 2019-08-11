using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DAO;
using DataAccess.DTO;
using MinigameVuabai.SignalR.Controllers;
using SlotMachine.Mini.TheSpinOfGod.Models;
using Utilities.Database;

namespace DataAccess.DAOImpl
{
    public class MiniGameImpl : IMinigame
    {
        private readonly DBHelper _db;
        public MiniGameImpl()
        {
            _db = new DBHelper(ConnectionString.GameConnectionString);
        }

        /// <summary>
        /// Gieo quẻ
        /// </summary>
        /// <param name="_AccountID"></param>
        /// <param name="_Username"></param>
        /// <param name="_LuckyTurn"></param>
        /// <param name="_LotteryNumber"></param>
        /// <param name="_ResponseStatus"></param>
        //">>Params
        //@_AccountID INT,
        //@_Username VARCHAR(30),
        //@_LuckyTurn INT = 0 OUTPUT, 
        //@_LotteryNumber VARCHAR(5) = '' OUTPUT,
        //@_ResponseStatus INT OUTPUT"
        public void SP_TET2015_E1_SetBet(int _AccountID, string _Username, out int _LuckyTurn, out string _LotteryNumber, out int _ResponseStatus)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E1_SetBet") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_AccountID", _AccountID);
                oCommand.Parameters.AddWithValue("@_Username", _Username);

                var p_LuckyTurn = new SqlParameter("@_LuckyTurn", SqlDbType.Int);
                p_LuckyTurn.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_LuckyTurn);

                var p_LotteryNumber = new SqlParameter("@_LotteryNumber", SqlDbType.NVarChar, 5);
                p_LotteryNumber.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_LotteryNumber);

                var p_ResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int);
                p_ResponseStatus.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_ResponseStatus);

                _db.ExecuteNonQuery(oCommand);

                _LuckyTurn = (int)p_LuckyTurn.Value;
                _LotteryNumber = p_LotteryNumber.Value.ToString();
                _ResponseStatus = (int)p_ResponseStatus.Value;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _LuckyTurn = _ResponseStatus = -1;
                _LotteryNumber = string.Empty;
            }
        }
        /// <summary>
        /// Lịch sử gieo quẻ
        /// </summary>
        /// <param name="_AccountID"></param>
        ///">>Params
        //@_AccountID INT
        //@_CurrentPage INT,
        //@_PageSize     INT,
        //@_TotalRecord   INT = 0 OUTPUT
        //    >> Output Table
        //- CreatedTime DATETIME
        //- LogType NVARCHAR(20)
        //- Number INT
        //- Details NVARCHAR(20)
        //- Prizes NVARCHAR(20)"
        public List<HistoryGieoQue> SP_TET2015_E1_GetAccountHistory(int _AccountID, int _CurrentPage, int _PageSize, out int _TotalRecord)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E1_GetAccountHistory") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_AccountID", _AccountID);
                oCommand.Parameters.AddWithValue("@_CurrentPage", _CurrentPage);
                oCommand.Parameters.AddWithValue("@_PageSize", _PageSize);

                var p_TotalRecord = new SqlParameter("@_TotalRecord", SqlDbType.Int);
                p_TotalRecord.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_TotalRecord);

                var lst = _db.GetList<HistoryGieoQue>(oCommand);
                _TotalRecord = (int)p_TotalRecord.Value;
                return lst;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _TotalRecord = -1;
                return new List<HistoryGieoQue>();
            }
        }
        //">>Params
        //@_GameSessionID INT OUTPUT
        // ,@_GameStatus INT = 0 OUTPUT-- 0:Waiting; 1:Racing
        // ,@_RemainWaiting INT = 0 OUTPUT 
        // ,@_RemainRacing INT = 0 OUTPUT"
        /// <summary>
        /// Lấy thông tin phiên nấu bánh chưng
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_GameStatus"></param>
        /// <param name="_RemainWaiting"></param>
        /// <param name="_RemainRacing"></param>
        public void SP_TET2015_E2_GetCurrentData(out int _GameSessionID, out int _GameStatus, out int _RemainWaiting, out int _RemainRacing)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E2_GetCurrentData") { CommandType = CommandType.StoredProcedure };
                var p_GameSessionID = new SqlParameter("@_GameSessionID", SqlDbType.Int);
                p_GameSessionID.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_GameSessionID);

                var p_GameStatus = new SqlParameter("@_GameStatus", SqlDbType.Int);
                p_GameStatus.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_GameStatus);

                var p_RemainWaiting = new SqlParameter("@_RemainWaiting", SqlDbType.Int);
                p_RemainWaiting.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_RemainWaiting);

                var p_RemainRacing = new SqlParameter("@_RemainRacing", SqlDbType.Int);
                p_RemainRacing.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_RemainRacing);

                _db.ExecuteNonQuery(oCommand);

                _GameSessionID = (int)p_GameSessionID.Value;
                _GameStatus = (int)p_GameStatus.Value;
                _RemainWaiting = (int)p_RemainWaiting.Value;
                _RemainRacing = (int)p_RemainRacing.Value;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _GameSessionID = _GameStatus = _RemainWaiting = _RemainRacing = -1;
            }
        }
        /// <summary>
        /// Lấy danh sách phòng nấu bánh chưng
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_GameStatus"></param>
        /// <returns></returns>
        public List<RoomNauBanh> SP_TET2015_E2_GetRoomInfo(int _GameSessionID, int _RoomID, out int _GameStatus)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E2_GetRoomInfo") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_GameSessionID", _GameSessionID);
                oCommand.Parameters.AddWithValue("@_RoomID", _RoomID);


                var p_GameStatus = new SqlParameter("@_GameStatus", SqlDbType.Int);
                p_GameStatus.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_GameStatus);

                var lst = _db.GetList<RoomNauBanh>(oCommand);
                _GameStatus = (int)p_GameStatus.Value;
                return lst;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _GameStatus = -1;
                return new List<RoomNauBanh>();
            }
        }
        //">>Params
        //@_AccountID INT,
        //@_Username VARCHAR(30),
        //@_RoomID INT,
        //@_CardID INT OUTPUT,
        //@_CardValue INT OUTPUT,
        //@_ResponseStatus INT OUTPUT"
        /// <summary>
        /// Săn thẻ nấu bánh chưng
        /// </summary>
        /// <param name="_AccountID"></param>
        /// <param name="_Username"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_CardID"></param>
        /// <param name="_CardValue"></param>
        /// <param name="_ResponseStatus"></param>
        public void SP_TET2015_E2_GetCard(int _AccountID, string _Username, int _RoomID, out int _CardID, out int _CardValue, out int _ResponseStatus)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E2_GetCard") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_AccountID", _AccountID);
                oCommand.Parameters.AddWithValue("@_Username", _Username);
                oCommand.Parameters.AddWithValue("@_RoomID", _RoomID);

                var p_CardID = new SqlParameter("@_CardID", SqlDbType.Int);
                p_CardID.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_CardID);

                var p_CardValue = new SqlParameter("@_CardValue", SqlDbType.Int);
                p_CardValue.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_CardValue);

                var p_ResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int);
                p_ResponseStatus.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_ResponseStatus);

                _db.ExecuteNonQuery(oCommand);
                _CardID = (int)p_CardID.Value;
                _CardValue = (int)p_CardValue.Value;
                _ResponseStatus = (int)p_ResponseStatus.Value;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _CardID = -1;
                _CardValue = -1;
                _ResponseStatus = -1;
            }
        }
        /// <summary>
        /// Share nhận thưởng nấu bánh chưng
        /// Vinh.vu - 27.01.2015
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_AccountID"></param>
        /// <param name="_ResponseStatus"></param>
        public void SP_TET2015_E2_Finish(int _GameSessionID, int _RoomID, int _AccountID, out int _ResponseStatus)
        {
            try
            {
                var oCommand = new SqlCommand("SP_TET2015_E2_Finish") { CommandType = CommandType.StoredProcedure };
                oCommand.Parameters.AddWithValue("@_AccountID", _AccountID);
                oCommand.Parameters.AddWithValue("@_GameSessionID", _GameSessionID);
                oCommand.Parameters.AddWithValue("@_RoomID", _RoomID);

                var p_ResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int);
                p_ResponseStatus.Direction = ParameterDirection.Output;
                oCommand.Parameters.Add(p_ResponseStatus);

                _db.ExecuteNonQuery(oCommand);
                _ResponseStatus = (int)p_ResponseStatus.Value;
            }
            catch (Exception ex)
            {
                NLogLogger.LogWarning(ex.Message);
                _ResponseStatus = -1;
            }
        }

        /// <summary>
        /// Set dữ liệu test
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="slotData"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        public int SetSlotData(int accountId, string slotData, int betType)
        {
            try
            {
                var pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_SlotData", slotData);
                pars[2] = new SqlParameter("@_BetType", betType);
                pars[3] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

                new DBHelper(ConnectionString.GameConnectionString).ExecuteNonQuerySP("SP_SlotData_Set", pars);

                return (int)pars[3].Value;
            }
            catch (Exception e)
            {
                NLogLogger.PublishException(e);
                return -99;
            }
        }

        /// <summary>
        /// Lấy dữ liệu test theo
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        public string GetSlotData(int accountId, int betType)
        {
            try
            {
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_SlotData", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
                pars[2] = new SqlParameter("@_BetType", betType);
                new DBHelper(ConnectionString.GameConnectionString).ExecuteNonQuerySP("SP_SlotData_Get", pars);
                return pars[1].Value.ToString();
            }
            catch (Exception e)
            {
                NLogLogger.PublishException(e);
                return string.Empty;
            }
        }
    }
}
