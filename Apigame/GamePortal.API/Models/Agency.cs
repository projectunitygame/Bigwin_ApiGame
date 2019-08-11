using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class Agency
    {
        public int ID { get; set; }
        public string Displayname { get; set; }
        public string GameName { get; set; }
        public string Tel { get; set; }
        public string Fb { get; set; }
        public string Telegram { get; set; }
        public string Information { get; set; }
    }
}