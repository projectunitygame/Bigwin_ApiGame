using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities.Log;

namespace OTP
{
    public class OTP
    {
        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static ConcurrentDictionary<long, OTPError> _cache = new ConcurrentDictionary<long, OTPError>();

        public static string GenerateOTP(long accountId, string phonenumber = "")
        {
            OTPError err = null;

            if (!string.IsNullOrEmpty(phonenumber))
            {
                if (_cache.TryGetValue(accountId, out err))
                {
                    if (!err.CanGet)
                    {
                        if (DateTime.Now.Subtract(err.LastTime).TotalMinutes <= 2)
                            return "-70";
                        err.LastTime = DateTime.Now;
                        err.CanGet = true;
                    }
                    else
                    {
                        err.CanGet = true;
                        err.LastTime = DateTime.Now;
                    }
                }
                if (err == null)
                {
                    err = new OTPError
                    {
                        CanGet = true,
                        LastTime = DateTime.Now
                    };
                    _cache.AddOrUpdate(accountId, err, (k, v) => err);
                }
            }

            if (string.IsNullOrEmpty(phonenumber))
                phonenumber = string.Empty;
            int responseStatus = -99;
            var ct = OtpDAO.GenerateCounter(accountId, out responseStatus);
            NLogManager.LogMessage(accountId + "|" + phonenumber + "|" + ct.C + "|" + (ct.C + GetCurrentCounter()));
            if (!string.IsNullOrEmpty(phonenumber))
            {
                err.CanGet = false;
                return GetTimeOTP(ct.T + "_" + phonenumber, ct.C + GetCurrentCounter());
            }
            else
            {
                return GetTimeOTP(ct.AppT + "_" + phonenumber, ct.C + GetCurrentCounter());
            }
        }

        public static bool ValidateOTP(long accountId, string otp, string phonenumber = "")
        {
            if (string.IsNullOrEmpty(phonenumber))
                phonenumber = string.Empty;
            var ct = OtpDAO.GetCurrentCounter(accountId);
            if (ct == null || string.IsNullOrEmpty(ct.T))
                return false;
            bool valid = false;
            NLogManager.LogMessage(GetTimeOTP(ct.T + "_" + phonenumber, ct.C + GetCurrentCounter()) + "|" + otp + "|" + phonenumber + "|" + accountId + "|" + ct.C + "|" + (ct.C + GetCurrentCounter())) ;
            if (!string.IsNullOrEmpty(phonenumber))
            {
                valid = GetTimeOTP(ct.T + "_" + phonenumber, ct.C + GetCurrentCounter()) == otp;
                if (!valid)
                    valid = GetTimeOTP(ct.T + "_" + phonenumber, ct.C - 1 + GetCurrentCounter()) == otp;
            }
            else {
                valid = GetTimeOTP(ct.AppT + "_" + phonenumber, ct.C + GetCurrentCounter()) == otp;
                if (!valid)
                {
                    valid = GetTimeOTP(ct.AppT + "_" + phonenumber, ct.C - 1 + GetCurrentCounter()) == otp;
                }
            }
            NLogManager.LogMessage("ValidateOTP: " + valid);
            return valid;
        }

        public static string GetCurrentAccountToken(long accountId)
        {
            int responseStatus = -99; 
            var par = OtpDAO.GenerateCounter(accountId, out responseStatus);
            return par?.AppT;
        }

        public static void SetToken(long accountId, string deviceToken)
        {
            OtpDAO.SetToken(accountId, deviceToken);
        }

        public static void InsertToken(long accountId, string deviceToken)
        {

        }

        private static string GeneratePassword(string secret, long iterationNumber, int digits = 6)
        {
            byte[] counter = BitConverter.GetBytes(iterationNumber);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(counter);

            byte[] key = Encoding.ASCII.GetBytes(secret);

            HMACSHA1 hmac = new HMACSHA1(key, true);

            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24)
                | ((hash[offset + 1] & 0xff) << 16)
                | ((hash[offset + 2] & 0xff) << 8)
                | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, digits); // 6 digits

            return password.ToString(new string('0', digits));
        }

        private static string GetTimeOTP(string secret, long counter)
        {
            return GeneratePassword(secret, counter);
        }

        private static long GetCurrentCounter()
        {
            return GetCurrentCounter(DateTime.UtcNow, UNIX_EPOCH, 120);
        }

        private static long GetCurrentCounter(DateTime now, DateTime epoch, int timeStep)
        {
            return (long)TimeSpan.FromTicks(now.Ticks - epoch.Ticks).TotalSeconds / timeStep;
        }
    }

    public class OTPError
    {
        public bool CanGet { get; set; }
        public int Try { get; set; }
        public DateTime LastTime { get; set; }
    }
}
