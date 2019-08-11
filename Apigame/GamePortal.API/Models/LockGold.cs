using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class LockGold
    {
        public int ResponseCode { get; set; }
        public long CurrentGold { get; set; }
    }
}