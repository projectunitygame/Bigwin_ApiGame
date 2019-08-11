using Minigame.MiniPokerServer.Database.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Session;

namespace Minigame.MiniPokerServer.Controllers
{
    public class TestSlotController : ApiController
    {
        [HttpOptions, HttpGet]
        [ActionName("CreateSampleData")]
        public int CreateSampleData(int cardType)
        {
            string accountName = AccountSession.AccountName;
            accountName = "Player_15364517";
            if (string.IsNullOrEmpty(accountName))
                return -13;
            return AbstractDaoMinigame.Instance().CreateMiniPokerDao().SetTestData(accountName, cardType);
        }

        [HttpGet, HttpOptions]
        [ActionName("GetSampleData")]
        public int GetSampleData()
        {
            string accountName = AccountSession.AccountName;
            accountName = "Player_15364517";
            if (string.IsNullOrEmpty(accountName))
                return -13;
            return AbstractDaoMinigame.Instance().CreateMiniPokerDao().GetTestData(accountName);
        }
    }
}
