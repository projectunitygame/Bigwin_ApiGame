using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Payment
{
    public class PayTimo
    {
        public static async Task <PaymentTopupResponse> Topup(string cardSerial, string cardCode, string cardType, int cardAmount, string accountName, string transactionId)
        {
            cardObj ca = new cardObj();
            ca.Serial = cardSerial;
            ca.Code = cardCode;
            ca.Key = cardType;
            ca.Request_id = transactionId;
            ca.Key = cardType;
            ca.Amount = cardAmount;

            var response = await new processCard().PostCardToService(ca);

            if(response.Status == 0 && response.Amount > 0)
            {
                return new PaymentTopupResponse
                {
                    Amount = response.Amount,
                    ErrorCode = 1
                };
            }

            if (response.Status > 0)
                response.Status *= -1;

            return new PaymentTopupResponse
            {
                ErrorCode = response.Status,
                Message = "Thẻ sai định dạng hoặc thẻ đã được sử dụng"
            };
        }

    }

    /// <summary>
    /// Khởi tạo biến chứa các giá trị gửi đi và nhận về
    /// </summary>
    public partial class cardObj
    {
        #region Khai báo dữ liệu chuyển lên service
        private string key; // loại thẻ
        private string code; // số thẻ
        private string serial; // mã thẻ
        private string request_id; // khởi tạo khi request lên service, đối tượng là duy nhất
        private string partnerID = "1537948008"; // tên ID TimoPay cấp
        private string partnerKey = "ed23e94863ff"; // password TimoPay cấp
        private string checksum; // đối tượng là duy nhất, dùng để check giao dịch khi có khiếu nại
        public string Key
        {
            get { return key; }
            set { key = value; }
        }
        public string Code
        {
            get { return code; }
            set { code = value; }
        }
        public string Serial
        {
            get { return serial; }
            set { serial = value; }
        }
        public string Request_id
        {
            get { return request_id; }
            set { request_id = value; }
        }
        public string PartnerID
        {
            get { return partnerID; }
            set { partnerID = value; }
        }
        public string PartnerKey
        {
            get { return partnerKey; }
            set { partnerKey = value; }
        }
        public string Checksum
        {
            get { return checksum; }
            set { checksum = value; }
        }
        #endregion

        #region Khai báo dữ liệu trả về từ service
        private int status; // trạng thái trả về khi request lên service
        private string message;// Nội dung trả về từ service
        private int amount; // số tiền nạp thành công
        private string transid; // mã giao dịch bên TimoPay

        public int Status
        {
            get { return status; }
            set { status = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        public string Transid
        {
            get { return transid; }
            set { transid = value; }
        }

        #endregion

    }

    /// <summary>
    /// hảm xử lý get và post card từ service
    /// </summary>
    public class processCard
    {
        public async Task<cardObj> PostCardToService(cardObj ca)
        {
            try
            {
                ca.Checksum = GetMD5(ca.PartnerID + ca.Key + ca.Serial + ca.Request_id);

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(100);
                    var formContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string, string>("key", ca.Key),
                    new KeyValuePair<string, string>("code", ca.Code),
                    new KeyValuePair<string, string>("serial", ca.Serial),
                    new KeyValuePair<string, string>("request_id", ca.Request_id),
                    new KeyValuePair<string, string>("PartnerID", ca.PartnerID),
                    new KeyValuePair<string, string>("PartnerKey", ca.PartnerKey.ToString()),
                    new KeyValuePair<string, string>("checksum", ca.Checksum),
                });

                    var data = await client.PostAsync("https://timopay.com/card_charging_api.html", formContent);
                    var stringStr = await data.Content.ReadAsStringAsync();
                    string res = Base64Decode(stringStr);
                    NLogManager.LogMessage(JsonConvert.SerializeObject(ca) + "|Response TIMO: " + res);
                    dynamic stuff = JsonConvert.DeserializeObject(res);
                    cardObj ce = new cardObj();
                    ce.Status = -99;
                    if (stuff.status != null)
                    {
                        ce.Status = Convert.ToInt32(stuff.status.ToString());
                    }
                    if (stuff.message != null)
                    {
                        ce.Message = stuff.message;
                    }
                    if (stuff.amount != null)
                    {
                        ce.Amount = stuff.amount;
                    }
                    if (stuff.key != null)
                    {
                        ce.Key = stuff.key;
                    }
                    if (stuff.serial != null)
                    {
                        ce.Serial = stuff.serial;
                    }
                    if (stuff.code != null)
                    {
                        ce.Code = stuff.code;
                    }
                    if (stuff.request_id != null)
                    {
                        ce.Request_id = stuff.request_id;
                    }
                    if (stuff.transid != null)
                    {
                        ce.Transid = stuff.transid;
                    }
                    return ce;
                }
            }catch(Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            cardObj ce1 = new cardObj();
            ce1.Status = -99;

            return ce1;
        }
        public cardObj GetListCardFromService(cardObj ca)
        {
            ASCIIEncoding enCoding = new ASCIIEncoding();
            string postdata = detectData(ca, "get");
            string URL = "https://timopay.com/card_charging_api/get_card_keys.html";
            byte[] data = enCoding.GetBytes(postdata);

            WebRequest request = WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            Stream stream = request.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();

            WebResponse response = request.GetResponse();
            stream = response.GetResponseStream();

            using (StreamReader responseReader = new StreamReader(stream, Encoding.ASCII))
            {
                string sdata = responseReader.ReadToEnd();
                string res = Base64Decode(sdata);
                dynamic stuff = JsonConvert.DeserializeObject(res);
                cardObj ce = new cardObj();
                if (stuff.status != null)
                {
                    ce.Status = Convert.ToInt32(stuff.status.ToString());
                }
                if (stuff.message != null)
                {
                    ce.Message = stuff.message;
                }
                return ce;

            }

        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string GetMD5(string str)
        {

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            byte[] bHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));

            StringBuilder sbHash = new StringBuilder();

            foreach (byte b in bHash)
            {
                sbHash.Append(String.Format("{0:x2}", b));
            }

            return sbHash.ToString();
        }

        public static string detectData(cardObj ca, string type)
        {
            string data = "";
            if (type == "get")
            {
                data = "PartnerID=" + ca.PartnerID + "&PartnerKey=" + ca.PartnerKey;
            }
            else if (type == "post")
            {
                data = "key=" + ca.Key + "&Code=" + ca.Code + "&serial=" + ca.Serial + "&request_id=" + ca.Request_id + "&PartnerID=" + ca.PartnerID + "&PartnerKey=" + ca.PartnerKey + "&checksum=" + ca.Checksum;
            }
            return data;
        }


    }
}