using GamePortal.API.DataAccess;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Utilities.Encryption;

namespace GamePortal.API.Models
{
    public class UserInfo
    {
        public string userid { get; set; }
        public string username { get; set; }
    }
    public class Account
    {
        public string tokenAuthen { get; set; }//update 06-08-2019 - TD
        public long AccountID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int AvatarID { get; set; }
        public long Gold { get; set; }
        public long Coin { get; set; }
        public bool IsUpdateAccountName
        {
            get;
            set;
        }
        public string Tel { get; set; }
        public bool IsOTP { get; set; }
        public DateTime CreatedTime { get; set; }
        [JsonIgnore]
        public bool IsBlocked { get; set; }
        [JsonIgnore]
        public bool IsAgency { get; set; }
        public Account()
        {

        }

        public int UserType { get; set; }

        public Account(long accountId)
        {
            AccountID = accountId;
        }

        public int UpdateDisplayName(string displayname)
        {
            Regex rUserName = new Regex("^[a-zA-Z0-9_.-]{6,20}$");

            if (!rUserName.IsMatch(displayname))
                return 0;

            int res = AccountDAO.UpdateDisplayName(AccountID, displayname);

            if (res > 0)
                DisplayName = displayname;

            IsUpdateAccountName = res > 0;

            return res;
        }

        public int RegisterNormal(string userName, string password)
        {
            Regex rUserName = new Regex("^[a-zA-Z0-9_.-]{6,20}$");
            Regex rPassword = new Regex("^[a-zA-Z0-9_.-]{6,18}$");

            if (!rUserName.IsMatch(userName))
                return -20;

            if (!rPassword.IsMatch(password))
                return -30;

            AccountID = AccountDAO.CreateNormalAccount(userName, Security.MD5Encrypt(password), AvatarID); 

            if (AccountID < 0)
                return (int)AccountID;
            else
            {
                bool isrong = ConfigurationManager.AppSettings["RONG88"] == "true";
                if (isrong)
                    DisplayName = "U." + AccountID;
                else DisplayName = "Player_" + AccountID;
                Username = userName;
                Gold = 0;
                return 1;
            }
        }

        public int RegisterFacebookAccount(string username)
        {
            AccountID = AccountDAO.CreateFacebookAccount(username, AvatarID);

            if (AccountID < 0)
                return (int)AccountID;
            else
            {
                DisplayName = "U." + AccountID;
                Gold = 0;
                Username = username;
                return 1;
            }
        }
    }
}