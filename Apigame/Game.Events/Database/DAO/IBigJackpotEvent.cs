using Game.Events.Database.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Events.Database.DAO
{
    public interface IBigJackpotEvent
    {
        BigJackpotInfo GetBigJackpotInfo();
        List<BigJackpotHistory> GetBigJackpotHistory();
    }
}
