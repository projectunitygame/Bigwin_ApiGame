using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Configuration;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayGA
    {
        public static string KEY_TELCO = WebConfigurationManager.AppSettings["KEY_TELCO"];
        public static PaymentTopupResponse Topup(string cardSerial, string cardCode, string cardType, int cardAmount, string note, string id)
        {
            NLogManager.LogMessage("TOPUP GA INFO: cardSerial = " + cardSerial +
                "\r\ncardCode = " + cardCode +
                "\r\ncardType = " + cardType +
                "\r\ncardAmount = " + cardAmount +
                "\r\nnote = " + note + 
                "\r\nid = " + id);
            //string url = $"http://naptien.ga/api/SIM/RegCharge?apiKey=e914760f-86c9-4f37-8f78-b7e4c785367f&code={cardCode}&serial={cardSerial}&type={cardType}&menhGia={cardAmount}&requestId={id}";
            string url = string.Format("http://naptien.ga/api/SIM/RegCharge?apiKey={0}&code={1}&serial={2}&type={3}&menhGia={4}&requestId={5}", KEY_TELCO, cardCode, cardSerial, cardType, cardAmount, id);//$"http://naptien.ga/api/SIM/RegCharge?apiKey={KEY_TELCO}&code={cardCode}&serial={cardSerial}&type={cardType}&menhGia={cardAmount}&requestId={id}";
            NLogManager.LogMessage("TOPUP GA ENDPOINT: " + url);
            GAResponse res = null;
            using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(url).Result;
                NLogManager.LogMessage("TOPUP PAYVZ ENDPOINT: " + url + "\nResponse: " + response);
                res = JsonConvert.DeserializeObject<GAResponse>(response);
            }

            if (res.stt == 1)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20000000,
                    Message = "Thẻ của bạn đang được hệ thống duyệt, xin vui lòng chờ xác nhận qua hòm thư"
                };
            }
            else if (res.stt == 0)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20,
                    Message = "Thẻ không được chấp nhận, kiểm tra mã và serial, hoặc thẻ đã tồn tại -> KHÔNG ĐƯỢC GỬI LẠI THẺ NÀY"
                };
            }

            if (res.stt > 0)
                res.stt *= -1;

            return
                 new PaymentTopupResponse
                 {
                     ErrorCode = res.stt,
                     Message = "Nạp thẻ thất bại"
                 };
        }
    }


    public class Data
    {
        public string id { get; set; }
    }

    public class GAResponse
    {
        public int stt { get; set; }
        public string msg { get; set; }
        public Data data { get; set; }
    }
}