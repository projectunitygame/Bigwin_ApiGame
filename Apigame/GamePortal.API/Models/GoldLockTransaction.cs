using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class GoldLockTransaction
    {
        public long ID { get; set; }
        public DateTime CreatedTime { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
    }
}