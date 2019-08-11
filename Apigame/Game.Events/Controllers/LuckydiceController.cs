using Game.Events.Database.DAOImpl;
using Game.Events.Database.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace Game.Events.Controllers
{
    public class LuckydiceController : ApiController
    {
        [HttpOptions, HttpGet]
        public bool checkEvent()
        {
            return LuckyDiceEventDAO.checkEvent();
        }

        [HttpOptions, HttpGet, Authorize]
        public dynamic getAccountEvent()
        {
            try
            {
                string id = $"{DateTime.Now.Year.ToString("D4")}{DateTime.Now.Month.ToString("D2")}{DateTime.Now.Day.ToString("D2")}{AccountSession.AccountID}";
                var accountEvent = LuckyDiceEventDAO.getAccountEvent(id);
                if (accountEvent == null)
                    return new
                    {
                        TotalWin = 0,
                        TotalLose = 0,
                        MaxWin = 0,
                        MaxLose = 0
                    };
                return accountEvent;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

        [HttpOptions, HttpGet, Authorize]
        public BetKingTime getTimeEvent()
        {
            try
            {
                return LuckyDiceEventDAO.GetTimeEvent();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

        [HttpOptions, HttpGet]
        public IEnumerable<LuckydiceRank> getTop(string day, int type)
        {
            try
            {
                if (string.IsNullOrEmpty(day))
                    day = $"{DateTime.Now.Year.ToString("D4")}{DateTime.Now.Month.ToString("D2")}{DateTime.Now.Day.ToString("D2")}";

                var ranks = LuckyDiceEventDAO.getRankEvent(day, type);
                ranks = ranks.Select((i, x) =>
                {
                    i.ID = x + 1;
                    return i;
                });
                return ranks;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new List<LuckydiceRank>();
        }
    }
}
