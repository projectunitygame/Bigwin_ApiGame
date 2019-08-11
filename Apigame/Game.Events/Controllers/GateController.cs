using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data;
using Game.Events.Database.DTO;
using Game.Events.Database.Factory;
using Utilities;
using Utilities.Cache;
using Game.Events.Models;
using System.Web;
using Game.Events.Database.DAOImpl;

namespace Game.Events.Controllers
{
    public class GateController : ApiController
    {
        public List<Notification> GetNotification()
        {
            List<Notification> cached = (List<Notification>)CacheHandler.Get("Notification");
            if (cached != null)
            {
                return cached;
            }
            cached = AbstractDAOFactory.Instance().CreateGateDAO().GetNotification("");
            CacheHandler.Add("Notification", cached, 180);
            return cached;
        }

        public List<BigWinPlayers> GetBigWinPlayers()
        {
            List<BigWinPlayers> cached = (List<BigWinPlayers>)CacheHandler.Get("BigWinPlayersList");
            if (cached != null)
            {
                return cached;
            }
            //var test = AbstractDAOFactory.Instance().CreateGateDAO().GetNotification("");
            cached = AbstractDAOFactory.Instance().CreateGateDAO().GeBigWinPlayersByID(0, 15);
            List<BigWinPlayers> list2 = AbstractDAOFactory.Instance().CreateGateDAO().GeBigWinPlayersByID(5, 3);
            cached.AddRange(list2);
            RandomUtil.Shuffle<BigWinPlayers>(cached);
            CacheHandler.Add("BigWinPlayersList", cached, 30);
            return cached;
        }

        public ImageEvent GetImageEvent()
        {
            var links = new List<string>
            {
                "http://alphaserver.rong88.club/assets/image_event/event1.png",
                "http://alphaserver.rong88.club/assets/image_event/event2.png",
                "http://alphaserver.rong88.club/assets/image_event/event3.png"
            };

            return new ImageEvent
            {
                Version = "1.0",
                Links = links
            };


        }
    }
}
