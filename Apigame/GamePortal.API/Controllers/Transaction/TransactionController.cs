using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using GamePortal.API.Models.InappPurchase;
using GamePortal.API.Models.Topup;
using GamePortal.API.Payment;
using Newtonsoft.Json;
using OTP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities;
using Utilities.Encryption;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Transaction
{
    public class TransactionController : ApiController
    {
        [HttpGet, HttpOptions, Authorize]
        public long Transfer(string accountName, long amount, string reason, string captcha, string token)
        {
            NLogManager.LogMessage("Transfer info: " + accountName + "|" + amount + "|" + reason);
            try
            {
                var myAccount = AccountDAO.GetAccountById(AccountSession.AccountID);
                if (myAccount == null)
                {
                    NLogManager.LogMessage("EX Result transfer: " + -58);
                    return -58;
                }
                if (amount < 10200)
                {
                    NLogManager.LogMessage("EX Result transfer: " + -80);
                    return -80;
                }
                NLogManager.LogMessage("AccountInfo: " + JsonConvert.SerializeObject(myAccount));
                int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, token);
                if (captchaVeriryStatus < 0) return captchaVeriryStatus;

                if(accountName.Length == 13 && accountName.Substring(0,5) == "UWIN.")
                {
                    #region Chuyen gold to daily
                    var accountSandbox = ConfigurationManager.AppSettings["AccountSandbox"].Split(',').ToList();
                    NLogManager.LogMessage("accountSandbox: " + String.Join(",", accountSandbox));
                    //var accountId = AccountSession.AccountID;
                    if (accountSandbox.Count > 0)
                    {
                        if (accountSandbox.Count(x => x == myAccount.AccountID.ToString()) > 0)
                        {
                            int code = 0;
                            string msg = "";
                            string phone = "";
                            amount = (long)(amount / 1.02);
                            NLogManager.LogMessage($"Transfer to agency => {accountName}|{amount}|{reason}");
                            var d = TransactionDAO.SendGold_v1(myAccount.AccountID, accountName, amount, reason, IPAddressHelper.GetClientIP(), ref code, ref msg, ref phone);
                            NLogManager.LogMessage("result transfer agency: " + JsonConvert.SerializeObject(d) +
                                "\r\ncode: " + code +
                                "\r\nmsg: " + msg +
                                "\r\nphone: " + phone);
                            if (code == 1 && !string.IsNullOrEmpty(phone))
                            {
                                Models.SMS.SmsService.SendMessage(phone, "yeu cau chuyen tien " + myAccount.Username + " so tien " + formatMoney(amount));
                            }
                            return code == 1 ? d.Balance : code;
                        }
                    }
                    #endregion
                }
                else
                {
                    var account = AccountDAO.GetAccountByAccountName(accountName);
                    if (account == null)
                    {
                        NLogManager.LogMessage("EX Result transfer: " + -58);
                        return -58;
                    }
                    long totalTransfer = amount + (long)(amount * 0.02);
                    long r = TransactionDAO.SendGold(AccountSession.AccountID,
                        account.AccountID,
                        AccountSession.AccountName,
                        account.DisplayName,
                        account.IsAgency,
                        totalTransfer,
                        amount,
                        reason);
                    NLogManager.LogMessage("Result transfer: " + r);
                    return r;
                }


                
                //NLogManager.LogMessage($"Transfer => {accountName}|{amount}|{reason}");
                //amount = (long)(amount / 1.02);
                //var myAccount = AccountDAO.GetAccountById(AccountSession.AccountID);
                //NLogManager.LogMessage(JsonConvert.SerializeObject(myAccount));
                //if (!myAccount.IsAgency)
                //{
                //    long totalTransfer = amount + (long)(amount * 0.02);
                //    long r = TransactionDAO.SendGold(AccountSession.AccountID,
                //        account.AccountID,
                //        AccountSession.AccountName,
                //        account.DisplayName,
                //        account.IsAgency,
                //        totalTransfer,
                //        amount,
                //        reason);
                //    NLogManager.LogMessage("Result transfer: " + r);
                //    return r;
                //}
                //else
                //{
                //    var agencyInfo = AccountDAO.GetAgencyInfo(AccountSession.AccountID);
                //    if (agencyInfo.Level == 2)
                //    {
                //        long r = TransactionDAO.Transfer(
                //                agencyInfo.ID,
                //                agencyInfo.GameAccountId,
                //                agencyInfo.Username,
                //                amount,
                //                account.IsAgency ? 0 : (long)(amount * 0.02),
                //                agencyInfo.Level,
                //                reason,
                //                account.AccountID,
                //                account.DisplayName,
                //                account.IsAgency
                //            );
                //        NLogManager.LogMessage("Result transfer: " + r);
                //        return r;
                //    }
                //    else
                //    {
                //        NLogManager.LogMessage("Result transfer: " + -99);
                //        return -99;
                //    }
                //}
            }
            catch (Exception ex)
            {
                NLogManager.LogError("Transfer ERROR: " + ex);
                NLogManager.PublishException(ex);
                NLogManager.LogMessage("EX Result transfer: " + ex);
            }
            NLogManager.LogMessage("EX Result transfer: " + -99);
            return -99;
        }

        [HttpGet, HttpOptions, Authorize]
        public long Transfer2(string accountName, long amount, string reason, string otp)
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

                var account = AccountDAO.GetAccountByAccountName(accountName);
                if (account == null)
                    return -58;

                if (amount < 10200)
                    return -80;

                NLogManager.LogMessage($"Transfer => {accountName}|{amount}|{reason}");
                amount = (long)(amount / 1.02);
                var myAccount = AccountDAO.GetAccountById(AccountSession.AccountID);
                if (!myAccount.IsAgency)
                {
                    long totalTransfer = amount + (long)(amount * 0.02);
                    return TransactionDAO.SendGold(AccountSession.AccountID,
                        account.AccountID,
                        AccountSession.AccountName,
                        account.DisplayName,
                        account.IsAgency,
                        totalTransfer,
                        amount,
                        reason);
                }
                else
                {
                    var agencyInfo = AccountDAO.GetAgencyInfo(AccountSession.AccountID);
                    if (agencyInfo.Level == 2)
                        return TransactionDAO.Transfer(
                                agencyInfo.ID,
                                agencyInfo.GameAccountId,
                                agencyInfo.Username,
                                amount,
                                account.IsAgency ? 0 : (long)(amount * 0.02),
                                agencyInfo.Level,
                                reason,
                                account.AccountID,
                                account.DisplayName,
                                account.IsAgency
                            );
                    else
                        return -99;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return -99;
        }

        [HttpOptions, HttpPost]
        public string callbackbb2d([FromBody]paybb2dCallBack pay)
        {
            NLogManager.LogMessage(JsonConvert.SerializeObject(pay));
            try
            {

                var his = TransactionDAO.GetTopupHistory(pay.RefCode);
                if (his != null)
                {
                    List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                    var d = data.FirstOrDefault(x => x.Type == his.CardType && x.Prize == pay.Amount);
                    if (d == null)
                        d = new CardConfig();
                    long exchangeValue = pay.Amount * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    if (pay.Status != 1 && pay.Status > 0)
                        pay.Status *= -1;
                    TransactionDAO.UpdatePayResult(pay.RefCode, pay.Status, pay.Amount);
                    TransactionDAO.RetopupCard(pay.RefCode, pay.Amount, exchangeValue, pay.Status);
                }

                return "1|success";
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return "-1|failed";
        }
        [HttpOptions, HttpPost]
        public string callbackgg([FromBody]PayGooglesCallback pay)
        {
            NLogManager.LogMessage("CALLBACK PAY GG: " + JsonConvert.SerializeObject(pay));
            try
            {

                var his = TransactionDAO.GetTopupHistory(pay.transaction_id);
                if (his != null)
                {
                    List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                    var d = data.FirstOrDefault(x => x.Type == his.CardType && x.Prize == pay.amount);
                    if (d == null)
                        d = new CardConfig();
                    long exchangeValue = pay.amount * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    if (pay.error_code != 1 && pay.error_code > 0)
                        pay.error_code *= -1;
                    TransactionDAO.UpdatePayResult(pay.transaction_id, pay.error_code, pay.amount);
                    TransactionDAO.RetopupCard(pay.transaction_id, pay.amount, exchangeValue, pay.error_code);
                }

                return "1|success";
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return "-1|failed";
        }
        [HttpOptions, HttpGet]
        public bool callbackTopupKhan(string id, int status, int amount)
        {
            NLogManager.LogMessage("PayKHAN CALLBACK => " + $"{id}|{status}|{amount}");
            try
            {

                var his = TransactionDAO.GetTopupHistory(id);
                if (his != null)
                {
                    List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                    var d = data.FirstOrDefault(x => x.Type == his.CardType && x.Prize == amount);
                    if (d == null)
                        d = new CardConfig();
                    long exchangeValue = amount * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    if (status > 0)
                        status *= -1;
                    if (status == 0)
                        status = 1;
                    TransactionDAO.UpdatePayResult(id, status, amount);
                    TransactionDAO.RetopupCard(id, amount, exchangeValue, status);
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }
        [HttpOptions, HttpGet]
        public int callbackth(int status, string message, int amount, int port, string type, string code, string serial, string tranid, string ext, string receive, string response, string updated)
        {
            try
            {
                NLogManager.LogMessage($"CallBack TIMO: {status}|{message}|{amount}|{port}|{type}|{code}|{serial}|{tranid}|{ext}|{receive}|{response}|{updated}");
                var his = TransactionDAO.GetTopupHistory(tranid);
                if (his != null)
                {
                    List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                    var d = data.FirstOrDefault(x => x.Type == his.CardType && x.Prize == amount);
                    if (d == null)
                        d = new CardConfig();
                    long exchangeValue = amount * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    if (status > 0 && status != 1)
                        status *= -1;
                    if (status == 0)
                        status = -1;
                    TransactionDAO.UpdatePayResult(tranid, status, amount);
                    TransactionDAO.RetopupCard(tranid, amount, exchangeValue, status);
                }

                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        public static string formatMoney(long money)
        {
            return String.Format("{0:#,###}", money).Replace(",", ".");
        }

        private string dateTt()
        {

            // string formatDATE = DateTime.Now.Month + "/" + DateTime.Now.Day + "/" + DateTime.Now.Year + "/" + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Millisecond;
            string formatDATE = DateTime.Now.ToString("ddMMyyHHmmss");
            //NLogManager.Instance.LogMessage("{0}", formatDATE);
            return formatDATE;
        }

        [HttpGet, HttpOptions, Authorize]
        public CashoutModel Cashout(int cardType, int prize)
        {
            try
            {
                List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                var d = data.FirstOrDefault(x => x.Type == cardType && x.Prize == prize);
                if (d == null)
                    return new CashoutModel { Status = -100 };

                return TransactionDAO.Cashout(AccountSession.AccountID, AccountSession.AccountName, cardType, prize, prize * (d.CashoutRate - d.Promotion) / 100);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new CashoutModel
            {
                Status = -99
            };
        }

        [HttpGet, HttpOptions]
        public int topupPostBackGA(int card_id, string phoneNum, string result, int menhGiaThe, int menhGiaDK, int menhGiaThuc, string status, string requestId)
        {
            try
            {
                int stt = -1;
                if(status == "success")
                {
                    stt = 1;
                }
                var his = TransactionDAO.GetTopupHistory(requestId);
                if (his != null)
                {
                    List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                    var d = data.FirstOrDefault(x => x.Type == his.CardType && x.Prize == menhGiaThuc);
                    if (d == null)
                        d = new CardConfig();
                    long exchangeValue = menhGiaThuc * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    //TransactionDAO.UpdatePayResult(id, status, amount);
                    TransactionDAO.RetopupCard(requestId, menhGiaThuc, exchangeValue, stt);
                }

                return 1;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return -99;
        }

        [HttpGet, HttpOptions]
        public List<CardCheck> TopupInfo()
        {
            List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();

            return GetTopupInfo(data);
        }

#if UWIN
        [HttpGet, HttpOptions, Authorize]
        public async Task<TopupResponse> TopupCard(string serial, string pin, int cardType, int prize, string captcha, string token)
        {
            try
            {
                int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, token);
                if (captchaVeriryStatus < 0) return new TopupResponse { ErrorCode = captchaVeriryStatus, Message = "Mã xác nhận không chính xác hoặc đã hết hạn" };
                List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
                var d = data.FirstOrDefault(x => x.Type == cardType && x.Prize == prize);
                if (d == null)
                    return new TopupResponse { ErrorCode = -100, Message = "Loại thẻ này không hợp lệ" };
                var dataPay = GetTopupInfo(data);
                if (!dataPay.Exists(x => x.Type == cardType && x.Enable && x.Prizes.Exists(y => y.Prize == prize)))
                    return new TopupResponse { ErrorCode = captchaVeriryStatus, Message = "Thẻ mệnh giá này hiện tại không sử dụng được." };
                var idx = data.FirstOrDefault(x => x.Type == cardType && x.Enable && x.Prize == prize);
                int payIndex = PayIndexCounter.PayIndex(idx.PayOrderConfig, prize, idx.Type);
                long randomFactor = DateTime.Now.Ticks + RandomUtil.NextInt(1000) + AccountSession.AccountID;
                string transactionId = Security.MD5Encrypt(pin + "_" + serial + "_" + cardType + "_" + randomFactor);
                PaymentTopupResponse response = null;
                switch (payIndex)
                {
                    //case 1:
                    //    int payVzCardType = 1; //the viettel
                    //    if (cardType == 4)
                    //        payVzCardType = 2; //the zing
                    //    response = PayVZ.Topup(serial, pin, payVzCardType, prize, string.Empty);
                    //    break;
                    //case 2:
                    //    var paybb2dCardType = string.Empty;
                    //    if (cardType == 1)
                    //        paybb2dCardType = "viettel";
                    //    else if (cardType == 2)
                    //        paybb2dCardType = "vms";
                    //    else if (cardType == 3)
                    //        paybb2dCardType = "vnp";
                    //    response = PayBB2DUwin.Topup(serial, pin, paybb2dCardType, prize, AccountSession.AccountName, transactionId);
                    //    break;
                    //case 3:
                    //    var paytimoCardType = string.Empty;
                    //    if (cardType == 1)
                    //        paytimoCardType = "VTT";
                    //    else if (cardType == 2)
                    //        paytimoCardType = "VMS";
                    //    else if (cardType == 3)
                    //        paytimoCardType = "VNP";
                    //    transactionId = transactionId.Substring(0, 20);
                    //    response = PayTichHop.Topup(serial, pin, paytimoCardType, prize, AccountSession.AccountName, IPAddressHelper.GetClientIP(), transactionId);
                    //    break;
                    //case 4:
                    //    var payKhanCardType = string.Empty;
                    //    if (cardType == 1)
                    //        payKhanCardType = "1";
                    //    else if (cardType == 2)
                    //        payKhanCardType = "3";
                    //    else if (cardType == 3)
                    //        payKhanCardType = "2";
                    //    transactionId = (Thread.CurrentThread.ManagedThreadId + long.Parse(dateTt().Trim()) + RandomUtil.NextInt(1000)).ToString();
                    //    response = await PayKHAN.Topup(serial, pin, payKhanCardType, prize, AccountSession.AccountName, transactionId);
                    //    break;
                    //case 5:
                    //    int payMbCardType = cardType;
                    //    if (cardType == 4)
                    //        payMbCardType = 2;
                    //    else if (cardType == 2)
                    //        payMbCardType = 221;
                    //    response = PayMB.Topup(serial, pin, payMbCardType, prize, AccountSession.AccountName, transactionId);
                    //    break;
                    default:
                        string cardTypeGA = string.Empty;
                        if (cardType == 1)
                            cardTypeGA = "vt";
                        else if (cardType == 2)
                            cardTypeGA = "mb";
                        else if (cardType == 3)
                            cardTypeGA = "vn";
                        response = PayGA.Topup(serial, pin, cardTypeGA, prize, string.Empty, transactionId);
                        break;
                }


                if (response.ErrorCode == 1)
                {
                    prize = response.Amount;
                    long exchangeValue = prize * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    var topUpResponse = TransactionDAO.TopupCard(transactionId, AccountSession.AccountID, AccountSession.AccountName,
                        prize, exchangeValue, 1, pin, serial, AccountSession.DeviceID, "Nạp thẻ", cardType);
                    if (topUpResponse >= 0)
                    {
                        return new TopupResponse
                        {
                            Balance = topUpResponse,
                            ErrorCode = 1,
                            ExchangeValue = exchangeValue
                        };
                    }
                    else
                    {
                        NLogManager.LogMessage("Topup Error: " + AccountSession.AccountID + "|" + serial + "|" + pin + "|" + prize + "|" + topUpResponse);
                    }
                }

                if (response.ErrorCode == -20000000)
                {
                    long exchangeValue = prize * d.TopupRate / 100;
                    exchangeValue += (exchangeValue * d.Promotion / 100);
                    var topUpResponse = TransactionDAO.TopupCard(transactionId, AccountSession.AccountID, AccountSession.AccountName,
                        prize, exchangeValue, 0, pin, serial, AccountSession.DeviceID, "Nạp thẻ", cardType);

                    return new TopupResponse
                    {
                        ErrorCode = response.ErrorCode,
                        Message = response.Message
                    };
                }

                return new TopupResponse
                {
                    ErrorCode = response.ErrorCode,
                    Message = response.Message
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new TopupResponse
            {
                ErrorCode = -99,
                Message = "Hệ thống của chúng tôi đang bận, xin bạn vui lòng thử lại sau",
            };
        }
#endif

        [HttpGet, HttpOptions]
        public List<CardCheck> CashoutInfo()
        {
            List<CardConfig> data = TransactionDAO.GetCardConfigs().ToList();
            return GetCashoutInfo(data);
        }
        private List<CardCheck> GetTopupInfo(List<CardConfig> data)
        {
            try
            {
                bool isrong = false;
#if UWIN
                isrong = true;
#endif
                List<CardCheck> res = new List<CardCheck>();
                var data1 = data.GroupBy(x => x.Type);
                foreach (var i in data1)
                {
                    res.Add(new CardCheck
                    {
                        Type = i.FirstOrDefault().Type,
                        Enable = i.ToList().Exists(x =>
                        {
                            if (x.Enable && isrong)
                            {
                                if (!string.IsNullOrEmpty(x.PayOrderConfig))
                                {
                                    return x.PayOrderConfig.Split('|').ToList().Exists(y =>
                                    {
                                        int a = 0;
                                        int.TryParse(y, out a);
                                        return a > 0;
                                    });
                                }
                            }
                            return x.Enable;
                        }),
                        Prizes = i.Where(x =>
                        {
                            if (x.Enable && isrong)
                            {
                                if (!string.IsNullOrEmpty(x.PayOrderConfig))
                                {
                                    return x.PayOrderConfig.Split('|').ToList().Exists(y =>
                                    {
                                        int a = 0;
                                        int.TryParse(y, out a);
                                        return a > 0;
                                    });
                                }
                            }
                            return x.Enable;
                        }).Select(x => new CfgCard
                        {
                            Prize = x.Prize,
                            Promotion = x.Promotion,
                            Rate = x.TopupRate
                        }).ToList()
                    });
                }
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        private List<CardCheck> GetCashoutInfo(List<CardConfig> data)
        {
            try
            {
                List<CardCheck> res = new List<CardCheck>();
                var data1 = data.GroupBy(x => x.Type);
                foreach (var i in data1)
                {
                    res.Add(new CardCheck
                    {
                        Type = i.FirstOrDefault().Type,
                        Enable = i.ToList().Exists(x =>
                        {
                            return x.EnableCashout;
                        }),
                        Prizes = i.Where(x =>
                        {
                            return x.EnableCashout;
                        }).Select(x => new CfgCard
                        {
                            Prize = x.Prize,
                            Promotion = x.PromotionCashout,
                            Rate = x.CashoutRate
                        }).ToList()
                    });
                }
                return res;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [HttpGet, HttpOptions, Authorize]
        public IEnumerable<CashoutHistory> CashoutHistories()
        {
            try
            {
                NLogManager.LogMessage("GET CASHOUT HISTORY => " + AccountSession.AccountID);
                return TransactionDAO.GetCashoutHistories(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return
                null;
        }

        [HttpGet, HttpOptions, Authorize]
        public IEnumerable<TopupHistory> TopupHistories()
        {
            try
            {
                return TransactionDAO.GetTopupHistories(AccountSession.AccountID);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return
                null;
        }
    }


    public class TopupResponse
    {
        public int ErrorCode { get; set; }
        public string Message { get; set; }
        public long Balance { get; set; }
        public long ExchangeValue { get; set; }
    }

    public class PayResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public string Pin { get; set; }
        public string Serial { get; set; }
        public int Amount { get; set; }
    }

    public class PayIndexCounter
    {
        private static object _lock;
        private static ConcurrentDictionary<string, long> _counters;
        public static void Init()
        {
            _lock = new object();
            _counters = new ConcurrentDictionary<string, long>();
        }

        public static int PayIndex(string payConfigList, int amount, int cardType)
        {
            if (Monitor.TryEnter(_lock, 5000))
            {
                try
                {
                    if (!_counters.ContainsKey($"{cardType}_{amount}"))
                    {
                        _counters.TryAdd($"{cardType}_{amount}", 0);
                    }

                    List<int> inx = new List<int>();
                    payConfigList.Split('|').Select(
                        (x, i) =>
                        {
                            if (!string.IsNullOrEmpty(x))
                            {
                                int value = int.Parse(x);
                                for (int j = 0; j < value; j++)
                                {
                                    inx.Add(i + 1);
                                }
                            }
                            return x;
                        }).ToArray();

                    long value1 = _counters[$"{cardType}_{amount}"];
                    int retur = inx[(int)(value1 % inx.Count)];
                    value1 += 1;
                    _counters[$"{cardType}_{amount}"] = value1;
                    return retur;
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }

            return -99;
        }
    }


    public class paybb2dCallBack
    {
        public string RefCode { get; set; }
        public int Status { get; set; }
        public int Amount { get; set; }
        public string Singature { get; set; }
    }

    public class InputIap
    {
        public string packageName { get; set; }
        public string productId { get; set; }
        public long purchaseTime { get; set; }
        public int purchaseState { get; set; }
        public string purchaseToken { get; set; }
    }

    public class PayGooglesCallback
    {
        public string transaction_id { get; set; }
        public int error_code { get; set; }
        public string error_text { get; set; }
        public int amount { get; set; }
    }

}
