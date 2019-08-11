using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Utilities.Encryption;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayTichHop
    {
        static string _partnerId = "TH1810090000";
        static string _partnerKey = "641749522831";
        static List<string> _port = new List<string>() { "1", "7", "15", "30" }; 
        public static PaymentTopupResponse Topup(string cardSerial, string cardCode, string cardType, int cardAmount, string accountName, string ip, string transactionId)
        {
            NLogManager.LogMessage("Topup TiMo: " + $"{accountName}|{cardSerial}|{cardCode}|{cardAmount}");
            ResponseTichHop response = null; 
            using (var httpClient = new HttpClient())
            {
                var formContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("PartnerID", _partnerId),
                    new KeyValuePair<string, string>("PartnerKey", _partnerKey),
                    new KeyValuePair<string, string>("port", _port.OrderBy(x => Guid.NewGuid()).FirstOrDefault()),
                    new KeyValuePair<string, string>("type", cardType),
                    new KeyValuePair<string, string>("code", cardCode),
                    new KeyValuePair<string, string>("serial", cardSerial),
                    new KeyValuePair<string, string>("tranid", transactionId),
                    new KeyValuePair<string, string>("ext", accountName),
                    new KeyValuePair<string, string>("sign", Security.MD5Encrypt(_partnerId + cardType + cardCode + cardSerial + transactionId)),
                    new KeyValuePair<string, string>("ip_user", ip),

                    });

                var res = httpClient.PostAsync("http://tichhop.vn/charging.html", formContent).Result;
                string ress = res.Content.ReadAsStringAsync().Result;
                ress = Base64Decode(ress);
                NLogManager.LogMessage($"{accountName}|{cardSerial}|{cardCode}|{cardAmount}" + "|Response TIMO: " + ress);
                response = JsonConvert.DeserializeObject<ResponseTichHop>(ress);

            }

            string msg = "Thẻ sai định dạng hoặc thẻ đã được sử dụng";

            if (response.status == 0)
                response.status = -1;

            if(response.status == 1)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = 1,
                    Amount = response.amount.Value
                };
            }

            if(response.status == 2)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20000000,
                    Message = "Thẻ của bạn đang được hệ thống duyệt, xin vui lòng chờ xác nhận qua hòm thư"
                };
            }

            if (response.status == 3)
                msg = "Nạp thẻ thất bại, xin vui lòng thử nạp lại";

            if (response.status > 0)
                response.status *= -1;

            return new PaymentTopupResponse
            {
                ErrorCode = response.status,
                Message = msg
            };
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class ResponseTichHop
    {
        public int status { get; set; }
        public string message { get; set; }
        public int ? amount { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public string serial { get; set; }
        public string tranid { get; set; }
    }
}