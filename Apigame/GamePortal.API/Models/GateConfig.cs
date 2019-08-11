using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace GamePortal.API.Models
{
    public class GateConfig
    {
        private static readonly string _cString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["GamePortal"].ConnectionString);

        public static string DbConfig => _cString;

    }
}