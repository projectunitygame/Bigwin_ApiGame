using Minigame.MiniPokerServer.Database.DAO;
using Minigame.MiniPokerServer.Database.DTO;
using Minigame.MiniPokerServer.Database.Factory;
using MiniPoker.WebServer.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Utilities.Log;

namespace Minigame.MiniPokerServer.Controllers
{
    public class BotController : ApiController
    {
        [HttpOptions, HttpPost]
        public dynamic BotSpin([FromBody] dynamic _data)
        {
            try
            {
                List<dynamic> returnData = new List<dynamic>();
                foreach (var data in _data)
                {
                    long accountId = data.AccountID;
                    string username = data.AccountName;
                    int roomID = data.RoomID;
                    bool isBot = data.IsBot;
                    bool nohu = data.NoHu;

                    MiniPokerSpinResponse miniPokerSpinResponse = MiniPokerHandler.Instance.BotSpin(accountId, username, (int)1, (int)roomID, "127.0.0.1", 1, 1, 1, isBot, nohu);

                    returnData.Add(miniPokerSpinResponse);

                    Thread.Sleep(1000);
                }
                return returnData;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }
    }
}