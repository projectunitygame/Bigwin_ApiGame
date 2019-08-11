using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace MiniGame.HiloServer.Models
{
    public class ConnectionString
    {
        static string connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["HiloConnectionString"].ConnectionString);
        public static string GameConnectionString
        {
            get
            {
                return connectionString;
            }
        }
    }
}