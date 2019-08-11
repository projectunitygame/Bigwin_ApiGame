using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minigames.DataAccess.DTO
{
    public class JackpotData
    {
        public DateTime LastUpdated { get; set; }
        public long Jackpot { get; set; }
        public byte BetType { get; set; }
        public byte RoomID { get; set; }
        public bool IsChanged { get; set; }
    }
}
