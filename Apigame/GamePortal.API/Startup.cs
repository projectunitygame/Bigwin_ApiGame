using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(GamePortal.API.Startup))]

namespace GamePortal.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            var hubConfiguration = new HubConfiguration
            {
                // You can enable JSONP by uncommenting line below.
                // JSONP requests are insecure but some older browsers (and some
                // versions of IE) require JSONP to work cross domain
                EnableJSONP = true
            };

            app.MapSignalR(hubConfiguration);
        }
    }
}
