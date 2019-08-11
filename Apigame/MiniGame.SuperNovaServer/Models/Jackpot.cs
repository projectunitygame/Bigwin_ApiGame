using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Studio.WebGame.SupperNova.Models
{
    public class Jackpot
    {
        public DateTime CreatedTime { get; set; }
        public int RoomID { get; set; }
        private string _Username;
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value.Substring(0, value.Length - 1) + "*";
            }
        }
        public long TotalPrizeValue { get; set; }
    }
}