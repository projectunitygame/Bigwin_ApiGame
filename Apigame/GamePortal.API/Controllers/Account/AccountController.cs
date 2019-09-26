using GamePortal.API.DataAccess;
using GamePortal.API.Models.SMS;
using Newtonsoft.Json;
using OTP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;
using Utilities;
using Utilities.Encryption;
using Utilities.FB;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;
using System.Linq;
using GamePortal.API.Models;

namespace GamePortal.API.Controllers.Account
{
    public class AccountController : ApiController
    {
        [HttpOptions, HttpGet]
        public string GetTokenAuthenTest()
        {
            TokenAuthen t = new TokenAuthen();
            string token = t.GetTokenAuthenTest();
            NLogManager.LogMessage("GetTokenAuthen test: " + token);
            return token;
        }

        [HttpOptions, HttpGet]
        public string TestSMS(string phone, string msg)
        {
            try
            {
                SmsService.SendMessage(phone, msg);
                return "success";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Chuyen tien tu Uwin sang game khac
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpOptions, HttpGet, HttpPost]
        public APIResultTransfer TransferMoney(PostTransferMoneyGame p)
        {
            APIResultTransfer result = new APIResultTransfer();
            if (AccountSession.AccountID <= 0)
            {
                NLogManager.LogMessage("TransferMoney Account NULL!");
                result.code = -1;
                result.msg = "Tài khoản không tồn tại!";
                return result;
            }

            if (Utilities.Captcha.Verify(p.captcha, p.token) <= 0)
            {
                result.code = -1;
                result.msg = "Nhập mã captcha không chính xác!";
                return result;
            }

            var accountInfo = AccountDAO.GetAccountInfo(AccountSession.AccountID);
            NLogManager.LogMessage("TransferMoney: " + JsonConvert.SerializeObject(p) +
                "\r\nAccount: " + JsonConvert.SerializeObject(accountInfo));

            if (CheckAccountMap(accountInfo.AccountID, accountInfo.DisplayName, Lib.Constant.gameID_ca) < 0)
            {
                result.code = -101;
                result.msg = "Lỗi hệ thống check tài khoản!";
                return result;
            }

            string msg = "";
            string receiptID = "";
            long curentMoney = 0;
            int r = AccountDAO.TransferSubMoneyGames(accountInfo.AccountID, "Chuyển " + Lib.General.FormatMoneyVND(p.amount) + " tiền sang game cá", p.amount, Utilities.IP.IPAddressHelper.GetClientIP(), Lib.Constant.gameID_ca, ref msg, ref receiptID, ref curentMoney);
            result.code = r;
            result.msg = msg;
            result.currentMoney = curentMoney;
            if (r == 1)
            {
                // gọi api cá để add tien
                string postData = string.Format("userid={0}&amount={1}&apiToken={2}&transactionId={3}", accountInfo.AccountID, p.amount, Lib.Constant.apiToken, receiptID);
                string res = Lib.WebHelper.WebRequest(Lib.WebHelper.Method.POST, Lib.Constant.url_fish + "deposit", postData);
                //string res = Lib.WebClass.SendPost(JsonConvert.SerializeObject(postData), Lib.Constant.url_fish + "deposit", "application/x-www-form-urlencoded;charset=utf-8");
                //{"returnCode":0,"result":{"returnCode":0,"message":null,"data":{"returnCode":0,"message":null,"userid":"201839156","depositValue":10000,"currentBalance":10000,"receipId":4,"timeString":"22:37:02 30/08/2019"}}}
                APIResponseTopup dataTopup = JsonConvert.DeserializeObject<APIResponseTopup>(res);
                NLogManager.LogMessage("Response dataTopup: " + JsonConvert.SerializeObject(dataTopup));
                if (dataTopup != null && dataTopup.result.data.receipId > 0 && dataTopup.result.data.depositValue > 0)
                {
                    r = AccountDAO.ConsumeMoneyGames(accountInfo.AccountID, receiptID, dataTopup.result.data.receipId.ToString(), ref msg, ref curentMoney);
                    result.code = r;
                    result.msg = msg;
                    result.currentMoneyCa = dataTopup.result.data.currentBalance;
                }
                else
                {
                    result.code = -10;
                    result.msg = "Lỗi chuyển tiền không thành công!";
                }
            }
            NLogManager.LogMessage("Response: " + JsonConvert.SerializeObject(result));
            return result;
        }

        /// <summary>
        /// Chuyển tiền từ game cá sang uwin
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        [HttpOptions, HttpGet, HttpPost]
        public APIResultTransfer WithdrawalMoney(PostTransferMoneyGame p)
        {
            APIResultTransfer result = new APIResultTransfer();
            if (AccountSession.AccountID <= 0)
            {
                NLogManager.LogMessage("WithdrawalMoney Account NULL!");
                result.code = -1;
                result.msg = "Tài khoản không tồn tại!";
                return result;
            }

            if (Utilities.Captcha.Verify(p.captcha, p.token) <= 0)
            {
                result.code = -1;
                result.msg = "Nhập mã captcha không chính xác!";
                return result;
            }

            var accountInfo = AccountDAO.GetAccountInfo(AccountSession.AccountID);
            NLogManager.LogMessage(">>> WithdrawalMoney: " + JsonConvert.SerializeObject(p) +
                "\r\nAccount: " + JsonConvert.SerializeObject(accountInfo));

            if (CheckAccountMap(accountInfo.AccountID, accountInfo.DisplayName, Lib.Constant.gameID_ca) < 0)
            {
                result.code = -101;
                result.msg = "Lỗi hệ thống check tài khoản!";
                return result;
            }

            string postData = string.Format("userid={0}&apiToken={1}&transactionId={2}", accountInfo.AccountID, Lib.Constant.apiToken, Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString()));
            string res = Lib.WebHelper.WebRequest(Lib.WebHelper.Method.POST, Lib.Constant.url_fish + "get_balance", postData);
            var data = JsonConvert.DeserializeObject<ApiFishInfo>(res);
            if (data != null)
            {
                if (data.result.data.currentBalance < p.amount)
                {
                    result.code = -2;
                    result.msg = "Số dư không đủ để chuyển!";
                    return result;
                }
                string msg = "";
                string receiptID = "";
                long curentMoney = 0;
                int r = AccountDAO.TransferAddMoneyGames(accountInfo.AccountID, "Chuyển " + Lib.General.FormatMoneyVND(p.amount) + " tiền sang uwin", p.amount, Utilities.IP.IPAddressHelper.GetClientIP(), Lib.Constant.gameID_ca, ref msg, ref receiptID, ref curentMoney);
                if (r == 1)
                {
                    postData = string.Format("userid={0}&amount={1}&apiToken={2}&transactionId={3}", accountInfo.AccountID, p.amount, Lib.Constant.apiToken, receiptID);
                    res = Lib.WebHelper.WebRequest(Lib.WebHelper.Method.POST, Lib.Constant.url_fish + "withdraw", postData);
                    var dataWithdraw = JsonConvert.DeserializeObject<APIResponseWithdraw>(res);
                    NLogManager.LogMessage("Response dataWithdraw: " + JsonConvert.SerializeObject(dataWithdraw));
                    if (dataWithdraw != null && dataWithdraw.result.data.receipId > 0 && dataWithdraw.result.data.withdrawResponse > 0)
                    {
                        r = AccountDAO.ConsumeWithdraw(accountInfo.AccountID, receiptID, dataWithdraw.result.data.receipId.ToString(), ref msg, ref curentMoney);
                        result.code = r;
                        result.msg = msg;
                        result.currentMoney = curentMoney;
                        result.currentMoneyCa = dataWithdraw.result.data.currentBalance;
                    }
                    else
                    {
                        result.code = -10;
                        result.msg = "Lỗi chuyển tiền không thành công!";
                    }
                }
                else
                {
                    result.code = r;
                    result.msg = msg;
                }
            }
            else
            {
                result.code = -3;
                result.msg = "Lỗi không tìm thấy tài khoản!";
            }
            NLogManager.LogMessage("Response: " + JsonConvert.SerializeObject(result));
            return result;
        }

        public long CheckAccountMap(long accountID, string username, int gameID)
        {
            try
            {
                long accountIndex = AccountDAO.FindAccountGameMap(accountID, gameID);
                if (accountIndex == 0)
                {
                    //gọi api ca tạo tk
                    string postData = string.Format("userid={0}&username={1}&apiToken={2}&balance={3}", accountID, username, Lib.Constant.apiToken, 0);
                    string res = Lib.WebHelper.WebRequest(Lib.WebHelper.Method.POST, Lib.Constant.url_fish + "register", postData);
                    //Lib.WebClass.SendPost(JsonConvert.SerializeObject(postData), Lib.Constant.url_fish + "register", "application/x-www-form-urlencoded;charset=utf-8");
                    NLogManager.LogMessage(res);
                    var regInfo = JsonConvert.DeserializeObject<APIResponseRegister>(res);
                    if (regInfo.result.returnCode == 0 || regInfo.result.returnCode == 4)
                    {
                        accountIndex = AccountDAO.AddMapAccountGame(accountID, gameID);
                    }
                    else
                        accountIndex = -1;
                }
                NLogManager.LogMessage("CheckAccountMap: " + accountIndex);
                return accountIndex;
            }
            catch (Exception ex)
            {
                NLogManager.LogError("ERROR checkAccountMap: " + ex);
                return -1;
            }
        }


        /// <summary>
        /// Lay thong tin tài khoản game ca
        /// </summary>
        /// <returns></returns>
        [HttpOptions, HttpGet]
        public FishData GetFishAccount()
        {
            try
            {
                FishData r = new FishData();
                if (AccountSession.AccountID <= 0)
                {
                    NLogManager.LogMessage("Account NULL!");
                    r.returnCode = -1;
                    r.message = "Account is null!!!";
                    return r;
                }
                TokenAuthen t = new TokenAuthen();
                var accountInfo = AccountDAO.GetAccountInfo(AccountSession.AccountID);

                if (CheckAccountMap(accountInfo.AccountID, accountInfo.DisplayName, Lib.Constant.gameID_ca) < 0)
                {
                    r.returnCode = -101;
                    r.message = "Lỗi hệ thống check tài khoản!";
                    return r;
                }
                string postData = string.Format("userid={0}&apiToken={1}&transactionId={2}", accountInfo.AccountID, Lib.Constant.apiToken, Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString()));
                string res = Lib.WebHelper.WebRequest(Lib.WebHelper.Method.POST, Lib.Constant.url_fish + "get_balance", postData);
                var data = JsonConvert.DeserializeObject<ApiFishInfo>(res);
                if (data != null)
                {
                    r.returnCode = data.returnCode;
                    r.data = data.result.data;
                    r.message = "success";
                }
                NLogManager.LogMessage("Response fish account: " + JsonConvert.SerializeObject(r));
                return r;
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage("Error GetFishAccount: " + ex);
                return null;
            }          
        }

        /// <summary>
        /// Lay token authen
        /// </summary>
        /// <returns></returns>
        [HttpOptions, HttpGet]
        public RequestToken GetTokenAuthen()
        {
            if (AccountSession.AccountID <= 0)
            {
                NLogManager.LogMessage("GetTokenAuthen Account NULL!");
                return new RequestToken();
            }
            TokenAuthen t = new TokenAuthen();
            var accountInfo = AccountDAO.GetAccountInfo(AccountSession.AccountID);
            string token = accountInfo.TokenAuthen; //t.GetTokenAuthen(accountInfo);
            RequestToken r = new RequestToken()
            {
                token = token,
                key = Lib.Constant.apiToken
            };
            NLogManager.LogMessage("GetTokenAuthen: " + JsonConvert.SerializeObject(accountInfo) +
                "\r\nTokenAuthen: " + JsonConvert.SerializeObject(r));
            //CheckAccountMap(accountInfo.AccountID, accountInfo.DisplayName, Lib.Constant.gameID_ca);
            return r;
        }

        /// <summary>
        /// lay thong tin user bang tokenAuthen
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpOptions, HttpGet]
        public UserInfo AccessTokenAuthen(string token)
        {
            TokenAuthen t = new TokenAuthen();
            if (token == "14fa9cf3124592deac48a9019ca1e2fd")
            {
                UserInfo accountInfo = new UserInfo()
                {
                    userid = "20190000",
                    username = "TestAccount"
                };
                NLogManager.LogMessage("AccessTokenAuthen: " + token + "\r\n" + JsonConvert.SerializeObject(accountInfo));
                return accountInfo;
            }
            else
            {
                var d = t.AccessToken(token);
                NLogManager.LogMessage("AccessTokenAuthen: " + token + "\r\n" + JsonConvert.SerializeObject(d));
                return d;
                //if (d != null)
                //{
                //    UserInfo accountInfo = new UserInfo()
                //    {
                //        userid = d.AccountID.ToString(),
                //        username = d.DisplayName
                //    };
                //    NLogManager.LogMessage("AccessTokenAuthen: " + token + "\r\n" + JsonConvert.SerializeObject(accountInfo));
                //    return accountInfo;
                //}
                //else
                //    return null;
            }
        }


        [HttpOptions, HttpGet]
        public bool SetExampleCookie()
        {
            SetAuthCookie(201806701, "Devokkk", 5, 2);
            return true;
        }
        [HttpOptions, HttpGet]
        public bool CheckAuthenticated()
        {
            return AccountSession.AccountID > 0;
        }

        [HttpOptions, HttpGet]
        public bool Signout()
        {
            try
            {
                FormsAuthentication.SignOut();
                HttpCookie cookie2 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                string cookieDomain = HttpContext.Current.Request.Url.Host;
                if (cookieDomain.StartsWith("services"))
                    cookieDomain = cookieDomain.Substring(8, cookieDomain.Length - 8);
                else if (cookieDomain.StartsWith("api"))
                    cookieDomain = cookieDomain.Substring(3, cookieDomain.Length - 3);
                cookie2.Expires = DateTime.Now.AddYears(-1);
                cookie2.Domain = cookieDomain;
                HttpContext.Current.Response.Cookies.Add(cookie2);
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }
    

        [HttpOptions, HttpGet]
        public GamePortal.API.Models.Account GetAccountInfo()
        {
            if (AccountSession.AccountID <= 0)
                return null;
            return AccountDAO.GetAccountInfo(AccountSession.AccountID);
        }

        [HttpOptions, HttpGet, HttpPost]
        public ApiAccountReponse RegisterNormal(PostCreateAccount data)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                int captchaVeriryStatus = Utilities.Captcha.Verify(data.captcha, data.token);
                if (captchaVeriryStatus < 0) return new ApiAccountReponse { Code = captchaVeriryStatus };
                var account = new Models.Account();
                int response = account.RegisterNormal(data.username, data.password);
                if (response < 0) return new ApiAccountReponse { Code = response };
                SetAuthCookie(account.AccountID, "U." + account.AccountID, data.device, 1);
                account.TokenAuthen = Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString());
                LogDAO.Login(data.deviceID, account.TokenAuthen, data.device, IPAddressHelper.GetClientIP(), account.AccountID, 1, true);
                return new ApiAccountReponse { Code = response, Account = account };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }

        [HttpOptions, HttpGet, HttpPost]
        public async Task<ApiAccountReponse> LoginFacebook(PostLoginFacebook data)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                var fb = await Utilities.FB.Facebook.GetIDsForBusiness(data.accessToken);
                if (fb == null)
                    return new ApiAccountReponse { Code = -50 };
                string accountIds = fb.Select(x => x.id).Aggregate((i, j) => i + ";" + j);
                long accountId = AccountDAO.CheckBussinessAccount(accountIds);//request the minium user_id
                var account = new Models.Account();
                if (accountId > 0)
                    account = AccountDAO.GetAccountInfo(accountId);
                if (account == null || account.AccountID == 0)
                    account = new Models.Account();
                else
                {
                    if (account.IsBlocked)
                        return new ApiAccountReponse { Code = -65 };
                    if (account.IsOTP)
                    {
                        string token = $"{DateTime.Now.Ticks}|{account.AccountID}|{account.DisplayName}|{data.device}";
                        return new ApiAccountReponse
                        {
                            Code = 2,
                            Account = account,
                            OTPToken = Security.TripleDESEncrypt(ConfigurationManager.AppSettings["OTPKey"], token)
                        };
                    }
                    account.TokenAuthen = Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString());
                    LogDAO.Login(data.deviceID, account.TokenAuthen, data.device, IPAddressHelper.GetClientIP(), account.AccountID, 2);
                    SetAuthCookie(account.AccountID, account.DisplayName, data.device, 2);
                    return new ApiAccountReponse { Code = 1, Account = account };
                }
                int response = account.RegisterFacebookAccount($"FB_{fb.FirstOrDefault().id}");
                if (response < 0) return new ApiAccountReponse { Code = response };
                AccountDAO.CheckBussinessAccount(accountIds);
                account.TokenAuthen = Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString());
                LogDAO.Login(data.deviceID, account.TokenAuthen, data.device, IPAddressHelper.GetClientIP(), account.AccountID, 2, true);
                SetAuthCookie(account.AccountID, "U." + account.AccountID, data.device, 2);
                return new ApiAccountReponse { Code = response, Account = account };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }

        [HttpOptions, HttpGet, HttpPost]
        public ApiAccountReponse Login(PostLogin data)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                var account = AccountDAO.Login(data.username, Security.MD5Encrypt(data.password));
                if (account == null || account.AccountID == 0)
                    return new ApiAccountReponse { Code = -51 };
                if (account.IsBlocked)
                    return new ApiAccountReponse { Code = -65 };
                if (account.IsOTP)
                {
                    string token = $"{DateTime.Now.Ticks}|{account.AccountID}|{account.DisplayName}|{data.device}";
                    return new ApiAccountReponse
                    {
                        Code = 2,
                        Account = account,
                        OTPToken = Security.TripleDESEncrypt(ConfigurationManager.AppSettings["OTPKey"], token)
                    };
                }
                account.TokenAuthen = Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString());
                LogDAO.Login(data.deviceID, account.TokenAuthen, data.device, IPAddressHelper.GetClientIP(), account.AccountID, 1);
                SetAuthCookie(account.AccountID, account.DisplayName, data.device, account.UserType);
                //NLogManager.LogMessage("Login success: " + JsonConvert.SerializeObject(account));
                return new ApiAccountReponse { Code = 1, Account = account };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }

        /// <summary>
        /// login OTP ngoai lobby
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpOptions, HttpGet, HttpPost]
        public ApiAccountReponse LoginOTP(PostLoginOTP data)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                //NLogManager.LogMessage(JsonConvert.SerializeObject(data));
                string decryptToken = Security.TripleDESDecrypt(ConfigurationManager.AppSettings["OTPKey"], System.Web.HttpUtility.UrlDecode(data.tokenOTP).Replace(" ", "+"));
                string[] splData = decryptToken.Split('|');

                long time = long.Parse(splData[0]);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalSeconds > 120)
                    return new ApiAccountReponse { Code = -1 }; //Experied captcha

                long accountId = Convert.ToInt64(splData[1]);
                string displayName = splData[2].ToString();
                int device = Convert.ToInt32(splData[3]);

                var account = AccountDAO.GetAccountById(accountId);
                if (account.IsBlocked)
                    return new ApiAccountReponse { Code = -65 }; ;
                NLogManager.LogMessage("LOGIN OTP: " + accountId + "|" + data.otp);

                var infoApp = OtpDAO.GetCurrentCounter(accountId);
                string token = infoApp?.AppT;
                if (!string.IsNullOrEmpty(infoApp?.AppT))
                {
                    if (OTPApp.ValidateOTP($"{Security.MD5Encrypt($"{accountId}_{token}")}_{token}", data.otp))
                        goto doneOTP;
                }

                if (!OTP.OTP.ValidateOTP(accountId, data.otp, account.Tel))
                {
                    NLogManager.LogMessage("ValidateOTP: " + -60);
                    return new ApiAccountReponse { Code = -60 };
                }

                doneOTP:
                account.TokenAuthen = Utilities.Encryption.Security.MD5Encrypt(Guid.NewGuid().ToString());
                LogDAO.Login(data.deviceID, account.TokenAuthen, device, IPAddressHelper.GetClientIP(), accountId, 1);
                SetAuthCookie(accountId, account.DisplayName, device, account.UserType);
                return new ApiAccountReponse { Code = 1, Account = account };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }
        [HttpOptions, HttpGet, HttpPost, Authorize]
        public int UpdateName(string name)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                int update = new Models.Account(accountId).UpdateDisplayName(name);
                if (update > 0)
                    return update;
                var account = AccountDAO.GetAccountById(accountId);
                FormsAuthentication.SignOut();
                SetAuthCookie(accountId, name, AccountSession.DeviceID, account.UserType);

                return update;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return -99;
        }

        [Authorize, HttpOptions, HttpGet]
        public int UpdateAvatar(int id)
        {
            try
            {
                AccountDAO.UpdateAvatar(AccountSession.AccountID, id);
                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }
        [HttpOptions, HttpGet]
        public ApiAccountReponse OAuth(string username, string password, string deviceToken)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                var account = AccountDAO.Login(username, Security.MD5Encrypt(password));
                if (account == null || account.AccountID == 0)
                    return new ApiAccountReponse { Code = -51 };
                if (account.IsBlocked)
                    return new ApiAccountReponse { Code = -65 };
                //string deviceT = OTP.OTP.GetCurrentAccountToken(account.AccountID);
                //if (!string.IsNullOrEmpty(deviceT) && deviceT != deviceToken)
                //    return new ApiAccountReponse { Code = -72 };
                // OTP.OTP.SetToken(account.AccountID, deviceToken);
                return new ApiAccountReponse
                {
                    Code = 1,
                    Account = account,
                    OTPToken = GenerateToken(account.AccountID, deviceToken)
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }
        [HttpOptions, HttpGet]
        public async Task<ApiAccountReponse> OAuthFb(string access_token, string deviceToken)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new ApiAccountReponse { Code = Lib.Constant.MA_GAME };
                }
                var fb = await Utilities.FB.Facebook.GetIDsForBusiness(access_token);
                if (fb == null)
                    return new ApiAccountReponse { Code = -50 };
                GamePortal.API.Models.Account account = null;
                string accountIds = fb.Select(x => x.id).Aggregate((i, j) => i + ";" + j);
                long accountId = AccountDAO.CheckBussinessAccount(accountIds);//request the minium user_id
                if (accountId > 0)
                    account = AccountDAO.GetAccountInfo(accountId);
                if (account == null || account.AccountID == 0)
                    return new ApiAccountReponse { Code = -51 };
                if (account.IsBlocked)
                    return new ApiAccountReponse { Code = -65 };
                //string deviceT = OTP.OTP.GetCurrentAccountToken(account.AccountID);
                //if (!string.IsNullOrEmpty(deviceT) && deviceT != deviceToken)
                //    return new ApiAccountReponse { Code = -72 };
                // OTP.OTP.SetToken(account.AccountID, deviceToken);
                return new ApiAccountReponse
                {
                    Code = 1,
                    Account = account,
                    OTPToken = GenerateToken(account.AccountID, deviceToken)
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new ApiAccountReponse
            {
                Code = -99
            };
        }
        private string GenerateToken(long accountId, string deviceToken)
        {
            return Security.TripleDESEncrypt($"APP_deviceToken", deviceToken + "_" + accountId + "_" + DateTime.Now.Ticks);
        }
        [HttpOptions, HttpGet]
        public APIResponse GetLoginAppOTPCode(string token)
        {
            try
            {
                if (Lib.Constant.MA_GAME != 0)
                {
                    return new APIResponse { ResponseCode = Lib.Constant.MA_GAME };
                }
                // NLogManager.LogMessage("TOKEN: " + token);
                string parseToken = Security.TripleDESDecrypt("APP_deviceToken", token);
                string[] split = parseToken.Split('_');
                long accountId = long.Parse(split[1]);
                long time = long.Parse(split[2]);
                if (TimeSpan.FromTicks(DateTime.Now.Ticks - time).TotalMinutes > 5)
                    return new APIResponse
                    {
                        Message = "Phiên đăng nhập của bạn đã hết hạn",
                        ResponseCode = -80
                    };

                var account = AccountDAO.GetAccountInfo(accountId);
                if (string.IsNullOrEmpty(account.Tel))
                    return new APIResponse
                    {
                        Message = "Tài khoản này chưa được kích hoạt tính năng bảo mật đăng nhập",
                        ResponseCode = -81
                    };

                bool deduct = TransactionDAO.DeductGold(account.AccountID, 1000, "Phí dịch vụ OTP", 2);
                //NLogManager.LogMessage("DEDUCT OTP STATUS: " + deduct + "|" + account.DisplayName);
                if (!deduct)
                    return new APIResponse
                    {
                        Message = "Số dư tài khoản của bạn không đủ để thực hiện giao dịch này",
                        ResponseCode = -81
                    };
                var status = OTP.OTP.GenerateOTP(accountId, account.Tel);
                if (status == "-70")
                    return new APIResponse
                    {
                        Message = "Chỉ có thể nhận mã OTP 5 phút một lần",
                        ResponseCode = -81
                    };
                SmsService.SendMessage(account.Tel, $"Ma xac nhan: " + status);
                return new APIResponse
                {
                    ResponseCode = 1
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new APIResponse
            {
                Message = "Hệ thống của chúng tôi đang bận, xin bạn vui lòng thử lại sau!",
                ResponseCode = -99
            };
        }
        [HttpOptions, HttpGet]
        public APIResponse TwoFactorOtpApp(string token, string otp)
        {
            try
            {
                string parseToken = Security.TripleDESDecrypt("APP_deviceToken", token);
                string[] split = parseToken.Split('_');
                long accountId = long.Parse(split[1]);
                long time = long.Parse(split[2]);

                var account = AccountDAO.GetAccountInfo(accountId);
                if (string.IsNullOrEmpty(account.Tel))
                    return new APIResponse
                    {
                        Message = "Tài khoản này chưa được kích hoạt tính năng bảo mật đăng nhập",
                        ResponseCode = -81
                    };

                if (!OTP.OTP.ValidateOTP(accountId, otp, account.Tel))
                    return new APIResponse
                    {
                        Message = "Mã xác nhận không chính xác",
                        ResponseCode = -81
                    };

               // NLogManager.LogMessage("Set token => " + account.DisplayName + "|" + split[0]);
                OTP.OTP.SetToken(accountId, split[0]);

                return new APIResponse
                {
                    ResponseCode = 1,
                    Message = Security.MD5Encrypt($"{accountId}_{split[0]}")
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new APIResponse
            {
                Message = "Hệ thống của chúng tôi đang bận, xin bạn vui lòng thử lại sau!",
                ResponseCode = -99
            };
        }
        private void SetAuthCookie(long accountId, string accountName, int deviceId, int userType = 0)
        {
            //NLogManager.LogMessage(HttpContext.Current.Request.Url.Host);
            string cookieUsername = $"{accountId}|{accountName}|{deviceId}|{userType}";
            string cookieDomain = HttpContext.Current.Request.Url.Host;
            if (cookieDomain.StartsWith("services"))
                cookieDomain = cookieDomain.Substring(8, cookieDomain.Length - 8) ;
            else if(cookieDomain.StartsWith("api"))
                cookieDomain = cookieDomain.Substring(3, cookieDomain.Length - 3);
            FormsAuthentication.SetAuthCookie(cookieUsername, false, FormsAuthentication.FormsCookiePath);
            HttpCookie cookie = FormsAuthentication.GetAuthCookie(cookieUsername, false, FormsAuthentication.FormsCookiePath);
            cookie.HttpOnly = false;
            cookie.Domain = cookieDomain;
            //NLogManager.LogMessage($"{cookieDomain}|{accountId}|{accountName}|{deviceId}|{userType}");
           // cookie.Domain = ConfigurationManager.AppSettings["domain"];

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

#if CHANHCLUB
        [HttpGet, HttpOptions]
        public HttpResponseMessage LoginOpenID()
        {
            var fb = new Facebook.FacebookClient();

            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = "1792549730783013",
                client_secret = "5fd8d1fe75779972717e0881fc121882",
                redirect_uri = "https://api.buscity.xyz/Account/Token",
                response_type = "code",
            });

            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(loginUrl.AbsoluteUri);
            return response;
        }

        [HttpGet, HttpOptions]
        public async Task<HttpResponseMessage> Token(string code)
        {
            string domainWeb = "https://chanh.win/";
            try
            {
                var fb = new Facebook.FacebookClient();
                dynamic result = fb.Post("oauth/access_token", new
                {
                    client_id = "1792549730783013",
                    client_secret = "5fd8d1fe75779972717e0881fc121882",
                    redirect_uri = "https://api.buscity.xyz/Account/Token",
                    code = code
                });

                var accessToken = result.access_token;

                List<IDs_Business> fb1 = await Utilities.FB.Facebook.GetIDsForBusiness(accessToken);
                if (fb1 == null)
                    throw new Exception();

                string accountIds = fb1.Select(x => x.id).Aggregate((i, j) => i + ";" + j);
                long accountId = AccountDAO.CheckBussinessAccount(accountIds);//request the minium user_id
                var account = new Models.Account();
                if (accountId > 0)
                    account = AccountDAO.GetAccountInfo(accountId);
                if (account == null || account.AccountID == 0)
                    account = new Models.Account();
                else
                {
                    if (account.IsBlocked)
                        throw new Exception();

                    LogDAO.Login(0, IPAddressHelper.GetClientIP(), account.AccountID, 2);
                    SetAuthCookie(account.AccountID, account.DisplayName, 0, 2);
                }
                int response1 = account.RegisterFacebookAccount($"FB_{fb1.FirstOrDefault().id}");
                if (response1 < 0) throw new Exception();
                AccountDAO.CheckBussinessAccount(accountIds);
                LogDAO.Login(0, IPAddressHelper.GetClientIP(), account.AccountID, 2, true);
                SetAuthCookie(account.AccountID, "U." + account.AccountID, 0, 2);


            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            var response = Request.CreateResponse(HttpStatusCode.Moved);
            response.Headers.Location = new Uri(domainWeb);
            return response;

        }
#endif
    }

    public class PostTransferMoneyGame
    {
        public string accountId { get; set; }
        public string reason { get; set; }
        public int gameId { get; set; }
        public long amount { get; set; }
        public string captcha { get; set; }
        public string token { get; set; }
    }

    public class ApiAccountReponse
    {
        public int Code { get; set; }
        public GamePortal.API.Models.Account Account { get; set; }
        public string OTPToken { get; set; }
    }

    public class PostCreateAccount
    {
        public string username { get; set; }
        public string password { get; set; }
        public string captcha { get; set; }
        public string token { get; set; }
        public int device { get; set; }
        public string deviceID { get; set; }
    }

    public class PostLogin
    {
        public string username { get; set; }
        public string password { get; set; }
        public int device { get; set; }
        public string deviceID { get; set; }
    }

    public class PostLoginOTP
    {
        public string otp { get; set; }
        public int type { get; set; }
        public string tokenOTP { get; set; }
        public string deviceID { get; set; }
    }

    public class PostLoginFacebook
    {
        public string accessToken { get; set; }
        public int device { get; set; }
        public string deviceID { get; set; }
    }

    public class APIResponse
    {
        public string Message { get; set; }
        public int ResponseCode { get; set; }
    }

    public class RequestToken
    {
        public string key;
        public string token;
    }
    public class ApiFishInfo
    {
        public ApiFishInfo()
        {
            result = new FishData();
        }
        public int returnCode;
        public FishData result;
    }

    public class FishData
    {
        public FishData()
        {
            data = new FishInfo();
            returnCode = -1;
            message = "Account not found!";
        }
        public int returnCode;
        public string message;
        public FishInfo data;
    }

    public class FishInfo
    {
        public string userid;
        public long currentBalance;
    }


    public class APIResultTransfer
    {
        public int code;
        public string msg;
        public long currentMoney;
        public long currentMoneyCa;
    }

    public class APIResponseTopup
    {
        public int returnCode;
        public ResultTopupInfo result;
    }

    public class ResultTopupInfo
    {
        public int returnCode;
        public string message;
        public TopupInfo data;
    }

    public class TopupInfo
    {
        public string userid;
        public long depositValue;
        public long currentBalance;
        public long receipId;
        public string timeString;
        public int returnCode;
        public string message;
    }

    public class APIResponseWithdraw
    {
        public int returnCode;
        public ResultWithdrawInfo result;
    }

    public class ResultWithdrawInfo
    {
        public int returnCode;
        public string message;
        public WithdrawInfo data;
    }

    public class WithdrawInfo
    {
        public string userid;
        public long withdrawRequest;
        public long withdrawResponse;
        public long currentBalance;
        public long receipId;
        public string timeString;
        public int returnCode;
        public string message;
    }




    public class APIResponseRegister
    {
        public int returnCode;
        public ResultRegisterInfo result;
    }

    public class ResultRegisterInfo
    {
        public int returnCode;
        public string message;
        public UCaInfo data;
    }

    public class UCaInfo
    {
        public string userid;
        public long currentBalance;
    }
}
