using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace Utilities.Session
{
    public class AccountSession
    {
        public static long GetAccountID(dynamic context)
        {
            try
            {
                long userId = 0;
                if (context.Request.User != null && context.Request.User.Identity.IsAuthenticated)
                {
                    var s = context.Request.User.Identity.Name.Split('|');
                    if (s != null && s.Length > 0)
                        userId = Convert.ToInt64(s[0]);
                }
                return userId;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return 0;
            }
        }
        public static string GetAccountName(dynamic context)
        {
            try
            {
                string userName = string.Empty;

                if (context.Request != null && context.Request.User.Identity.IsAuthenticated &&
                    !string.IsNullOrEmpty(context.Request.User.Identity.Name))
                {
                    var s = context.Request.User.Identity.Name.Split('|');
                    if (s != null && s.Length > 1)
                        userName = s[1];
                }
                NLogManager.LogMessage("GetAccountName: " + userName);
                return userName;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                return "";
            }
        }


        public static long AccountID
        {
            get
            {
                //return 24;
                long userId = 0;

                if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var s = HttpContext.Current.User.Identity.Name.Split('|');
                    if (s.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(s[0]))
                        {
                            userId = Convert.ToInt64(s[0]);
                        }
                    }
                }
                //NLogManager.LogMessage("AccountID: " + userId);
                return userId;
            }
        }

        public static string AccountName
        {
            get
            {
                //return "rieng1goctroi";
                string userName = string.Empty;

                if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated && !string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name))
                {
                    var s = HttpContext.Current.User.Identity.Name.Split('|');
                    if (s != null && s.Length > 1)
                        userName = s[1];
                }
                //NLogManager.LogMessage("AccountID: " + userName);
                return userName;
            }
        }

        public static int DeviceID
        {
            get
            {
                int userId = 0;

                if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var s = HttpContext.Current.User.Identity.Name.Split('|');
                    if (s.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(s[2]))
                        {
                            userId = Convert.ToInt32(s[2]);
                        }
                    }
                }
                //NLogManager.LogMessage("AccountID: " + userId);
                return userId;
            }
        }

        public static int UserType
        {
            get
            {
                int userId = 0;

                if (HttpContext.Current != null && HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    var s = HttpContext.Current.User.Identity.Name.Split('|');
                    if (s.Length > 3)
                    {
                        if (!string.IsNullOrWhiteSpace(s[3]))
                        {
                            userId = Convert.ToInt32(s[3]);
                        }
                    }
                }

                return userId;
            }
        }
    }
}
