
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataAccess.Factory;
using Utilities.Session;

namespace SlotMachine.Mini.TheSpinOfGod.Controllers
{
    public class HistoryController : ApiController
    {
        // GET api/<controller>
        [HttpGet]
        public DataTable GetHistory()
        {
            long accountId = AccountSession.AccountID;
            if (accountId < 1)
                return null;
            return AbstractDaoFactory.Instance().CreateEventDao().GetHistory(accountId, 100);
        }
        [HttpGet]
        public DataTable GetSpinDetails(int spinId)
        {
            return AbstractDaoFactory.Instance().CreateEventDao().GetSpinDetails(spinId);
        }
        public DataTable GetJackpotHistory()
        {
            return AbstractDaoFactory.Instance().CreateEventDao().GetJackpotHistory(100);
        }
        [HttpGet]
        public DataTable GetBigWinData()
        {
            return AbstractDaoFactory.Instance().CreateEventDao().GetBigWinData(100);
        }
        [HttpGet]
        public string GetSlotsDataTest()
        {
            string accountName = AccountSession.AccountName;
            if (string.IsNullOrEmpty(accountName))
                return null;
            return AbstractDaoFactory.Instance().CreateEventDao().GetSlotsDataTest(accountName);
        }
        [HttpGet]
        public int SetSlotsDataTest(string slotData)
        {
            string accountName = AccountSession.AccountName;
            if (string.IsNullOrEmpty(accountName))
                return -33;
            return AbstractDaoFactory.Instance().CreateEventDao().SetSlotsDataTest(accountName, slotData);
        }
    }
}