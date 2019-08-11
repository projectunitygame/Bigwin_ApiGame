using Game.Events.Database.DTO;
using Game.Events.Database.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Game.Events.Controllers
{
    public class MiniSlot1Controller : ApiController
    {
        public BigJackpotInfo GetBigJackpotInfo()
        {
            return AbstractDAOFactory.Instance().CreateSlotGodBigJackpotDAO().GetBigJackpotInfo();
        }

        public List<BigJackpotHistory> GetBigJackpotHistory()
        {
            return AbstractDAOFactory.Instance().CreateSlotGodBigJackpotDAO().GetBigJackpotHistory();
        }
    }
}
