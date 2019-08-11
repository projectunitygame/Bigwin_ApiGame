using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigames.DataAccess.DAO;
using Minigames.DataAccess.DTO;
using Newtonsoft.Json;
using MiniGame.HiloServer.Models;
using Utilities.Database;
using Utilities.Log;

namespace Minigames.DataAccess.DAOImpl
{
    public class HiloDaoImpl : IHiloDao
    {
        public HiLoGetAccountInfoResponse GetAccountInfo(int accountId, string username)
        {
            try
            {
                var pars = new SqlParameter[14];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_CurrentTurnID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[3] = new SqlParameter("@_CurrentStep", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[4] = new SqlParameter("@_CurrentRoomID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[5] = new SqlParameter("@_CurrentBetType", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[6] = new SqlParameter("@_CurrentBetValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[7] = new SqlParameter("@_CurrentCardData", SqlDbType.NVarChar) { Direction = ParameterDirection.Output, Size = 200 };
                pars[8] = new SqlParameter("@_CurrentAces", SqlDbType.NVarChar) { Direction = ParameterDirection.Output, Size = 20 };
                pars[9] = new SqlParameter("@_AcesCount", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_RemainTime", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_BetRateUp", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 };
                pars[12] = new SqlParameter("@_BetRateDown", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 };
                pars[13] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
  
                var db = new DBHelper(ConnectionString.GameConnectionString);
                db.ExecuteNonQuerySP("SP_Accounts_GetAccountInfo", pars);
                HiLoGetAccountInfoResponse data = new HiLoGetAccountInfoResponse();

                data.currentTurnId = Convert.ToInt32(pars[2].Value);
                try
                {
                    data.currentStep = Convert.ToInt32(pars[3].Value);
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }

                data.currentRoomId = Convert.ToInt32(pars[4].Value);
                data.currentBetType = Convert.ToInt32(pars[5].Value);
                data.currentBetValue = Convert.ToInt32(pars[6].Value);
                data.currentCardData = Convert.ToString(pars[7].Value);
                data.currentAces = Convert.ToString(pars[8].Value);
                data.acesCount = Convert.ToInt32(pars[9].Value);
                data.remainTime = Convert.ToInt32(pars[10].Value);
                data.betRateUp = Convert.ToDecimal(pars[11].Value);
                data.betRateDown = Convert.ToDecimal(pars[12].Value);
                data.responseStatus = Convert.ToInt32(pars[13].Value);
                if (data.currentTurnId > 0)
                {
                    NLogManager.LogMessage(string.Format("GetAccountInfoHiLo=>Acc:{0}|User:{1}|Result:{2}",
                       accountId, username, JsonConvert.SerializeObject(data)));
                }

                return data;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return new HiLoGetAccountInfoResponse();
        }

        public HiLoSetBetResponse SetBet(int accountId, string username, int roomId, int betType, 
            int stepType, int locationId, string clientIp, int sourceId, int merchantId)
        {
            try
            {
                //NLogManager.LogMessage(string.Format("Acc:{0}|User:{1}|RoomId:{2}|Bettype:{3}|steptype:{4}|Location:{5}",
                //    accountId,username,roomId,betType,stepType,locationId));
                var pars = new SqlParameter[21];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_Username", username);
                pars[2] = new SqlParameter("@_RoomID", roomId);
                pars[3] = new SqlParameter("@_BetType", betType);
                pars[4] = new SqlParameter("@_StepType", stepType);
                pars[5] = new SqlParameter("@_LocationID", locationId);
                pars[6] = new SqlParameter("@_ClientIP", clientIp);
                pars[7] = new SqlParameter("@_SourceID", sourceId);
                pars[8] = new SqlParameter("@_MerchantID", merchantId);
                pars[9] = new SqlParameter("@_BetValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_PrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_IsJackpot", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_TurnID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_Step", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[14] = new SqlParameter("@_CardID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[15] = new SqlParameter("@_BetRateUp", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 };
                pars[16] = new SqlParameter("@_BetRateDown", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 };
                pars[17] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[18] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[19] = new SqlParameter("@_IsBonus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[20] = new SqlParameter("@_TotalPoint", SqlDbType.Int) { Direction = ParameterDirection.Output };

                var db = new DBHelper(ConnectionString.GameConnectionString);
                db.ExecuteNonQuerySP("SP_Turns_CreateTransaction", pars);
                HiLoSetBetResponse data = new HiLoSetBetResponse();

                data.betValue = Convert.ToInt32(pars[9].Value);
                data.prizeValue = Convert.ToInt32(pars[10].Value);
                data.isJackpot = Convert.ToInt32(pars[11].Value);
                data.turnId = Convert.ToInt32(pars[12].Value);
                data.step = Convert.ToInt32(pars[13].Value);
                data.cardId = Convert.ToInt32(pars[14].Value);
                data.betRateUp = Convert.ToDecimal(pars[15].Value);
                data.betRateDown = Convert.ToDecimal(pars[16].Value);
                data.balance = Convert.ToInt64(pars[17].Value);
                data.responseStatus = Convert.ToInt32(pars[18].Value);
                data.IsBonus = Convert.ToInt32(pars[19].Value);
                data.TotalPoint = Convert.ToInt32(pars[20].Value);
                NLogManager.LogMessage(String.Format("HiloSetBet:Acc:{0}|User:{1}|Room:{2}|Bettype:{3}|StepType:{4}|Location:{5}|Respone:{6}|Ip:{7}",
                    accountId, username, roomId, betType,stepType,locationId, JsonConvert.SerializeObject(data), clientIp));
                return data;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return new HiLoSetBetResponse();
        }

        public long GetJackpot(int betType, int roomId)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_BetType", betType);
                pars[1] = new SqlParameter("@_RoomID", roomId);

                var db = new DBHelper(ConnectionString.GameConnectionString);
                var result = db.GetInstanceSP<HiLoGetJackpot>("SP_Funds_GetJackpot", pars);
                if (result != null)
                    return result.JackpotFund;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return 0;
        }

        public List<HiLoGetTopAccount> GetTopAccount(int betType, int topCount)
        {
            try
            {
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_BetType", betType);
                pars[1] = new SqlParameter("@_TopCount", topCount);

                var db = new DBHelper(ConnectionString.GameConnectionString);
                List<HiLoGetTopAccount> list = db.GetListSP<HiLoGetTopAccount>("SP_TopAccounts_GetRows", pars);
                //if (list != null && list.Count > 0)
                //{
                //    list.ForEach(p => p.UserName = StringUtil.MaskUserName(p.UserName));
                //}
                return list;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return new List<HiLoGetTopAccount>(1);
        }

        public List<HiLoGetAccountHistory> GetAccountHistory(int accountId, int betType, int topCount)
        {
            try
            {
                if (topCount > 300 || topCount < 0)
                {
                    topCount = 100;
                }
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_BetType", betType);
                pars[2] = new SqlParameter("@_TopCount", topCount);

                var db = new DBHelper(ConnectionString.GameConnectionString);
                var result = db.GetListSP<HiLoGetAccountHistory>("SP_Turns_GetHistory", pars);
                if (result != null && result.Count > topCount)
                {
                    result = result.Take(topCount).ToList();
                }
                return result;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }
            return new List<HiLoGetAccountHistory>(1);
        }

    }
}
