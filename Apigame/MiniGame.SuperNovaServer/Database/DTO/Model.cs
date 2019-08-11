using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class Model
    {
        public string Username { get; set; }
        public long PrizeValue { get; set; }
        public long BetValue { get; set; }
        public DateTime CreatedTime { get; set; }
    }

    public class BigJackpotCount
    {
        public int JackpotCount { get; set; }
        public int InEvent { get; set; }
    }
}
