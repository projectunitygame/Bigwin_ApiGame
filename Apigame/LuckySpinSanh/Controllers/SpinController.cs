using LuckySpinSanh.Database;
using LuckySpinSanh.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace LuckySpinAT.Controllers
{
    public class SpinController : ApiController
    {
        /// <summary>
        /// Số lần quay còn lại trong ngày
        /// </summary>
        /// <returns></returns>
        [Authorize, HttpOptions, HttpGet]
        public dynamic GetAvailable()
        {
            try
            {
                long accountId = AccountSession.AccountID;
                int spinAvailable = GetAvailableSpin(accountId, true);
                if (spinAvailable < 0)
                    spinAvailable = 0;
                return new
                {
                    Code = 1,
                    SpinChance = spinAvailable
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return new
            {
                Code = -99
            };
        }

        [Authorize, HttpOptions, HttpGet]
        public async Task<dynamic> Spin(string captcha, string token)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                long totalTopup = SpinDAO.GetRecentTopupCard(accountId);
                var portalCaptcha = new LuckySpinSanh.PortalCaptcha.captchaSoapClient();
                bool checkCapt = portalCaptcha.ProxyCheckCaptcha(captcha, token);
                if (checkCapt == false)
                    return new
                    {
                        Code = -2
                    };
                portalCaptcha.Close();
                int spinAvailable = GetAvailableSpin(accountId, false);
                if (spinAvailable < 0)
                    spinAvailable = 0;

                if (spinAvailable < 1)
                    return new
                    {
                        Code = -11
                    };

                byte flow = 1;

                if (totalTopup < 100000)
                    flow = 1;
                else if (totalTopup >= 100000 && totalTopup < 200000)
                    flow = 2;
                else if (totalTopup >= 200000 && totalTopup < 500000)
                    flow = 3;
                else if (totalTopup >= 500000)
                    flow = 4;

                var smallResult = SpinSmall(flow);
                var bigResult = SpinBig(flow);

                var isLog = SpinDAO.LogSession(accountId, smallResult, bigResult, out long sessionId);
                if (!isLog)
                    return new
                    {
                        Code = -99
                    };
                long gold = -1;
                if (smallResult > 0 || bigResult > 0)
                {
                    var isSuccessAward = Award(accountId, smallResult, bigResult, sessionId, out gold);
                    if (isSuccessAward)
                        return new
                        {
                            Code = 1,
                            StarResult = bigResult,
                            CoinResult = smallResult,
                            SpinChance = spinAvailable - 1,
                            Gold = gold
                        };
                }

                return new
                {
                    Code = 1,
                    StarResult = bigResult,
                    CoinResult = smallResult,
                    SpinChance = spinAvailable - 1,
                    Gold = gold
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new
            {
                Code = -99
            };
        }

        [Authorize, HttpOptions, HttpGet]
        public dynamic History(int page = 1, int itemPerPage = 10)
        {
            try
            {
                long accountId = AccountSession.AccountID;
                var history = SpinDAO.GetHistory(accountId, page, itemPerPage);

                return new
                {
                    Code = 1,
                    History = history
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new
            {
                Code = -99
            };
        }

        private bool Award(long accountId, int smallResult, int bigResult, long sessionId, out long gold)
        {
            var status = SpinDAO.Award(accountId, smallResult, bigResult, sessionId, out gold);
            return status;
        }

        private int GetAvailableSpin(long accountId, bool getOnly = true)
        {
            var curDtInt = ParseDateTimeToInt(DateTime.Now);
            var spinChance = SpinDAO.GetSpinChancePerDay(curDtInt);
            var spun = SpinDAO.GetSpin(accountId, curDtInt, spinChance, getOnly);
            return spun;
        }

        private bool CheckCaptcha(string captcha, string token)
        {
            if (string.IsNullOrEmpty(captcha) || string.IsNullOrEmpty(token))
                return false;
            int captchaVeriryStatus = Utilities.Captcha.Verify(captcha, token);
            return captchaVeriryStatus > 0;
        }

        private int SpinSmall(byte flow)
        {
            int prizeCode = 0;
            int quantity = 0;

            var spinConfigs = new List<SmallSpinConfig>();
            spinConfigs = SpinDAO.GetSmallSpinConfig(flow);

            var rnd = GenerateRandomNumber(5);
            foreach (var item in spinConfigs)
            {
                if (rnd >= item.StartValue && rnd <= item.EndValue)
                {
                    prizeCode = item.Code;
                    quantity = item.Quantity;
                }
            }

            // nếu không trượt thì kiểm tra còn giải hay không, hết giải => trượt
            if (prizeCode != 0)
                prizeCode = IsPrizeRemaining(prizeCode, quantity, true) ? prizeCode : 0;

            return prizeCode;
        }

        private int SpinBig(byte flow)
        {
            int prizeCode = 0;
            int quantity = 0;
            long price = 0;

            var spinConfigs = new List<BigSpinConfig>();
            spinConfigs = SpinDAO.GetBigSpinConfig(flow);

            var rnd = GenerateRandomNumber(4);
            foreach (var item in spinConfigs)
            {
                if (rnd >= item.StartValue && rnd <= item.EndValue)
                {
                    prizeCode = item.Code;
                    quantity = item.Quantity;
                    price = item.Price;
                }
            }

            // nếu trúng thì kiểm tra còn giải hay không, hết giải => trượt
            if (prizeCode != 0)
                prizeCode = IsPrizeRemaining(prizeCode, quantity, true) ? prizeCode : 0;

            return prizeCode;
        }

        private bool IsPrizeRemaining(int prizeCode, int quantity, bool isCoinPrize = false)
        {
            int currentDate = ParseDateTimeToInt(DateTime.Now);
            var wonPrize = SpinDAO.PrizeCounterByDate(prizeCode, currentDate, isCoinPrize);
            return quantity > wonPrize;
        }

        /// <summary>
        /// convert current time to int 'yyyymmdd' format
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private int ParseDateTimeToInt(DateTime dt)
        {
            return dt.Year * 10000 + dt.Month * 100 + dt.Day;
        }

        private int GenerateRandomNumber(int maxSize)
        {
            char[] chars = new char[62];
            chars = "1234567890".ToCharArray();

            byte[] data = new byte[1];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(maxSize);

            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return Convert.ToInt32(result.ToString());
        }
    }
}