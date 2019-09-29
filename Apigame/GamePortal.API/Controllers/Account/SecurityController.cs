﻿using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using GamePortal.API.Models.AnotherLogic;
using GamePortal.API.Models.SMS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OTP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities.Encryption;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Account
{
    public class SecurityController : ApiController
    {
        [Authorize, HttpOptions, HttpGet]
        public int ChangePass(string old, string pass, string otp)
        {
            try
            {
                var accountId = AccountSession.AccountID;
                var accountInfo = AccountDAO.GetAccountInfo(accountId);

                var infoApp = OtpDAO.GetCurrentCounter(accountId);
                string token = infoApp?.AppT;
                if (!string.IsNullOrEmpty(infoApp?.AppT))
                {
                    if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{token}")}_{token}", otp))
                        goto doneOTP;
                }

                if (string.IsNullOrEmpty(otp) || (!OTP.OTP.ValidateOTP(accountId, otp, accountInfo.Tel)))
                    return -60;

                doneOTP:

                Regex rPassword = new Regex("^[a-zA-Z0-9_.-]{6,18}$");
                if (!rPassword.IsMatch(old))
                    return -30;
                if (!rPassword.IsMatch(pass))
                    return -30;

                var account = SecurityDAO.GetByIdPass(accountId, Security.MD5Encrypt(old));
                if (account == null)
                    return -31;

                SecurityDAO.ChangePassword(AccountSession.AccountID, Security.MD5Encrypt(old), Security.MD5Encrypt(pass));
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        [Authorize, HttpOptions, HttpGet]
        public AccountSecurity Info()
        {
            try
            {
                return SecurityDAO.GetInfo(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public AccountOTPInfo GetAccountOTPInfo()
        {
            try
            {
                return SecurityDAO.GetOTPInfo(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public int UpdatePhoneNumber(string phoneNumber, string captcha,string tokenCaptcha)
        {
            try
            {
                NLogManager.LogMessage("UpdatePhoneNumber:" + phoneNumber + ":" + captcha+":"+ tokenCaptcha);
                if (!PhoneDetector.IsValidPhone(phoneNumber))
                    return -54;

                var accountId = AccountSession.AccountID;
                
                var account = AccountDAO.GetAccountById(AccountSession.AccountID);
                if (AccountDAO.CheckPhoneUsed(phoneNumber))
                {
                    return -59;
                }
                if (!string.IsNullOrEmpty(account.Tel))
                {
                    string p = account.Tel;

                    int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, tokenCaptcha);
                    if (captchaVeriryStatus < 0) return -2;
                }
                else
                {
                    int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, tokenCaptcha);
                    if (captchaVeriryStatus < 0) return -2;
                }

                SecurityDAO.UpdatePhoneNumber(AccountSession.AccountID, phoneNumber);

                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        [Authorize, HttpOptions, HttpGet]
        public int UpdateRegisterSMSPlus(bool isCancel, string otp = "")
        {
            try
            {
                var accountId = AccountSession.AccountID;
                var accountInfo = AccountDAO.GetAccountInfo(accountId);

                if (string.IsNullOrEmpty(accountInfo.Tel))
                    return -99;

                if (isCancel)
                {
                    var infoApp = OtpDAO.GetCurrentCounter(accountId);
                    string token = infoApp?.AppT;
                    if (!string.IsNullOrEmpty(infoApp?.AppT))
                    {
                        if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{token}")}_{token}", otp))
                            goto doneOTP;
                    }

                    if (string.IsNullOrEmpty(otp) || (!OTP.OTP.ValidateOTP(accountId, otp, accountInfo.Tel)))
                        return -60;
                }
                doneOTP:
                SecurityDAO.UpdateRegisterSMSPlus(AccountSession.AccountID, isCancel);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        [HttpOptions, HttpGet]
        public void GenerateOtpTelegram(string phoneNumber, string chatIdTele)
        {
            if (phoneNumber.Substring(0,1) != " ")
            {
                phoneNumber = phoneNumber.Insert(0,"+");
            }
            phoneNumber = phoneNumber.Substring(3);
            phoneNumber = "0" + phoneNumber;
            long accountID = AccountDAO.GetAccountByTel(phoneNumber).AccountID;
            var otp = OTP.OTP.GenerateOTP(accountID, phoneNumber);
            
            NLogManager.LogMessage("GenerateOtpTelegram:" + phoneNumber + ":" + chatIdTele + ":" + otp);
            //OtpTelegram otpTelegram = SecurityDAO.InserOrUpdateOtp(phoneNumber, otp);
            if (otp == "-70")
            {t
                otp = "Sau 2 phút mới có thể lấy otp mới!";
            }
            else
            {
                otp = "OTP của bạn là : " + otp;
            }
            GET(string.Format("https://api.telegram.org/bot758314877:AAHZSrou7c9SyljCEf-tAKLDAt_QqK_JMDk/sendMessage?chat_id={0}&text={1}", chatIdTele, otp));
        }


        public JObject GET(string url, Dictionary<string, object> jObject = null)
        {
            JObject result = new JObject();
            var sunbLink = "";
            if (jObject != null)
            {
                sunbLink += "?";
                foreach (var key in jObject.Keys)
                {
                    sunbLink = sunbLink + key + "=" + jObject[key] + "&";
                }
                sunbLink = sunbLink.Substring(0, sunbLink.Length - 1);
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + sunbLink);
            try
            {
                WebResponse response = request.GetResponse();
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    string data = reader.ReadToEnd();
                    result = JObject.Parse(data);
                }
            }
            catch (WebException ex)
            {
                WebResponse errorResponse = ex.Response;
                using (Stream responseStream = errorResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    String errorText = reader.ReadToEnd();
                }
                throw;
            }
            return result;
        }

        [Authorize, HttpOptions, HttpGet]
        public int ReceiveOTP(string phoneNumber = "")
        {
            try
            {
                NLogManager.LogMessage("ReceiveOTP: " + phoneNumber);
                var account = AccountDAO.GetAccountById(AccountSession.AccountID);
                NLogManager.LogMessage("ReceiveOTP: " + JsonConvert.SerializeObject(account));

                //chua lay otp 1 lan free
                if (SecurityDAO.GetIsGetOtpFirst(account.AccountID).IsGetOtpFirst == 0)
                {
                    if (account.Tel != "")
                    {
                        phoneNumber = account.Tel;
                    }
                    NLogManager.LogMessage("phoneNumber:"+ phoneNumber);
                    if (phoneNumber == "" ) {
                        return -71;
                    }
                    NLogManager.LogMessage("phoneNumber1:" + phoneNumber);
                    if (!PhoneDetector.IsValidPhone(phoneNumber))
                    {
                        NLogManager.LogMessage("FAIL PHONE: " + phoneNumber);
                        return -54;
                    }
                    NLogManager.LogMessage("phoneNumber2:" + phoneNumber);
                    //send to phonenumber
                    //this case is for the first time update phone
                    var status = OTP.OTP.GenerateOTP(AccountSession.AccountID, phoneNumber);
                    NLogManager.LogMessage("OTP: " + status);
                    if (int.Parse(status) < 0) return int.Parse(status);
                    //send the otp to phone
                    SecurityDAO.UpdateIsGetOtpFirst(account.AccountID,true);
                    SmsService.SendMessage(phoneNumber, $"Ma xac nhan: " + status);

                    return 1;
                }
                else
                {
                    if (string.IsNullOrEmpty(account.Tel) == false)
                        account.Tel = phoneNumber;
                    NLogManager.LogMessage("OTP to phone: " + phoneNumber);
                    var status = OTP.OTP.GenerateOTP(AccountSession.AccountID, account.Tel);
                    NLogManager.LogMessage("OTP: " + status);
                    if (int.Parse(status) < 0) return int.Parse(status);
                    bool deduct = TransactionDAO.DeductGold(account.AccountID, 1000, "Phí dịch vụ OTP", 2);
                    NLogManager.LogMessage("deduct: " + deduct);
                    if (!deduct)
                        return -62;
                    //send the otp to phone
                    SmsService.SendMessage(account.Tel, $"Ma xac nhan: " + status);

                    return 1;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                NLogManager.LogMessage("ERROR ReceiveOTP: " + ex);
            }
            return -99;
        }

        //[Authorize, HttpOptions, HttpGet]
        //public string Test_ReceiveOTP(string phone = "")
        //{
        //    return OTP.OTP.GenerateOTP(AccountSession.AccountID, phone);
        //}

        [HttpOptions, HttpGet]
        public string ReceiveLoginOTP(string tokenOTP)
        {
            try
            {
                NLogManager.LogMessage("ReceiveLoginOTP: " + tokenOTP);
                if (string.IsNullOrEmpty(tokenOTP))
                {
                    NLogManager.LogMessage("RETURN ERROR ReceiveLoginOTP: -60");
                    return "-60";
                }

                string decryptToken = Security.TripleDESDecrypt(ConfigurationManager.AppSettings["OTPKey"], System.Web.HttpUtility.UrlDecode(tokenOTP).Replace(" ", "+"));
                string[] splData = decryptToken.Split('|');

                long time = long.Parse(splData[0]);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalSeconds > 120)
                {
                    NLogManager.LogMessage("RETURN ERROR ReceiveLoginOTP: -1");
                    return "-1"; //Experied captcha
                }
                long accountId = Convert.ToInt64(splData[1]);
                var account = AccountDAO.GetAccountById(accountId);

                var status = OTP.OTP.GenerateOTP(accountId, account.Tel);
                if (int.Parse(status) < 0) return status;
                bool deduct = TransactionDAO.DeductGold(account.AccountID, 1000, "Phí dịch vụ OTP", 2);
                if (!deduct)
                {
                    NLogManager.LogMessage("RETURN ERROR ReceiveLoginOTP: -62");
                    return "-62";
                }
                SmsService.SendMessage(account.Tel, $"Ma xac nhan: " + status);
                NLogManager.LogMessage("RETURN ReceiveLoginOTP SUCCESS: 1, " + status);
                return "1";
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                NLogManager.LogMessage("ERROR ReceiveLoginOTP: " + ex);
            }
            NLogManager.LogMessage("RETURN ERROR ReceiveLoginOTP: -99");
            return "-99";
        }

        [Authorize, HttpOptions, HttpGet]
        public List<GoldLockTransaction> GetLockGoldTransaction()
        {
            try
            {
                return SecurityDAO.GetLockGoldTransaction(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public LockedGoldInfo GetLockedGoldInfo()
        {
            try
            {
                return SecurityDAO.GetLockedGoldInfo(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="type"> 1: khóa, 2: mở </param>
        /// <returns></returns>
        [Authorize, HttpOptions, HttpGet]
        public LockGold UpdateLockGold(long amount, int typeLock, string otp = "")
        {
            try
            {
                if (amount <= 0)
                    return new LockGold
                    {
                        ResponseCode = -99
                    };

                if (typeLock == 2)
                {
                    long accountId = AccountSession.AccountID;
                    var account = AccountDAO.GetAccountById(accountId);

                    var infoApp = OtpDAO.GetCurrentCounter(accountId);
                    string token = infoApp?.AppT;
                    if (!string.IsNullOrEmpty(infoApp?.AppT))
                    {
                        if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{token}")}_{token}", otp))
                            goto doneOTP;
                    }

                    if (string.IsNullOrEmpty(otp) || (!OTP.OTP.ValidateOTP(accountId, otp, account.Tel)))
                        return new LockGold
                        {
                            ResponseCode = -60
                        };
                }
                doneOTP:
                SecurityDAO.UpdateLockGold(AccountSession.AccountID, amount, typeLock, "user lock", out long currGold);
                return new LockGold
                {
                    ResponseCode = 1,
                    CurrentGold = currGold,
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return new LockGold
            {
                ResponseCode = -99
            };
        }

        private bool IsValidEmailAddress(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            else
            {
                var regex = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return regex.IsMatch(s) && !s.EndsWith(".");
            }
        }

        [Authorize, HttpOptions, HttpGet]
        public string ReceiveForgotPassOTP(string username, int otpType, string phoneNumber)
        {
            try
            {
                NLogManager.LogMessage("ReceiveForgotPassOTP: " + username + "|" + otpType + "|" + phoneNumber);
                if (otpType == 1)
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(phoneNumber))
                        return "-99";

                var account = AccountDAO.GetAccountByUsername(username);
                if (!string.IsNullOrEmpty(account.Tel))
                    if (!PhoneDetector.IsValidPhone(phoneNumber))
                        return "-54";

                if (!string.IsNullOrEmpty(phoneNumber) && account.Tel != phoneNumber)
                    return "-73";
                else
                {
                    var status = OTP.OTP.GenerateOTP(account.AccountID, phoneNumber);
                    NLogManager.LogMessage("OTP: " + status);
                    if (int.Parse(status) < 0) return status;
                    bool deduct = TransactionDAO.DeductGold(account.AccountID, 1000, "Phí dịch vụ OTP", 2);
                    if (!deduct)
                        return "-62";
                    SmsService.SendMessage(phoneNumber, $"Ma xac nhan: " + status);

                    string token = $"{DateTime.Now.Ticks}|{account.AccountID}|{account.Tel}";
                    return Security.TripleDESEncrypt(ConfigurationManager.AppSettings["OTPKey"], token);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return "-99";
        }

        [Authorize, HttpOptions, HttpGet]
        public string RequestChangePass(string token, string otp)
        {
            try
            {
                string decryptToken = Security.TripleDESDecrypt(ConfigurationManager.AppSettings["OTPKey"], System.Web.HttpUtility.UrlDecode(token).Replace(" ", "+"));
                string[] splData = decryptToken.Split('|');

                long time = long.Parse(splData[0]);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalSeconds > 120)
                    return "-1"; //Experied captcha

                long accountId = Convert.ToInt64(splData[1]);
                string phoneNumber = splData[2].ToString();

                var infoApp = OtpDAO.GetCurrentCounter(accountId);
                string tokenOTPa = infoApp?.AppT;
                if (!string.IsNullOrEmpty(infoApp?.AppT))
                {
                    if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{tokenOTPa}")}_{tokenOTPa}", otp))
                        goto doneOTP;
                }

                if (string.IsNullOrEmpty(otp) || (!OTP.OTP.ValidateOTP(accountId, otp, phoneNumber)))
                    return "-60";

                doneOTP:

                string tokenOTP = $"{DateTime.Now.Ticks}|{accountId}|{phoneNumber}";
                return Security.TripleDESEncrypt(ConfigurationManager.AppSettings["OTPKey"], tokenOTP);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return "-99";
        }

        [Authorize, HttpOptions, HttpGet]
        public int CreatedNewPass(string tokenOTP, string newPassword)
        {
            try
            {
                string decryptToken = Security.TripleDESDecrypt(ConfigurationManager.AppSettings["OTPKey"], System.Web.HttpUtility.UrlDecode(tokenOTP).Replace(" ", "+"));
                string[] splData = decryptToken.Split('|');

                long time = long.Parse(splData[0]);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalSeconds > 120)
                    return -1; //Experied captcha

                long accountId = Convert.ToInt64(splData[1]);

                Regex rPassword = new Regex("^[a-zA-Z0-9_.-]{6,18}$");
                if (!rPassword.IsMatch(newPassword))
                    return -30;
                if (!rPassword.IsMatch(newPassword))
                    return -30;

                var account = AccountDAO.GetAccountInfo(accountId);
                if (account == null)
                    return -31;

                SecurityDAO.CreateNewPassword(AccountSession.AccountID, Security.MD5Encrypt(newPassword));
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }
    }
}
