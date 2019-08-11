using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilities.Util;

namespace Game.Events.Models
{
    public static class ConnectionString
    {
        private static string _gamePortalConnectionString = ConfigurationManager.ConnectionStrings["GamePortal"].ToString();
        private static string _slotMachineReportConnectionString = ConfigurationManager.ConnectionStrings["SlotMachineReportConnectionString"].ToString();
        private static string _Game1ConnectionString = ConfigurationManager.ConnectionStrings["Game1ConnectionString"].ToString();
        private static string _Game2ConnectionString = ConfigurationManager.ConnectionStrings["Game2ConnectionString"].ToString();
        private static string _Game3ConnectionString = ConfigurationManager.ConnectionStrings["Game3ConnectionString"].ToString();
        private static string _MiniSlot1ConnectionString = ConfigurationManager.ConnectionStrings["MiniSlot1ConnectionString"].ToString();
        private static string _MiniSlot2ConnectionString = ConfigurationManager.ConnectionStrings["MiniSlot2ConnectionString"].ToString();
        private static string _MiniPokerConnectionString = ConfigurationManager.ConnectionStrings["MiniPokerConnectionString"].ToString();

        public static string GamePortalConnectionString => ConnectionStringUtil.Decrypt(_gamePortalConnectionString);

        public static string SlotMachineReportConnectionString => ConnectionStringUtil.Decrypt(_slotMachineReportConnectionString);

        public static string TamQuocConnectionString => ConnectionStringUtil.Decrypt(_Game1ConnectionString);

        public static string VoLamConnectionString => ConnectionStringUtil.Decrypt(_Game2ConnectionString);

        public static string Game25LinesConnectionString => ConnectionStringUtil.Decrypt(_Game3ConnectionString);

        public static string SlotGodConnectionString => ConnectionStringUtil.Decrypt(_MiniSlot1ConnectionString);

        public static string SuperNovaConnectionString => ConnectionStringUtil.Decrypt(_MiniSlot2ConnectionString);

        public static string MiniPokerConnectionString => ConnectionStringUtil.Decrypt(_MiniPokerConnectionString);
    }
}