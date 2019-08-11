using MiniGame.HiloServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Utilities.Database;
using Utilities.Log;

namespace MiniGame.HiloServer
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            NLogManager.LogMessage("***** START SERVER *****");
            DBHelper db = new DBHelper(ConnectionString.GameConnectionString);
            NLogManager.LogMessage("Connect db: " + db.OpenConnection().State);
            db.Close();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            try
            {
                string origin = Context.Request.Headers["Origin"];

                if (Context.Request != null && Context.Request.Path != null)
                {
                    NLogManager.LogMessage("Connect: " + Utilities.IP.IPAddressHelper.GetClientIP() + "\r\n" + Context.Request.Path);
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
