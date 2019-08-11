using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayKHAN
    {
        public static async Task<PaymentTopupResponse> Topup(string cardSerial, string cardCode, string cardType, int cardAmount, string accountName, string transactionId)
        {
            try
            {
                string uid = "103";
                string secretKey = "fcca3731c37";
                string ts = DateTime.Now.Ticks.ToString();
                SHA256 sha256 = SHA256Managed.Create();
                byte[] bytes = Encoding.UTF8.GetBytes($"{transactionId}{uid}{cardSerial}{cardCode}{cardAmount}{cardType}{ts}{secretKey}");
                byte[] hash = sha256.ComputeHash(bytes);
                string sign = ByteToString(hash);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(60);
                    var formContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("id", transactionId),
                    new KeyValuePair<string, string>("uid", uid),
                    new KeyValuePair<string, string>("seri", cardSerial),
                    new KeyValuePair<string, string>("code", cardCode),
                    new KeyValuePair<string, string>("telco", cardType),
                    new KeyValuePair<string, string>("money", cardAmount.ToString()),
                    new KeyValuePair<string, string>("ts", ts),
                    new KeyValuePair<string, string>("pk", sign),
                });
                    NLogManager.LogMessage("Topup PayKHAN: " + $"{transactionId}|{accountName}|{cardSerial}|{cardCode}|{cardAmount}|{cardType}");
                    var data = await client.PostAsync("http://api.thecao.co/card/input", formContent);
                    var stringStr = await data.Content.ReadAsStringAsync();
                    NLogManager.LogMessage("Topup PayKHAN: " + $"{accountName}|{cardSerial}|{cardCode}|{cardAmount}|Response => " + stringStr);
                    KhanResponse response = JsonConvert.DeserializeObject<KhanResponse>(stringStr);

                    if(response.ErrCode == 0 && response.Money > 0)
                    {
                        return new PaymentTopupResponse
                        {
                            Amount = response.Money,
                            ErrorCode = 1
                        };
                    }

                    if (response.ErrCode > 0)
                        response.ErrCode *= -1;

                    return new PaymentTopupResponse
                    {
                        ErrorCode = response.ErrCode,
                        Message = "Thẻ sai định dạng hoặc thẻ đã được sử dụng"
                    };
                }

            }
            catch (TaskCanceledException)
            {
                return new PaymentTopupResponse
                {
                    ErrorCode = -20000000,
                    Message = "Thẻ của bạn đang được hệ thống duyệt, xin vui lòng chờ xác nhận qua hòm thư"
                };
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

        private static string ByteToString(byte[] buff)
        {
            string sbinary = "";

            for (int i = 0; i < buff.Length; i++)
            {
                sbinary += buff[i].ToString("x2"); // hex format
            }
            return (sbinary);
        }
    }

    public class KhanResponse
    {
        public int ErrCode { get; set; }
        public string Message { get; set; }
        public int Money { get; set; }
    }
}