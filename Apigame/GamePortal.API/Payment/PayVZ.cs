using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayVZ
    {

        public static PaymentTopupResponse Topup(string cardSerial, string cardCode, int cardType, int cardAmount, string note)
        {
            string card = cardAmount.ToString();
            if (cardType == 2)
                card = "";
            string url = "https://ken.shopdoithe.vn/api/card";
            long merchanatId = 4717872397;
            string merchant_user = "muctnhsm4ak8gl8";
            string merchant_password = "2fe71461cdeadcb18e528e966c97f172f3a695bd1ac0b482086fb4c452ce75ad";
            string plaintText = $"{merchanatId}|{merchant_user}|{merchant_password}|{cardType}|{card}|{cardSerial}|{cardCode}";
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(plaintText);
            byte[] hash = sha256.ComputeHash(bytes);
            string sign = ByteToString(hash);
            string endPoint = $"{url}?merchant_id={merchanatId}&merchant_user={merchant_user}&merchant_password={merchant_password}&card_type={cardType}&card_amount={card}&card_seri={cardSerial}&card_code={cardCode}&note=&sign={sign}";
            NLogManager.LogMessage("TOPUP PAYVZ ENDPOINT: " + endPoint);
            PayVZResponse res = null;
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(endPoint, null).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                NLogManager.LogMessage("TOPUP PAYVZ ENDPOINT: " + endPoint + "\nResponse: " + responseString);
                responseString = responseString.Replace("\xEF\xBB\xBF", "");
                res = JsonConvert.DeserializeObject<PayVZResponse>(responseString);
            }

            if(res.status == 2)
            {
                return new PaymentTopupResponse
                {
                    Amount = res.amount,
                    ErrorCode = 1,
                };
            }else if(res.status == 20)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20,
                    Message = "Mệnh giá thẻ cào không hợp lệ"
                };
            }else if(res.status == 21)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -21,
                    Message = "Số seri không hợp lệ"
                };
            }
            else if (res.status == 22)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -22,
                    Message = "Mã thẻ không hợp lệ"
                };
            }
            else if (res.status == 23)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -23,
                    Message = "Thẻ đã được sử dụng"
                };
            }
            if (res.status > 0)
                res.status *= -1;

            return 
                 new PaymentTopupResponse
                {
                    ErrorCode = res.status,
                    Message = "Hệ thống của chúng tôi đang bận, xin bạn vui lòng thử lại sau"
                };
        }

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("X2"); // hex format
            }
            return (sbinary);
        }
    }

    public class PayVZResponse
    {
        public int status { get; set; }
        public string msg { get; set; }
        public int amount { get; set; }
        public string transaction_id { get; set; }
    }
}