using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Transaction
{
    public class ExchangeController : ApiController
    {
        [Authorize, HttpOptions, HttpGet]
        public AccountBalance Convert(long amount)
        {
            try
            {
                if (amount <= 0)
                    return null;

                var result = TransactionDAO.ExchangeCoin(AccountSession.AccountID, amount);
                if (result == null || result.Coin <= 0)
                    return null;
                return result;
            } catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }
    }
}
