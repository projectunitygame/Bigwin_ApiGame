using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Session;

namespace SlotGame._20Lines.Game1.Controllers.API
{
    public class HelperController : ApiController
    {
        [HttpGet]
        public bool CheckAuthenticated()
        {
            return AccountSession.AccountID > 0;
        }

        [HttpOptions, HttpGet] 
        public long GetAccountId()
        {
            long accountId = AccountSession.AccountID;
            return accountId;
        }
    }
}