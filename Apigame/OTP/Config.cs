using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Util;

namespace OTP
{
    internal class Config
    {
        private static string _cString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["GamePortal"].ConnectionString);
        public static string DbConfig => _cString;

    }
}
