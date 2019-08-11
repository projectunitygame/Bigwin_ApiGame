using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities.Database;
using Utilities.Log;

namespace GamePortal.API.DataAccess
{
    public class SecurityDAO
    {
        public static void ChangePassword(long accountId, string old, string pass)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            db.ExecuteNonQuery($"update dbo.Account set Password = '{pass}' where AccountID = {accountId} and Password = '{old}'\n" +
                $"if @@rowcount = 0 throw 50000, 'wrong pass', 1");
        }

        public static void CreateNewPassword(long accountId, string newPass)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            db.ExecuteNonQuery($"update dbo.Account set Password = '{newPass}' where AccountID = {accountId}");
        }

        public static AccountSecurity GetInfo(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<AccountSecurity>($"select * from dbo.Account where accountid = {accountId}");
        }

        public static bool UpdateInfo(long accountId, string identityCN, string email, string phoneNumber)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            db.ExecuteNonQuery($"update dbo.Account set IdentityCN = '{identityCN}', Email = '{email}', Tel = '{phoneNumber}' where AccountID = ${accountId}");

            return true;
        }

        public static bool UpdateEmail(long accountId, string email)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            db.ExecuteNonQuery($"update dbo.Account set Email = '{email}' where AccountID = ${accountId}");

            return true;
        }

        public static bool UpdatePhoneNumber(long accountId, string phoneNumber)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            db.ExecuteNonQuery($"update dbo.Account set Tel = '{phoneNumber}' where AccountID = ${accountId}");

            return true;
        }

        public static bool UpdateAvatar(long accountId, int avatarId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            db.ExecuteNonQuery($"update dbo.Account set AvatarID = '{avatarId}' where AccountID = ${accountId}");
            return true;
        }

        public static bool UpdateRegisterSMSPlus(long accountId, bool isCancel)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            var status = isCancel ? 0 : 1;
            db.ExecuteNonQuery($"UPDATE dbo.Account SET IsOTP = {status} WHERE AccountID = {accountId}");
            return true;
        }

        public static LockedGoldInfo GetLockedGoldInfo(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<LockedGoldInfo>($"SELECT TOP 1 [Username], [Gold], [LockedGold] FROM dbo.Account WHERE AccountId = {accountId}");
        }

        public static bool UpdateLockGold(long accountId, long amount, int typeLock, string description, out long currentGold)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            List<SqlParameter> pars = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_Amount", amount),
                new SqlParameter("@_TypeLock", typeLock),
                new SqlParameter("@_Description", description),
                new SqlParameter("@_CurrGold", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output }
            };

            db.ExecuteNonQuerySP("SP_UpdateLockGold", pars.ToArray());
            currentGold = Convert.ToInt64(pars[4].Value);

            return true;
        }

        public static AccountOTPInfo GetOTPInfo(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<AccountOTPInfo>($"SELECT TOP 1 [Tel], [IsOTP] FROM dbo.Account WHERE AccountId = {accountId}");
        }

        public static Account GetByIdPass(long accountId, string pass)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetInstance<Account>($"SELECT TOP 1 * FROM dbo.Account WHERE AccountId = {accountId} AND Password = '{pass}'");
        }

        public static List<GoldLockTransaction> GetLockGoldTransaction(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);

            return db.GetList<GoldLockTransaction>($"SELECT TOP 50 * FROM [log].[GoldLockTransaction] WHERE AccountId = {accountId} ORDER BY CreatedTime DESC");
        }
    }
}