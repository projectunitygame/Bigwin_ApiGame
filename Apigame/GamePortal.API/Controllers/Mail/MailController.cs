using GamePortal.API.DataAccess;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Mail
{
    public class MailController : ApiController
    {
        [Authorize, HttpOptions, HttpGet]
        public int GetUnread()
        {
            try
            {
                var accountId = AccountSession.AccountID;
                return MailDAO.GetUnread(accountId);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return 0;
        }

        [Authorize, HttpOptions, HttpGet]
        public List<Models.Mail> GetAll()
        {
            try
            {
                NLogManager.LogMessage("GET MAIL HISTORY => " + AccountSession.AccountID);
                var accountId = AccountSession.AccountID;
                return MailDAO.GetAll(accountId);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public Models.Mail Read(int id)
        {
            try
            {
                var accountId = AccountSession.AccountID;
                var mail = MailDAO.Read(accountId, id);
                if (mail != null)
                    mail.IsRead = true;
                return mail;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public bool Delete(int id)
        {
            try
            {
                var accountId = AccountSession.AccountID;
                MailDAO.Delete(id, accountId);
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        [Authorize, HttpOptions, HttpGet]
        public bool DeleteAll()
        {
            try
            {
                var accountId = AccountSession.AccountID;
                MailDAO.DeleteAll(accountId);
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }
    }
}