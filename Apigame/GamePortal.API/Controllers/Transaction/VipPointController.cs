using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Utilities.Database;
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
            NLogManager.LogMessage("GetVipPointDataBase:"+accountId);
            List<VipPointDatabase> vipPointDatabaseList = VipPointDAO.GetVipPointDatabase();
            return vipPointDatabaseList;
        }

        [HttpOptions, HttpGet, HttpPost, Authorize]
        public ShortVipPoint ReceiveLevelVipPoint(DoiDiemVipPointData doiDiemData)
        {
            var accountId = AccountSession.AccountID;
            VipPointDatabase vpData = VipPointDAO.GetVipPointDatabase()[doiDiemData.level];
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
            };
            ShortVipPoint shortVipPoint = db.GetInstanceSP<ShortVipPoint>("SP_GetShortInfoVipPoint", pars.ToArray());
            string[] arrReward = shortVipPoint.LevelReward.Split(',');
            for (int i = 0; i < arrReward.Length; i++)
            {
                NLogManager.LogMessage("CheckVipPoint:"+arrReward[i]+":"+(doiDiemData.level+1));
                if (int.Parse(arrReward[i]) == doiDiemData.level + 1)
                {
                    shortVipPoint.ResponseStatus = -104;
                    return shortVipPoint;
                }
            }
            if (doiDiemData.level + 1 > shortVipPoint.LevelMax)
            {
                shortVipPoint.ResponseStatus = -105;
                return shortVipPoint;
            }
            shortVipPoint.Gold += vpData.RewardLevel;
            shortVipPoint.LevelReward += "," + vpData.LevelVip;
            
            List <SqlParameter> pars1 = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_Gold", shortVipPoint.Gold),
                new SqlParameter("@_LevelReward", shortVipPoint.LevelReward),
            };
            db.ExecuteNonQuerySP("SP_ReceiveLevelVipPoint", pars1.ToArray());
            return shortVipPoint;
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

    public class DoiDiemVipPointData
    {
        public int level { get; set; }

    }
}