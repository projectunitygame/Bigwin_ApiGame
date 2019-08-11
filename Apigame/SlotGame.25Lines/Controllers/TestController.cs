using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SlotGame._25Lines.Models.Services;
using Utilities.Session;


namespace SlotGame._25Lines.Controllers
{

    public class TestController : ApiController
    {
        private static readonly ITestService _test = new TestService();

        [HttpGet]
        [Authorize]
        public int SetTestData(string data)
        {
            var accountName = AccountSession.AccountName;
            return string.IsNullOrEmpty(accountName) ? -99 : _test.IsTestAccount(accountName) ? _test.SetTestData(data) : -98;
        }
        [HttpGet]
        [Authorize]
        public string GetTestData()
        {
            var accountName = AccountSession.AccountName;
            return string.IsNullOrEmpty(accountName) ? "" : _test.IsTestAccount(accountName) ? string.Join(",", _test.GetTestData().Select(x => x.ToString()).ToArray())  : "";
        }
    }
}
