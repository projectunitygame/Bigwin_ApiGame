using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Encryption;

namespace Utilities.IP
{
    public static class IPAddressHelper
    {
        public static string GetClientIP()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;

            if (context.Request.Headers["CF-CONNECTING-IP"] != null)
                return context.Request.Headers["CF-CONNECTING-IP"];

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        private static string[] range = new string[] { "125.234", "125.235", "203.113", "220.231", "203.41", "203.162", "203.210", "222.255", "210.245" };
        public static string GetClientIp(string username)
        {
            try
            {
                string md5Name = Security.MD5Encrypt(username);
                byte[] array = Encoding.ASCII.GetBytes(md5Name);

                if (BitConverter.IsLittleEndian) Array.Reverse(array);
                int length = array.Length;

                int val1 = BitConverter.ToInt32(array, 0);
                int val2 = BitConverter.ToInt32(array, 1);
                int val3 = BitConverter.ToInt32(array, 2);

                val1 = val1 % range.Length;
                val2 = val2 % 254 + 1;
                val3 = val3 % 254 + 1;

                return string.Format("{0}.{1}.{2}", range[val1], val2, val3);
            }
            catch (Exception)
            {

            }
            return GetClientIP();
        }
    }
}
