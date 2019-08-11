using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities.Encryption;

namespace Utilities.ConfigHelper
{
    public sealed class Config
    {
        private static readonly Config instance = new Config();
        private readonly string _AdminAccountIds;

        private string _game1ConnectionString;
        private string _game2ConnectionString;
        private string _25linesConnectionString;

        private Config()
        {
            bool encrypted = true;
            bool production = true;

            string encryptedStr = ConfigurationManager.AppSettings["encrypted"];
            if (!string.IsNullOrEmpty(encryptedStr))
            {
                encrypted = bool.Parse(encryptedStr);
            }
            string productionStr = ConfigurationManager.AppSettings["production"];
            if (!string.IsNullOrEmpty(productionStr))
            {
                production = bool.Parse(productionStr);
            }
           
            _game1ConnectionString = GetConnStr("Game1ConnectionString");
            _game2ConnectionString = GetConnStr("Game2ConnectionString");
            _25linesConnectionString = GetConnStr("Game3ConnectionString");


            if (!encrypted)
            {
                if (production)
                {
                    Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                    AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");
                    ConnectionStringsSection connectStrings = (ConnectionStringsSection)config.GetSection("connectionStrings");

                    UpdateAppSettings(appSettings, "encrypted", "true");

                    UpdateConnectionStrings(connectStrings, "game1", true);

                    config.Save();
                    ConfigurationManager.RefreshSection("connectionStrings");
                }
            }
        }


        public static string Game1ConnectionString => instance._game1ConnectionString;
        public static string Game2ConnectionString => instance._game2ConnectionString;

        public static string Game25LinesConnectionString => instance._25linesConnectionString;

        public static string GetConnStr(string name)
        {
            return GetConnStr(name, true);
        }

        public static string GetConnStr(string name, bool encrypted)
        {
            string connStr = ConfigurationManager.ConnectionStrings[name] == null ? "" : ConfigurationManager.ConnectionStrings[name].ConnectionString;

            if (!encrypted)
            {
                return connStr;
            }

            try
            {
                return connStr == "" ? "" : new RijndaelEnhanced("rongclub88Key", "@1B2c3D4e5F6g7H8").Decrypt(connStr);
            }
            catch
            {
                return connStr;
            }
        }

        public static void UpdateAppSettings(AppSettingsSection appSettings, string key, string value)
        {
            if (appSettings.Settings[key] == null)
            {
                return;
            }

            appSettings.Settings[key].Value = value;
        }

        public static void UpdateConnectionStrings(ConnectionStringsSection connectStrings, string name, bool encrypt)
        {
            if (connectStrings.ConnectionStrings[name] == null)
            {
                return;
            }

            string connectionString = connectStrings.ConnectionStrings[name].ConnectionString;
            if (encrypt)
            {
                connectionString = new RijndaelEnhanced("pay", "@1B2c3D4e5F6g7H8").Encrypt(connectionString);
            }

            connectStrings.ConnectionStrings[name].ConnectionString = connectionString;
        }

        public static string GetAppSettings(string key, string defaultValue = "")
        {
            string value = defaultValue;

            if (string.IsNullOrEmpty(key))
                return value;

            try
            {
                value = ConfigurationManager.AppSettings[key];
            }
            catch { }

            return value;
        }

        public static int GetIntegerAppSettings(string key, int defaultValue = 0)
        {
            int value = defaultValue;

            if (string.IsNullOrEmpty(key))
                return value;

            try
            {
                value = Int32.Parse(ConfigurationManager.AppSettings[key]);
            }
            catch { }

            return value;
        }

        public static bool CheckEmail(string email)
        {
            //Kiểm tra định dạng email
            return Regex.IsMatch(email, @"^([0-9a-z]+[-._+&])*[0-9a-z]+@([-0-9a-z]+[.])+[a-z]{2,6}$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Get ra kiểu email (yahoo, gmail, msn) từ địa chỉ email
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns>
        /// 0: Loại khác
        /// 1: Yahoo
        /// 2: gmail
        /// 3: msn
        /// </returns>
        public static int GetEmailType(string emailAddress)
        {
            var listyahoo = new List<string> { "yahoo", "ymail.com", "rocketmail.com" };
            var listgmail = new List<string> { "gmail.com", "googlemail.com" };
            var listmsn = new List<string> { "hotmail.com", "msn.com", "live.com" };
            var domain = emailAddress.Substring(emailAddress.IndexOf('@') + 1);
            if (listyahoo.Exists(e => domain.ToLower().StartsWith(e)))
                return 1;
            if (listgmail.Exists(e => domain.ToLower().StartsWith(e)))
                return 2;
            return listmsn.Exists(e => domain.ToLower().StartsWith(e)) ? 3 : 0;
        }

        /// <summary>
        /// Kiểm tra độ mạnh của password
        /// pass phải từ 6-16 ký tự, bao gồm cả chữ cái và chữ số
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordStrong(string password)
        {
            return Regex.IsMatch(password, @"^(?=.{6,16})(?=.*\d)(?=.*[a-zA-Z]).*$");
        }
    }
}
