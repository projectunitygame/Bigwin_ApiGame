using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class TransferLog
    {
        public long ID { get; set; }
        public DateTime CreatedTime { get; set; }
        public string AccountName { get; set; }
        public long Amount { get; set; }
        public int Type { get; set; } // 1: chuyen, 2: nhan
    }
}