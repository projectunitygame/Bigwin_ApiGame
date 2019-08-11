using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class CashoutHistory
    {
        public long ID { get; set; }
        public string CardCode { get; set; }
        public string CardSerial { get; set; }
        public int CardType { get; set; }
        public long Amount { get; set; }
        public int Status { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime ? VerifyTime { get; set; }
    }
}