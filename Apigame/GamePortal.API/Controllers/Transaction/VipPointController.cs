using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Transaction
{
    public class VipPointController : ApiController
    {
        [HttpOptions, HttpGet, Authorize]
        public ShortVipPoint GetShortInfoVipPoint()
        {
            var accountId = AccountSession.AccountID;
            return VipPointDAO.GetShortInfoVipPoint(accountId);
        }

        [HttpOptions, HttpGet, Authorize]
        public List<VipPointDatabase> GetVipPointDataBase()
        {

            var accountId = AccountSession.AccountID;
            List<VipPointDatabase> vipPointDatabaseList = VipPointDAO.GetVipPointDatabase();
            return vipPointDatabaseList;
        }

        [HttpOptions, HttpGet, HttpPost, Authorize]
        public ShortVipPoint ExchangeVipPoint(ExChangeVipPointData data)
        {
            long accountId = AccountSession.AccountID;
            try
            {
                int captchaVeriryStatus = Utilities.Captcha.Verify(data.capcha, data.capchaToken);
                if (captchaVeriryStatus < 0) return new ShortVipPoint { ResponseStatus = captchaVeriryStatus };

                ShortVipPoint shortVipPoint = VipPointDAO.ExchangeVipPoint(accountId, data.vipPoint);
                return shortVipPoint;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ShortVipPoint
            {
                ResponseStatus = -200
            };

        }
    }

    public class ExChangeVipPointData
    {
        public int vipPoint { get; set; }
        public string capcha { get; set; }
        public string capchaToken { get; set; }

    }
}