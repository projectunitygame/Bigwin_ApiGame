using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Encryption;

namespace Utilities.Util
{
    public static class ConnectionStringUtil
    {
        public static string Decrypt(string connectionString, bool decrypt = true)
        {
            if(!decrypt)
                return string.IsNullOrEmpty(connectionString) ? string.Empty : connectionString;
            else
                return string.IsNullOrEmpty(connectionString) ? string.Empty : new RijndaelEnhanced("rongclub88Key", "@1B2c3D4e5F6g7H8").Decrypt(connectionString);
        }

        public static string Encrypt(string connectionString)
        {
            //return string.IsNullOrEmpty(connectionString) ? string.Empty : connectionString;
            return string.IsNullOrEmpty(connectionString) ? string.Empty : new RijndaelEnhanced("rongclub88Key", "@1B2c3D4e5F6g7H8").Encrypt(connectionString);
        }
    }
}
