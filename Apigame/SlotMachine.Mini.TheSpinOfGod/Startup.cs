using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;

[assembly: OwinStartup(typeof(SlotMachine.Mini.TheSpinOfGod.Startup))]

namespace SlotMachine.Mini.TheSpinOfGod
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var hubConfiguration = new HubConfiguration
            {
                EnableJavaScriptProxies = false,
                EnableDetailedErrors = false,
                EnableJSONP = true
            };
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR(hubConfiguration);
        }
    }
}
