using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Game.Events.Database.DTO;
using Game.Events.Database.Factory;

namespace Game.Events.Controllers
{
    public class MiniSlot2Controller : ApiController
    {
        public BigJackpotInfo GetBigJackpotInfo()
        {
            return AbstractDAOFactory.Instance().CreateSuperNovaBigJackpotDAO().GetBigJackpotInfo();
        }

        public List<BigJackpotHistory> GetBigJackpotHistory()
        {
            return AbstractDAOFactory.Instance().CreateSuperNovaBigJackpotDAO().GetBigJackpotHistory();
        }
    }
}
