using GamePortal.API.Models;
using GamePortal.API.Models.Topup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Dapper;
using Utilities.Log;

namespace GamePortal.API.DataAccess
{
    public class VipPointDAO
    {

        public static ShortVipPoint GetShortInfoVipPoint(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
            };
            return db.GetInstanceSP<ShortVipPoint>("SP_GetShortInfoVipPoint", pars.ToArray());
        }

        public static List<VipPointDatabase> GetVipPointDatabase()
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetList<VipPointDatabase>($"SELECT * FROM dbo.VipPoint");
        }

        public static ShortVipPoint ExchangeVipPoint(long accountId, int vipPoint)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
            };
            ShortVipPoint shortVipPoint = db.GetInstanceSP<ShortVipPoint>("SP_GetShortInfoVipPoint", pars.ToArray());
            if (shortVipPoint.VipPoint < vipPoint)
            {
                shortVipPoint.ResponseStatus = -202;
                //không đủ vippoint để exchange
                return shortVipPoint;
            }
            List<VipPointDatabase> listVipPointData = GetVipPointDatabase();
            int ratioExchange = listVipPointData[shortVipPoint.LevelVip].RatioExchange;
            shortVipPoint.VipPoint -= vipPoint;
            shortVipPoint.Gold += vipPoint * ratioExchange;
            shortVipPoint.Exp -= vipPoint * 1000000;
            for (int i = listVipPointData.Count - 1; i >= 0; i--)
            {
                if (shortVipPoint.VipPoint >= listVipPointData[i].VipPoint)
                {
                    shortVipPoint.LevelVip = i;
                    break;
                }
            }
            List<SqlParameter> pars1 = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_VipPoint", shortVipPoint.VipPoint),
                new SqlParameter("@_Gold", shortVipPoint.Gold),
                new SqlParameter("@_BetExchange", shortVipPoint.Exp),
                new SqlParameter("@_LevelVip", shortVipPoint.LevelVip),
            };
            db.ExecuteNonQuerySP("SP_ExchangeVipPoint", pars1.ToArray());
            return shortVipPoint;
        }
    }
}