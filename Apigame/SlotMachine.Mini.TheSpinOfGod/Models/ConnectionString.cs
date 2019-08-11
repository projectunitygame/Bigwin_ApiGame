using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace SlotMachine.Mini.TheSpinOfGod.Models
{
    public class ConnectionString
    {
        private static string _con = ConnectionStringUtil.Decrypt(ConfigurationManager
            .ConnectionStrings["SlotMachineConnectionString"].ConnectionString);

        public static string GameConnectionString => _con;
    }
}