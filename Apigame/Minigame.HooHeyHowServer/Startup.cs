using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Minigame.HooHeyHowServer.Models;
using Owin;

[assembly: OwinStartup(typeof(Minigame.HooHeyHowServer.Startup))]

namespace Minigame.HooHeyHowServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            GameSession.Init();
        }
    }
}
