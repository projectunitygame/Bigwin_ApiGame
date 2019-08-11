using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat;

[assembly: OwinStartup(typeof(PTCN.CrossPlatform.Minigame.LuckyDice.Startup))]

namespace PTCN.CrossPlatform.Minigame.LuckyDice
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            var hubConfiguration = new HubConfiguration
            {
                // You can enable JSONP by uncommenting line below.
                // JSONP requests are insecure but some older browsers (and some
                // versions of IE) require JSONP to work cross domain
                 EnableJSONP = true
            };

            app.MapSignalR(hubConfiguration);
            ChatFilter.Init();
            GameManager.Init();
        }
    }
}
