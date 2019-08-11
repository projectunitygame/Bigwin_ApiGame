using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Payment
{
    public class PaymentTopupResponse
    {
        public int Amount { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}