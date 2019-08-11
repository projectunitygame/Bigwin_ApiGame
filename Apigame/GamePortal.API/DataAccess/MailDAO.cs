using Dapper;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilities.Database;

namespace GamePortal.API.DataAccess
{
    public class MailDAO
    {
        public static int GetUnread(long accountId)
        {
            using (var sqlConnection = new SqlConnection(GateConfig.DbConfig))
            {
                var queryResult = sqlConnection.QueryFirstOrDefault($"SELECT CountUnreadMail FROM dbo.CountUnreadMail WHERE AccountId = {accountId}");
                if (queryResult != null)
                    return Convert.ToInt32(queryResult.CountUnreadMail);
                else return 0;
            }
        }

        public static List<Mail> GetAll(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            return db.GetListSP<Mail>("SP_GetAllMail", new SqlParameter("@_AccountId", accountId));
        }

        public static Mail Read(long accountId, int id)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            var _param = new List<SqlParameter>
            {
                new SqlParameter("@_AccountId", accountId),
                new SqlParameter("@_MailId", id),
            };
            return db.GetInstanceSP<Mail>("SP_ReadMail", _param.ToArray());
        }

        public static void Delete(long id, long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);
            db.ExecuteNonQuery($"UPDATE [dbo].[UserMail] SET IsDeleted = 1 WHERE AccountId = {accountId} AND MailId = {id}");
        }

        public static void DeleteAll(long accountId)
        {
            DBHelper db = new DBHelper(GateConfig.DbConfig);           
            var mail = GetAll(accountId);
            if (mail.Count > 0)
            {
                var query = "";
                foreach (var item in mail)
                {
                    query += $" UPDATE [dbo].[UserMail] SET IsDeleted = 1 WHERE AccountId = {accountId} AND MailId = {item.Id}";
                    query += " IF (@@ROWCOUNT = 0)";
                    query += $" INSERT INTO [dbo].[UserMail] (AccountId, MailId, IsDeleted) VALUES ({accountId}, {item.Id}, 1)";
                }
                db.ExecuteNonQuery(query);
            }
        }
    }
}