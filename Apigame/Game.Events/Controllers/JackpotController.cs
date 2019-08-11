using System.Collections.Generic;
using System.Web.Http;
using Game.Events.Database.DTO;
using Game.Events.Database.Factory;
using Utilities.Cache;

namespace Game.Events.Controllers
{
    public class JackpotController : ApiController
    {
        public List<Jackpot> GetAllJackpot()
        {
            List<Jackpot> jackpotList = (List<Jackpot>)CacheHandler.Get("Jackpot_JackpotList");
            if (jackpotList == null)
            {
                jackpotList = AbstractDAOFactory.Instance().CreateJackpotDAO().GetAllJackpot();
                if(jackpotList != null)
                    CacheHandler.Add("Jackpot_JackpotList", jackpotList, 5);
            }
            return jackpotList;
        }
    }
}
