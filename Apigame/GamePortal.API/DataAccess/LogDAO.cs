using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Database;
using Utilities.Log;

namespace GamePortal.API.DataAccess
{
    public class LogDAO
    {
        public static void Login(string deviceID, string token, int device, string ip, long accountId, int loginType, bool isRegister = false)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceID))
                    deviceID = "";
                DBHelper db = new DBHelper(GateConfig.DbConfig);
                db.ExecuteNonQuery($"insert into log.Login (AccountID, IP, DeviceType, LoginType, IsRegister) values ({accountId}, '{ip}', {device}, {loginType}, '{isRegister}') \n" +
                    $"update dbo.Account set LastActive = getdate(), LastActiveInt = cast(CONVERT(varchar(20),getdate(),112) as INT), TokenAuthen = '{token}', DeviceID = '{deviceID}' where AccountID = {accountId}" );
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
    }
}