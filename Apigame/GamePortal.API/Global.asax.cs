using GamePortal.API.Controllers.Transaction;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Utilities.Database;
using Utilities.Log;

namespace GamePortal.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            GlobalConfiguration.Configure(WebApiConfig.Register);
            PayIndexCounter.Init();
            NLogManager.LogMessage("***** START SERVER *****");
            //DBHelper db = new DBHelper(GateConfig.DbConfig);
            //NLogManager.LogMessage("Connect db: " + db.OpenConnection().State);
            //db.Close();
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }

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
                        }

                    }

                    if(Context.Request.HttpMethod == "OPTIONS")
                    {
                        this.Response.Clear();
                        this.Response.StatusCode = 200;
                        this.Response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                //NLogManager.PublishException(ex);
            }
        }
    }
}
