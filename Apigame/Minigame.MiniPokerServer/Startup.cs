using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(Minigame.MiniPokerServer.Startup))]

namespace Minigame.MiniPokerServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds. 
            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

            GlobalHost.Configuration.DefaultMessageBufferSize = 200;

            var hubConfiguration = new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = false,
                EnableJavaScriptProxies = false
            };
            GlobalHost.HubPipeline.RequireAuthentication();
            //In production set nableDetailedErrors = false

            app.MapSignalR(hubConfiguration);
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
