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
    public class GiftcodeController : ApiController
    {
        [HttpOptions, HttpGet, Authorize]
        public long InputCode(string code, string captcha, string token)
        {
            try
            {
                if (!CheckCaptcha(captcha, token))
                    return -2;

                if (string.IsNullOrEmpty(code) || code.Length != 12)
                    return -55;
                var result = TransactionDAO.InputCode(AccountSession.AccountID, AccountSession.AccountName, code);
                if (result == null || result.Gold <= 0)
                    return -56;
                return result.Gold;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -56;
        }

        private bool CheckCaptcha(string captcha, string token)
        {
            if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(token))
                return false;
            int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, token);
            return captchaVeriryStatus > 0;
        }
    }
}
