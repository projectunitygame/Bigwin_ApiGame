using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class TopupLog
    {
        public long ID { get; set; }
        public DateTime CreatedTime { get; set; }
        public long Amount { get; set; }
        public long Balance { get; set; }
        public int Type { get; set; }
        public string ReferName { get; set; }
    }
}