using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;
using Utilities.Encryption;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayBB2DUwin
    {
        public static PaymentTopupResponse Topup(string cardSerial, string cardCode, string cardType, int cardAmount, string accountName, string transactionId)
        {
            string partnerKey = "41071d5bd3895339da7c9f571cab7e87";
            string partnerCode = "hng2";
            string serviceCode = "cardtelco";
            string commandCode = "usecard";

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string requestContent = serializer.Serialize(new UseCardRequest()
            {
                CardSerial = cardSerial,
                CardCode = cardCode,
                CardType = cardType,
                AccountName = accountName,
                RefCode = transactionId,
                AmountUser = cardAmount,
                //CallbackUrl = "http://45.76.153.127:8083/transaction/callbackbb2d"
            });
            NLogManager.LogMessage("PAYBB2D DATA: " + requestContent);
            var sig = Security.MD5Encrypt(partnerCode + serviceCode + commandCode + requestContent + partnerKey);
            PayBB2DResponse apiRes = null;
            using (var httpClient = new HttpClient())
            {
                var json = new
                    RequestData()
                {
                    CommandCode = commandCode,
                    PartnerCode = partnerCode,
                    ServiceCode = serviceCode,
                    Signature = sig,
                    RequestContent = requestContent,
                };


                var result = httpClient.PostAsJsonAsync("https://apicard.muathe.shop/VPGJsonService.ashx", json).Result;
                string content = result.Content.ReadAsStringAsync().Result;
                NLogManager.LogMessage("PAYBB2D DATA: " + requestContent + "\nResponse: " + content);
                apiRes = JsonConvert.DeserializeObject<PayBB2DResponse>(content);
            }
            if (string.IsNullOrEmpty(apiRes.ResponseContent))
                apiRes.ResponseContent = "0";

            if (apiRes.ResponseCode == 1)
            {
                return new PaymentTopupResponse
                {
                    Amount = int.Parse(apiRes.ResponseContent),
                    ErrorCode = 1,
                };
            }

            if (apiRes.ResponseCode == -2)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20000000,
                    Message = "Thẻ của bạn đang được hệ thống duyệt, xin vui lòng chờ xác nhận qua hòm thư"
                };
            }

            if (apiRes.ResponseCode > 0)
                apiRes.ResponseCode *= -1;

            return new PaymentTopupResponse
            {
                ErrorCode = apiRes.ResponseCode,
                Message = "Thẻ sai định dạng hoặc thẻ đã được sử dụng"
            };
        }
    }

    public class RequestData
    {
        public string PartnerCode { get; set; }
        public string ServiceCode { get; set; }
        public string CommandCode { get; set; }
        public string RequestContent { get; set; }
        public string Signature { get; set; }
    }

    public class UseCardRequest { public string CardSerial { get; set; } public string CardCode { get; set; } public string CardType { get; set; } public string AccountName { get; set; } public string AppCode { get; set; } public string RefCode { get; set; } public int AmountUser { get; set; } public string CallbackUrl { get; set; } }

    public class PayBB2DResponse
    {
        public int ResponseCode { get; set; }
        public string Description { get; set; }
        public string ResponseContent { get; set; }
        public string Signature { get; set; }
    }
}