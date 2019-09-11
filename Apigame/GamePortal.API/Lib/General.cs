using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace GamePortal.API.Lib
{
    public class General
    {
        public static string FormatMoneyVND(long price)
        {
            return price.ToString("0,0", CultureInfo.CreateSpecificCulture("el-GR"));
        }
    }
}