using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DataAccess.DTO;
using DataAccess.Factory;
using MinigameVuabai.SignalR.Models;
using System.Configuration;
using Utilities.Session;

namespace MinigameVuabai.SignalR.Controllers
{
    public class GameController : ApiController
    {
        [HttpGet, HttpOptions]
        [ActionName("GetNotification")]
        public List<HistoryInfor> Vuabai_GetNotification(int topCount, int betType = 1)
        {
            if (CheckBlockedUser())
            {
                return new List<HistoryInfor>();
            }
            var lst = new List<HistoryInfor>();
            lst = AbstractDaoFactory.Instance().CreateEventDao().SP_SlotKingPocker_GetNotification(betType);

            return lst;
        }

        [HttpGet, HttpOptions]
        [ActionName("GetHistory")]
        public List<HistoryInfor> Vuabai_GetHistory(int topCount, int betType = 1)
        {
            if (CheckBlockedUser())
            {
                return new List<HistoryInfor>();
            }
            var lst = new List<HistoryInfor>();
            long accountId = AccountSession.AccountID;
            if (accountId < 1)
                return lst;
            //if (topCount > 200 || topCount < 1)
            //{
            //    topCount = 200;
            //}
            lst = AbstractDaoFactory.Instance().CreateEventDao().SP_SlotKingPocker_GetHisrorySpin(accountId, topCount, betType);
            if (lst != null)
            {
                //NLogLogger.LogInfo(string.Format("GetHistory =>accId: {0} | accName: {1} | currPage: {2} | recordPerPage :{3} | BetType: {4}, TopCount: {5}", AccountSession.AccountID, AccountSession.AccountID, currPage, recordPerPage, 0, recordPerPage));
                return lst;
            }
            //NLogLogger.LogInfo(string.Format("GetHistory =>accId: {0} | accName: {1} | currPage: {2} | recordPerPage :{3} | BetType: {4}, TopCount: {5}", AccountSession.AccountID, AccountSession.AccountID, currPage, recordPerPage, 0, recordPerPage));
            return new List<HistoryInfor>();
        }
        private bool CheckBlockedUser()
        {
            try
            {
                string line = null;
                var source = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["BlackList"]);
                using (StreamReader sr = new StreamReader(source))
                {
                    line = sr.ReadToEnd();
                }
                if (line.Contains(";" + AccountSession.AccountID + ";"))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                NLogLogger.PublishException(ex);
                return false;
            }
        }
    }
}
