using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuckySpinSanh.Models
{
    public class SmallSpinConfig
    {
        public byte Id { get; set; }
        public long Price { get; set; }
        public byte Code { get; set; }
        public long StartValue { get; set; }
        public long EndValue { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
    }
}