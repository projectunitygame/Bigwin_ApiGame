using GamePortal.API.DataAccess;
using GamePortal.API.Models.SMS;
using OTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Utilities.Encryption;
using Utilities.Log;

namespace GamePortal.API
{
    /// <summary>
    /// Summary description for OTP
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class OTPService : System.Web.Services.WebService
    {

        [WebMethod]
        public bool ValidateOTP(long accountId, string tel, string otp)
        {
            try
            {
                var infoApp = OtpDAO.GetCurrentCounter(accountId);
                string token = infoApp?.AppT;
                if (!string.IsNullOrEmpty(infoApp?.AppT))
                {
                    if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{token}")}_{token}", otp))
                        return true;
                }

                if (string.IsNullOrEmpty(otp) || (!OTP.OTP.ValidateOTP(accountId, otp, tel)))
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        [WebMethod]
        public bool GetOTP(long accountId, string tel)
        {
            try
            {
                var status = OTP.OTP.GenerateOTP(accountId, tel);
                NLogManager.LogMessage("OTP Agency: " + status);
                if (int.Parse(status) < 0) return false;
                bool deduct = TransactionDAO.DeductGold(accountId, 1000, "Phí dịch vụ OTP", 2);
                if (!deduct)
                {
                    NLogManager.LogMessage("Tru phi dich vu OTP dai ly that bai: " + accountId);
                    return false;
                }
                SmsService.SendMessage(tel, $"Ma xac nhan: " + status);
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
