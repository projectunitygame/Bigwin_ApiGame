using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using MinigameVuabai.SignalR.Controllers;
using System.Configuration;
using SlotMachine.Mini.TheSpinOfGod.Models;

namespace SlotMachine.Mini.TheSpinOfGod
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //Room.Init();
            ConnectionHandler.Instance.Init();

            var aTimer = new System.Timers.Timer(1000);

            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Interval = int.Parse(ConfigurationManager.AppSettings["TimmerJackport"]);
            aTimer.Enabled = true;


            //var getDBJackpotTimmer = new System.Timers.Timer(1000);

            //getDBJackpotTimmer.Elapsed += getDBJackpotTimmer_Elapsed;
            //getDBJackpotTimmer.Interval = int.Parse(ConfigurationManager.AppSettings["TimmerGetJackport"]);
            //getDBJackpotTimmer.Enabled = true;
        }

        void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ConnectionHandler.Instance.UpdateClientJackport();
        }
        //void getDBJackpotTimmer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    ConnectionHandler.Instance.GetAllDbJackport();
        //}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                string origin = Context.Request.Headers["Origin"];

                if (Context.Request != null && Context.Request.Path != null)
                {

                    if (!string.IsNullOrEmpty(origin))
                    {
                        //if (ConfigurationManager.AppSettings["CROSS_DOMAIN"].Contains("|" + origin + "|"))
                        {
                            if (this.Context.Request.Path.Contains("signalr/negotiate"))
                            {
                                this.Context.Request.Headers.Remove("Origin");
                            }
                            else
                            {
                                this.Context.Response.AppendHeader("Access-Control-Allow-Methods", "GET, POST , OPTIONS");
                            }
                            this.Context.Response.Headers.Remove("Access-Control-Allow-Origin");
                            this.Context.Response.Headers.Remove("Access-Control-Allow-Headers");
                            this.Context.Response.Headers.Remove("Access-Control-Allow-Credentials");

                            this.Context.Response.Headers.Set("Access-Control-Allow-Origin", origin);
                            this.Context.Response.AddHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                            this.Context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                            if (Context.Request.HttpMethod == "OPTIONS")
                            {
                                this.Response.Clear();
                                this.Response.StatusCode = 200;
                                this.Response.End();
                            }

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                // NLogManager.PublishException(ex);
            }
        }
    }
}