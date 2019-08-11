using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SlotGame._20Lines.Game1.Startup))]

namespace SlotGame._20Lines.Game1
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.MapSignalR(new HubConfiguration(){EnableJSONP = true, EnableDetailedErrors = false, EnableJavaScriptProxies = true});
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
