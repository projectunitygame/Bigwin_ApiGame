using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace GamePortal.API.Lib
{
    public class Constant
    {
        public static string apiToken = "fqI9VHKAGdifopUDloay";
        public static string url_fish = "http://167.179.80.156:9820/be/services/uwin/user/";
        public static int gameID_ca = 200;
        public static int MA_GAME = int.Parse(ConfigurationManager.AppSettings["MA_GAME"]);
    }
}