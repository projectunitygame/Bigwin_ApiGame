using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utilities;

namespace PTCN.CrossPlatform.Minigame.LuckyDice
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int a = RandomUtil.NextByte(6);
            Response.Write(a);
        }
    }
}