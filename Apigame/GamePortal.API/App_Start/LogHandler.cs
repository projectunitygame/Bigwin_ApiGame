using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Utilities.IP;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.App_Start
{
    public class LogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string accountName = AccountSession.AccountName;
            long accountId = AccountSession.AccountID;
            string response = string.Empty;

            string requestBody = await request.Content.ReadAsStringAsync();

            var result = await base.SendAsync(request, cancellationToken);

            if (result.Content != null)
            {
                response = await result.Content.ReadAsStringAsync();
            }

            NLogManager.LogMessage($"REQUEST BODY => API [{request.RequestUri}]" + "\n" +
            $"METHOD [{request.Method}] " + "\n" +
            $"BODY [{requestBody}] " + "\n" +
            $"RESPONSE [{response}] " + "\n" +
            $"IP [{IPAddressHelper.GetClientIP()}] " + "\n" +
            $"AccountName [{accountName}] " + "\n" +
            $"AccountID [{accountId}]");

            return result;
        }
    }

}