using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    [Serializable]
    public class Account
    {
        public Account()
        {
        }
        public Account(Account account)
        {
            this.AccountID = account.AccountID;
            this.UserName = account.UserName;
            this.Coin = account.Coin;
            this.TotalStar = account.TotalStar;
            this.Star = account.Star;
            this.Vcoin = account.Vcoin;
            this.EventCoin = account.EventCoin;

            this.IsOtp = account.IsOtp;

            this.RemoteIP = account.RemoteIP;
            this.MerchantID = account.MerchantID;
            this.SourceID = account.SourceID;
            this.IsRedirect = account.IsRedirect;
            this.Token = account.Token;

        }

        public Account(long accountId, string username, long totalStar, long coin, long star, int vcoin, int eventCoin, int isOTP, int merchantId)
        {
            this.AccountID = accountId;
            this.UserName = username;
            this.Coin = coin;
            this.TotalStar = totalStar;
            this.Star = star;
            this.Vcoin = vcoin;
            this.EventCoin = eventCoin;
            this.IsOtp = isOTP;
            this.MerchantID = merchantId;
            this.SourceID = 0;
            //this.Experiences = experiences;
            //this.Avatar = avatar;
            //this.Nickname = nickname;
        }

        public long AccountID { get; set; }//account ID

        public string UserName { get; set; }//username

        public long TotalStar { get; set; }//gold trong tai khoan

        public long Coin { get; set; }//star trong tai khoan

        public long Star { get; set; }

        public int Vcoin { get; set; }

        public int EventCoin { get; set; }

        public int IsOtp { get; set; }
        public string OtpToken { get; set; }

        public string RemoteIP { get; set; }
        public int MerchantID { get; set; }
        public int SourceID { get; set; }
        public int IsRedirect { get; set; }
        public int IsViewPayment { get; set; }
        public int IsViewHotLine { get; set; }
        public string Token { get; set; }

        public bool IsEnoughMoney(long amount, byte moneyType)
        {
            if (amount < 0 || moneyType > 2)
            {
                return false;
            }
            else
            {
                if (moneyType == 0)
                {
                    return this.Coin >= amount;
                }
                else if (moneyType == 1)
                {
                    return this.TotalStar >= amount;
                }
                return false;
            }
        }

    }
}
