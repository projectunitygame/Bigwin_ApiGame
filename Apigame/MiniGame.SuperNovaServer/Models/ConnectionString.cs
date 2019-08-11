
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace MiniGame.SuperNovaServer.Models
{
    public static class ConnectionString
    {
        static string connectionString = ConnectionStringUtil.Decrypt(System.Configuration.ConfigurationManager.ConnectionStrings["SuperNovaConnectionString"].ConnectionString);
        public static string GameConnectionString
        {
            get { return connectionString; }
        }
    }
}