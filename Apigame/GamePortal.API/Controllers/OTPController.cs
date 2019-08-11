using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Encryption;
using Utilities.Log;

namespace GamePortal.API.Controllers
{
    public class OTPController : ApiController
    {
        [HttpOptions, HttpGet]
        public OTPResponse GetOTP(string token)
        {
            try
            {
                // Security.TripleDESEncrypt($"APP_deviceToken", deviceToken + "_" + accountId + "_" + RandomUtil.NextInt(100000));
                string data = Security.TripleDESDecrypt("APP_deviceToken", token);
                string[] spl = data.Split('_');
                var t = OTP.OTP.GetCurrentAccountToken(long.Parse(spl[1]));
                if (t != spl[0])
                    return new OTPResponse
                    {
                        code = -72
                    };
                return new OTPResponse
                {
                    code = 1,
                    otp = OTP.OTP.GenerateOTP(long.Parse(spl[1]))
                };
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return new OTPResponse
            {
                code = -99
            };
        }
    }

    public class OTPResponse
    {
        public int code { get; set; }
        public string otp { get; set; }
    }
}
