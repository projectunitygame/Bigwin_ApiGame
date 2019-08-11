using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace Cardgame.DiskShaking.Database
{
    public class ConfigDB
    {
        public static readonly string PortalCons = ConfigurationManager.ConnectionStrings["Portal"]?.ConnectionString;
        public static readonly string GameCons = ConfigurationManager.ConnectionStrings["CardGame"]?.ConnectionString;
    }
}