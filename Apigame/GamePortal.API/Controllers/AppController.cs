using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Utilities.Encryption;
using Utilities.IP;
using Utilities.Log;

namespace GamePortal.API.Controllers
{
    public class AppController : ApiController
    {
        public async Task<string> fetch()
        {
            try
            {

                    return Security.MD5Encrypt("true");

            }catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return Security.MD5Encrypt("false");
        }
    }

    public class RootObject
    {
        public string city { get; set; }
        public string country { get; set; }
        public string countryCode { get; set; }
        public string isp { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string org { get; set; }
        public string query { get; set; }
        public string region { get; set; }
        public string regionName { get; set; }
        public string status { get; set; }
        public string timezone { get; set; }
        public string zip { get; set; }
    }
}
