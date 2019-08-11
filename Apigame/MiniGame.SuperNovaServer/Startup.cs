using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Studio.WebGame.SupperNova.Controllers;

[assembly: OwinStartup(typeof(MiniGame.SuperNovaServer.Startup))]

namespace MiniGame.SuperNovaServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            // Wait a maximum of 30 seconds after a transport connection is lost
            // before raising the Disconnected event to terminate the SignalR connection.
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(90);

            // For transports other than long polling, send a keepalive packet every
            // 10 seconds.
            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(30);
            //app.UseWebApi(WebApiConfig.Register);

            var hubConfiguration = new HubConfiguration
            {
                EnableJavaScriptProxies = false,
                EnableDetailedErrors = false,
                EnableJSONP = true
            };
            app.MapSignalR(hubConfiguration);

            var a = GameHandler.Instance;
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
