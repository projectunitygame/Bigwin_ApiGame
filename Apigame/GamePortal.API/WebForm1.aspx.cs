using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utilities.Util;

namespace GamePortal.API
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //string s = ConnectionStringUtil.Encrypt("Data Source=167.179.104.36;Initial Catalog=eBankGame.SlotIslandsDB;Persist Security Info=True;User ID=sa;Password=Uwin@2019");
            //Response.Write(s);
            //Response.Write(GateConfig.DbConfig);


            //string ss=ConnectionStringUtil.Decrypt("W7Q4kWfsK0R6irXbNotdxYv0z8WQfK8G4k9ZLo9HRS5Zp8RQo8+DVDYrENF+r1BO3PF7iuxuZTrIbQD0yWQvcgQsoBanGg1toQwcKBMseMvugIklTth9Orv63oQhULdpPNqiQ5VuNUmgELXTlgQsu6PGO9RbLEMM85MO/D5ZcmA=");
            //string s = ConnectionStringUtil.Encrypt(ss.Replace("45.32.99.34", "167.179.104.36"));
            //Response.Write(ss);
        }
    }
}