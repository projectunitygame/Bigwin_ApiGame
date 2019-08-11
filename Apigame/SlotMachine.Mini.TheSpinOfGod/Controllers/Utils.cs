using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace MinigameVuabai.SignalR.Controllers
{
    public class Utils
    {
        public static string GetIp()
        {
            var ip = string.Empty;

            try
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_CITRIX"] != null)
                {
                    ip = HttpContext.Current.Request.ServerVariables["HTTP_CITRIX"];
                }

                if (string.IsNullOrEmpty(ip) && HttpContext.Current.Request.Headers["CITRIX_CLIENT_HEADER"] != null)
                {
                    ip = HttpContext.Current.Request.Headers["CITRIX_CLIENT_HEADER"];
                }

                if (string.IsNullOrEmpty(ip) && HttpContext.Current.Request.Headers["X-Forwarded-For"] != null)
                {
                    return HttpContext.Current.Request.Headers["X-Forwarded-For"];
                }

                if (string.IsNullOrEmpty(ip))
                {
                    if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                    {
                        ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    }
                    if (ip == "")
                    {
                        ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

            return ip.Replace('|', '#').Trim();
        }
    }
    public class CookieManager
    {
        static string SSO_SESSION_COOKIES = "uwin.game.session";
        static string COOKIES_SSO = "uwin.game.info";
        static string COOKIE_DOMAIN = ".localhost";
        static string REMEMBERME_TIME_OUT = ConfigurationSettings.AppSettings["REMEMBERME_TIME_OUT"] ?? "30";

        static string CookieDomain
        {
            get
            {
                return ConfigurationSettings.AppSettings[COOKIE_DOMAIN] == null ? string.Empty : ConfigurationSettings.AppSettings[COOKIE_DOMAIN];
            }
        }



        public static void RemoveAllCookies(bool removeSesstionCookie)
        {
            HttpContext.Current.Response.Cookies.Clear();

            //foreach (string cookie in HttpContext.Current.Request.Cookies.AllKeys)
            //{
            //    HttpContext.Current.Response.Cookies.Set(new HttpCookie(cookie) { Expires = DateTime.Now.AddMonths(-1), Path = "/" });
            //}

            //HttpContext.Current.Response.Cookies.Set(new HttpCookie(cookie) { Expires = DateTime.Now.AddMonths(-1), Path = "/" });
            string[] cookies = HttpContext.Current.Request.Cookies.AllKeys;

            if (cookies == null || cookies.Length == 0)
                return;

            foreach (string cookie in cookies)
            {
                //Thêm cookie đã expire nếu có config Cookie Domain trong FormsAuthentication của web.config
                if (!string.IsNullOrEmpty(FormsAuthentication.CookieDomain) && cookie.Equals(FormsAuthentication.FormsCookieName))
                {
                    HttpContext.Current.Response.Cookies.Add(new HttpCookie(cookie)
                    {
                        Name = FormsAuthentication.FormsCookieName,
                        Domain = FormsAuthentication.CookieDomain,
                        Expires = DateTime.Now.AddYears(-1)
                    });
                }

                //if (cookie == FormsAuthentication.FormsCookieName) continue;
                //if (cookie == SSO_SESSION_COOKIES && !removeSesstionCookie) continue;
                //if (!cookie.ToLower().Contains("sso")) continue;
                try
                {
                    //lấy cookie có sẵn
                    HttpCookie httpCookie = HttpContext.Current.Request.Cookies[cookie];
                    if (httpCookie != null)
                    {
                        //set expires cho cookie
                        httpCookie.Domain = HttpContext.Current.Request.Url.Host.Contains("localhost") ? null : "." + HttpContext.Current.Request.Url.Host; ;
                        httpCookie.Expires = DateTime.Now.AddYears(-1);
                        //update cookie
                        HttpContext.Current.Response.Cookies.Set(httpCookie);
                    }

                    if (removeSesstionCookie)
                    {
                        HttpContext.Current.Request.Cookies.Remove(cookie);
                    }
                }
                catch (Exception ex)
                {
                    NLogLogger.PublishException(ex);
                }

            }
        }

    }

}