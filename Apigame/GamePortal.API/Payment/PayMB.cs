using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayMB
    {
        public static PaymentTopupResponse Topup(string cardSerial, string cardCode, int cardType, int cardAmount, string accountName, string transactionId)
        {
            try
            {
                string url = string.Empty;

                NLogManager.LogMessage("Topup MB: " + $"{accountName}|{cardSerial}|{cardCode}|{cardAmount}");

                using (var httpClient = new HttpClient())
                {
                    var formContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("type", cardType.ToString()),
                    new KeyValuePair<string, string>("val", cardAmount.ToString()),
                    new KeyValuePair<string, string>("seri", cardSerial.ToString()),
                    new KeyValuePair<string, string>("code", cardCode),

                    });

                    var res = httpClient.PostAsync("http://localhost:8023/card.php", formContent).Result;
                    url = res.Content.ReadAsStringAsync().Result;
                    NLogManager.LogMessage(url);
                }

                using (var httpClient = new HttpClient())
                {
                    var res = httpClient.GetAsync(url).Result;
                    var response = res.Content.ReadAsStringAsync().Result;
                    NLogManager.LogMessage("Topup MB: " + $"{accountName}|{cardSerial}|{cardCode}|{cardAmount}|Response => " + response);
                    PayMBObjectResponse responObjectPay = JsonConvert.DeserializeObject<PayMBObjectResponse>(response);
                    if(responObjectPay.result.status == 4002)
                    {
                        return new PaymentTopupResponse
                        {
                            Amount = responObjectPay.result.amount.Value,
                            ErrorCode = 1
                        };
                    }

                    if (responObjectPay.result.status > 0)
                        responObjectPay.result.status *= -1;

                    return new PaymentTopupResponse
                    {
                        ErrorCode = responObjectPay.result.status,
                        Message = "Thẻ sai định dạng hoặc thẻ đã được sử dụng"
                    };
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return new PaymentTopupResponse
            {
                ErrorCode = -99,
                Message = "Thẻ sai định dạng hoặc thẻ đã được sử dụng"
            };
        }
    }

    public class PayResult
    {
        public int status { get; set; }
        public string msg { get; set; }
        public int ? amount { get; set; }
    }

    public class PayMBObjectResponse
    {
        public int status { get; set; }
        public string msg { get; set; }
        public PayResult result { get; set; }
        public string transaction_id { get; set; }
    }
}