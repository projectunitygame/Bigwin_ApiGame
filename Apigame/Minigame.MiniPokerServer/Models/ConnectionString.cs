
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace Minigame.MiniPokerServer.Models
{
    public class ConnectionString
    {
        static string connectionString = ConnectionStringUtil.Decrypt(ConfigurationManager.ConnectionStrings["SlotMachineConnectionString"].ConnectionString, false);
        public static string GameConnectionString
        {
            get
            {
#if DEV
                return ConfigurationManager.ConnectionStrings["SlotMachineConnectionString"].ConnectionString;
#endif
                return (connectionString);
            }
        }
    }
}