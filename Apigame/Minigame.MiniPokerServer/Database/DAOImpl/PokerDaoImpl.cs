using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigame.MiniPokerServer.Database.DAO;
using Minigame.MiniPokerServer.Database.DTO;
using Newtonsoft.Json;
using Minigame.MiniPokerServer.Models;
using Utilities.Database;
using Utilities.Log;

namespace Minigame.MiniPokerServer.Database.DAOImpl
{
    public class PokerDaoImpl : IPokerDao
    {

        /// <summary>
        /// Quay 
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="betType"></param>
        /// <param name="roomId"></param>
        /// <param name="ip"></param>
        /// <param name="sourceId"></param>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        public MiniPokerSpinResponse Spin(
                         long accountId, string accountName, int betType, int roomId,
                         string ip, int sourceId, int merchantId, int mobilePl, bool isbot = false, bool nohu = false)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[15];
                pars[0] = new SqlParameter("@_AccountID", (int)accountId);
                pars[1] = new SqlParameter("@_Username", accountName);
                pars[2] = new SqlParameter("@_BetType", betType);
                pars[3] = new SqlParameter("@_RoomId", roomId);
                pars[4] = new SqlParameter("@_ClientIP", ip);
                pars[5] = new SqlParameter("@_SourceID", sourceId);
                pars[6] = new SqlParameter("@_MerchantID", merchantId);
                pars[7] = new SqlParameter("@_SpinID", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[8] = new SqlParameter("@_BetValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[9] = new SqlParameter("@_PrizeValue", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[10] = new SqlParameter("@_Balance", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[11] = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
                pars[12] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) { Direction = ParameterDirection.Output };
                pars[13] = new SqlParameter("@_IsBot", isbot);
                pars[14] = new SqlParameter("@_NoHu", nohu);

                List<MiniPokerListCardModel> cards = db.GetListSP<MiniPokerListCardModel>("SP_Spins_CreateTransaction", pars);

                int responseStatus;
                if (!int.TryParse(pars[12].Value.ToString(), out responseStatus))
                    throw new Exception("No responseStatus");

                MiniPokerSpinResponse data = new MiniPokerSpinResponse();
                if (responseStatus < 0)
                {
                    data.ResponseStatus = responseStatus;
                }
                else
                {
                    long longValue = 0;
                    data.AccountID = (int)accountId;
                    data.BetType = betType;
                    data.Cards = cards;
                    data.SpinID = long.TryParse(pars[7].Value.ToString(), out longValue) ? longValue : 0;
                    data.BetValue = long.TryParse(pars[8].Value.ToString(), out longValue) ? longValue : 0;
                    data.PrizeValue = long.TryParse(pars[9].Value.ToString(), out longValue) ? longValue : 0;
                    data.Balance = long.TryParse(pars[10].Value.ToString(), out longValue) ? longValue : 0;
                    data.Jackpot = long.TryParse(pars[11].Value.ToString(), out longValue) ? longValue : 0;
                }

                NLogManager.LogMessage(string.Format("PokerSpin:Acc:{0}|User:{1}|BetType:{2}|Room:{3}|PrizeValue:{4}|" +
                                                     "Respone:{5}|Ip:{6}|Data:{7}",
                    accountId, accountName, betType, roomId, data.PrizeValue, data.ResponseStatus, ip, JsonConvert.SerializeObject(cards)));
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
            return null;
        }

        /// <summary>
        /// Lấy giá trị jackpot hiện tại
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="accountName"></param>
        /// <param name="betType"></param>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public long GetJackpot(int betType, int roomId)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_BetType", betType);
                pars[1] = new SqlParameter("@_RoomId", roomId);
                pars[2] = new SqlParameter("@_Jackpot", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

                db.ExecuteNonQuerySP("SP_Funds_GetJackpot", pars);
                long returnValue = 0;
                return long.TryParse(pars[2].Value.ToString(), out returnValue) ? returnValue : 0;
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
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

        public List<MiniPokerTopWinnerModel> GetTopWinners(int betType, int topCount)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_BetType", betType);
                pars[1] = new SqlParameter("@_TopCount", topCount);

                var list = db.GetListSP<MiniPokerTopWinnerModel>("SP_Spins_GetTopWinners", pars);
                //if (list != null)
                //{
                //    list.ForEach(p => p.Username = StringUtil.MaskUserName(p.Username));
                //}
                return list;
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
            return new List<MiniPokerTopWinnerModel>(1);
        }

        public List<MiniPokerAccountHistory> GetAccountHistory(int accountId, int betType, int topCount)
        {
            DBHelper db = null;
            try
            {
                if (topCount > 300 || topCount < 0)
                {
                    topCount = 100;
                }
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_BetType", betType);
                pars[2] = new SqlParameter("@_TopCount", topCount);
                var list = db.GetListSP<MiniPokerAccountHistory>("SP_Spins_GetAccountHistory", pars);
                //SP_MiniPoker_GetAccountHistoryDetails mobilePL
                if (list != null && list.Count > topCount)
                {
                    list = list.Take(topCount).ToList();
                }
                return list;
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
            return new List<MiniPokerAccountHistory>();
        }

        public List<MiniPokerAccountHistoryDetail> GetAccountHistoryDetails(int accountId, int betType, long spinID)
        {
            DBHelper db = null;
            try
            {
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[3];
                pars[0] = new SqlParameter("@_AccountID", accountId);
                pars[1] = new SqlParameter("@_BetType", betType);
                pars[2] = new SqlParameter("@_SpinID", spinID);
                return db.GetListSP<MiniPokerAccountHistoryDetail>("SP_Spins_GetAccountHistoryDetails", pars);
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
            return new List<MiniPokerAccountHistoryDetail>(1);
        }

        public int SetTestData(string accountName, int cardType)
        {
            try
            {
                 DBHelper db = null;
                 db = new DBHelper(ConnectionString.GameConnectionString);
                 var pars = new SqlParameter[3];
                 pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_CardType", cardType);
                pars[2] = new SqlParameter("@_ResponseStatus", SqlDbType.Int) {Direction = ParameterDirection.Output};
                db.ExecuteNonQuerySP("SP_Spins_SetSlotsDataTest", pars);
                return pars[2].Value != DBNull.Value ? int.Parse(pars[2].Value.ToString()) : -301;
            }
            catch(Exception ex)
            {
                NLogManager.PublishException(ex);
                return -302;
            }
        }

        public int GetTestData(string accountName)
        {
            try
            {
                DBHelper db = null;
                db = new DBHelper(ConnectionString.GameConnectionString);
                var pars = new SqlParameter[2];
                pars[0] = new SqlParameter("@_AccountName", accountName);
                pars[1] = new SqlParameter("@_CardType", SqlDbType.Int) { Direction = ParameterDirection.Output };
                db.ExecuteNonQuerySP("SP_Spins_GetSlotsDataTest", pars);
                return pars[1].Value != DBNull.Value ? int.Parse(pars[1].Value.ToString()) : -301;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return -302;
            }
        }
    }
}
