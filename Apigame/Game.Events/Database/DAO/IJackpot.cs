using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.Events.Database.DTO;
namespace Game.Events.Database.DAO
{
    public interface IJackpot
    {
        List<Jackpot> GetAllJackpot();
    }
}