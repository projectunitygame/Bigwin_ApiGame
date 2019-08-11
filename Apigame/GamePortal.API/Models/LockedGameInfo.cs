using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class LockedGoldInfo
    {
        public string Username { get; set; }
        public long Gold { get; set; }
        public long LockedGold { get; set; }
    }
}