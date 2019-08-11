using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.DTO
{
    public class LuckydiceRank
    {
        public int ID { get; set; }
        public string AccountName { get; set; }
        public int Total { get; set; }
        public long AccountID { get; set; }
    }
}