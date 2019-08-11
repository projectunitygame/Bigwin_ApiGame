using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Controllers
{
    public class BotController : ApiController
    {
        [HttpOptions, HttpPost]
        public async Task BotLuckyDice([FromBody] dynamic data)
        {

        }
    }
}