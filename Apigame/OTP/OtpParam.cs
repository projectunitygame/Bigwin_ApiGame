using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTP
{
    public class OtpParam
    {
        /// <summary>
        /// Counter parameter 
        /// </summary>
        public long C { get; set; }
        /// <summary>
        /// Security key
        /// </summary>
        public string T { get; set; }
        public string AppT { get; set; }
    }
}
