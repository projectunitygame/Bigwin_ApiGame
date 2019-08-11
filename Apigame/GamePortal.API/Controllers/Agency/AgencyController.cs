using GamePortal.API.DataAccess;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Agency
{
    public class AgencyController : ApiController
    {
        [Authorize, HttpOptions, HttpGet]
        public List<Models.Agency> GetAll()
        {
            try
            {
                if(!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CloseAgencies"]))
                    return new List<Models.Agency>();
                var accountSandbox = ConfigurationManager.AppSettings["AccountSandbox"].Split(',').ToList();
                var accountId = AccountSession.AccountID;
                if (accountSandbox.Count() > 0)
                {
                    if (accountSandbox.Count(x => x == accountId.ToString()) > 0)
                    {
                        return AgencyDAO.GetAllAgency_v1();
                    }
                }
                return AgencyDAO.GetAllAgency();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }
    }
}
