using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace GamePortal.API
{
    /// <summary>
    /// Summary description for captcha
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class captcha : System.Web.Services.WebService
    {

        [WebMethod]
        public bool ProxyCheckCaptcha(string captcha, string token)
        {
            try
            {
                return Utilities.Captcha.Verify(captcha, token) > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
