using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using SlotGame._25Lines.Models;

[assembly: OwinStartup(typeof(SlotGame._25Lines.Startup))]

namespace SlotGame._25Lines
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           
            var builder = new ContainerBuilder();
            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterType<Checker>().As<IChecker>();
            var container = builder.Build();           
            GlobalHost.DependencyResolver = new AutofacDependencyResolver(container);

            var config = new HubConfiguration() { EnableJSONP = true };
            app.MapSignalR(config);
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
