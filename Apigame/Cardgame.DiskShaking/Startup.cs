using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Integration.SignalR;
using Cardgame.DiskShaking.Container;
using Cardgame.DiskShaking.Controllers;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Cardgame.DiskShaking.Startup))]

namespace Cardgame.DiskShaking
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var builder = new ContainerBuilder();

            builder.RegisterHubs(Assembly.GetExecutingAssembly());
            builder.RegisterType<PlayerManager>().SingleInstance();
            builder.RegisterType<GameManager>().SingleInstance();
            builder.RegisterType<ConnectionHandler>().SingleInstance();
            var container = builder.Build();
            GlobalHost.DependencyResolver = new AutofacDependencyResolver(container);

            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888a
            app.MapSignalR();

        }
    }
}
