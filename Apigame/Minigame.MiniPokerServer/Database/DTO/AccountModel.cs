using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigame.MiniPokerServer.Database.DTO
{
    public class AccountModel
    {
        public long AccountID
        {
            get;
            set;
        }

        public DateTime LastPlay
        {
            get;
            set;
        }
        public int count { get; set; }
        public AccountModel(long accid)
        {
            this.AccountID = accid;
            this.LastPlay = DateTime.Now;
            count = 0;
        }
        public AccountModel Copy()
        {
            AccountModel b = new AccountModel(this.AccountID);
            b.LastPlay = this.LastPlay;
            b.count = this.count;
            return b;
        }
    }
}
