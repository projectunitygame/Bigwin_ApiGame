using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Studio.WebGame.SupperNova.Models
{
	public class FreezeStarResponse
	{
		public long FreezeStarBalance { set; get; }
		public long StarBalance { set; get; }
		public string Description { set; get; }
		public string Extend { set; get; }
		public int ResponseCode { set; get; }
	}

    public class VerifyMobileResponse
    {
        public long AccountID { set; get; }
        public string AccountName { set; get; }
        public string Email { set; get; }
        public string JoinedTime { set; get; }
        public string JoinedUnixTime { set; get; }
        public string Mobile { set; get; }
        public string Password { set; get; }
        public int Status { set; get; }
    }
}
