using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Utilities.Database;

namespace OTP
{
    public class OtpDAO
    {
        public static OtpParam GenerateCounter(long accountId, out int responseStatus)
        {
            DBHelper db = new DBHelper(Config.DbConfig);
            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_ResponseStatus", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
            };

            var data = db.GetInstanceSP<OtpParam>("[otp].[SP_GetCounter]", pars.ToArray());
            responseStatus = Convert.ToInt32(pars[1].Value);
            return data;
        }

        public static void SetToken(long accountId, string token)
        {
            DBHelper db = new DBHelper(Config.DbConfig);
            db.ExecuteNonQuery($"update [otp].[Counter] set AppToken = '{token}' where AccountId = {accountId}");
        }

        public static OtpParam GetCurrentCounter(long accountId)
        {
            DBHelper db = new DBHelper(Config.DbConfig);
            return db.GetInstance<OtpParam>($"SELECT Counter C, Token T, AppToken AppT FROM [otp].[Counter] where AccountId = {accountId}");
        }
    }
}