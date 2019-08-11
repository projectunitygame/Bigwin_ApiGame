using Game.Events.Database.DTO;
using Game.Events.Database.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Cache;

namespace Game.Events.Controllers
{
    public class MiniPokerController : ApiController
    {
        public BigJackpotInfo GetBigJackpotInfo()
        {
            var bigJackpotInfo = (BigJackpotInfo)CacheHandler.Get("minipoker_BigJackpotInfo");
            if (bigJackpotInfo == null)
            {
                bigJackpotInfo = AbstractDAOFactory.Instance().CreateMiniPokerBigJackpotDAO().GetBigJackpotInfo();
                CacheHandler.Add("minipoker_BigJackpotInfo", bigJackpotInfo, 10);
            }
            return bigJackpotInfo;
        }

        public List<BigJackpotHistory> GetBigJackpotHistory()
        {
            List<BigJackpotHistory> bigJackpotHis = (List<BigJackpotHistory>)CacheHandler.Get("minipoker_BigJackpotHis");
            if(bigJackpotHis == null)
            {
                bigJackpotHis = AbstractDAOFactory.Instance().CreateMiniPokerBigJackpotDAO().GetBigJackpotHistory();
                CacheHandler.Add("minipoker_BigJackpotHis", bigJackpotHis, 30);
            }
            return bigJackpotHis;
        }
    }
}
