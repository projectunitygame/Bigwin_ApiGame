using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public class Game
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
    }
}