using Minigame.HooHeyHowServer.Database;
using Minigame.HooHeyHowServer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Utilities.Session;

namespace Minigame.HooHeyHowServer.Controllers
{
    [EnableCors(origins: "http://localhost:51712", headers: "*", methods: "*")]
    public class HooHeyHowController : ApiController
    {
        [HttpGet, HttpOptions, Authorize]
        public DataTable GetTransactionHistory(int moneyType)
        {
            long accountId = AccountSession.AccountID;
            return HooHeyHowDB.Instance.GetTransactionHistory(accountId, moneyType);
        }

        [HttpGet, HttpOptions, Authorize]
        public List<Rank> GetRank(int moneyType)
        {
            return HooHeyHowDB.Instance.GetRank(moneyType);
        }

        [HttpGet, HttpOptions, Authorize]
        public List<GameResult> GetHistory()
        {
            var results = HooHeyHowDB.Instance.GetRecentResult();
            return results;
        }
    }
}
