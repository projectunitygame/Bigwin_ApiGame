using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Utilities.Session;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Controllers
{

    public class LDController : ApiController
    {
        [HttpGet, HttpOptions]
        public List<DiceResult> GetHistory(int moneyType)
        {
            var accountId = AccountSession.AccountID;
            if (accountId < 1)
                return null;
            return GameManager.GetRecentResult(moneyType);
        }

        [HttpGet, HttpOptions]
        public List<Rank> GetRank(int moneyType)
        {
            return GameManager.GetRank(moneyType);
        }

        [HttpGet, HttpOptions]
        public SessionInfo GetSessionInfo(long sessionId, int moneyType)
        {
            return GameManager.GetSessionInfo(moneyType, sessionId);
        }

        [HttpGet, HttpOptions]
        public DataTable GetTransactionHistory(int moneyType)
        {
            long accountId = AccountSession.AccountID;
            if (accountId < 1)
                return null;
            return Lddb.Instance.GetTransactionHistory(accountId, moneyType);
        }
    }
}