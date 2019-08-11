using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace GamePortal.API.Controllers.Captcha
{
    public class CaptchaController : ApiController
    {
        [HttpOptions, HttpGet]
        public Utilities.Captcha Gen()
        {
            bool isrong = ConfigurationManager.AppSettings["RONG88"] == "true";
            if (isrong)
            {
                return new Utilities.Captcha(Color.Black, Color.Black, Color.WhiteSmoke);
            }
            return new Utilities.Captcha(Color.Red, Color.RoyalBlue, Color.PaleGreen);
        }

        [HttpOptions, HttpGet]
        public bool ProxyCheckCaptcha(string captcha, string token)
        {
            return Utilities.Captcha.Verify(captcha, token) > 0;
        }
    }
}
