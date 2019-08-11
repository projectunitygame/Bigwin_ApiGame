using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class CashoutModel
    {
        public long Balance { get; set; }
        public long Status { get; set; }
        public MobileCard CashoutCard { get; set; }
    }

    public class MobileCard
    {
        public string CardCode { get; set; }
        public string CardSerial { get; set; }
        public int CardType { get; set; }
        public long Amount { get; set; }
        public int Status { get; set; }
    }

    public class ResultTransferToAgency
    {
        public long Balance { get; set; }
        public string TransID { get; set; }
        public long Amount { get; set; }
        public string RecipientID { get; set; }
        public string DateTrans { get; set; }
    }
}