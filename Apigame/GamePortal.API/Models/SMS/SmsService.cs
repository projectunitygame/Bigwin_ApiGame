using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Utilities.HttpHelper;
using Utilities.Log;

namespace GamePortal.API.Models.SMS
{
    public class SmsService
    {
        public static void SendMessage(string phone, string message)
        {
            string msgSent = RemoveSign4VietnameseString(message);//HttpUtility.UrlEncode(message);
            string ApiKey = "3F11E064600D5C3E073FA1B033831B";
            string SecretKey = "C8F2CCDA4C9B32CAC4E3EFDD1C08B8";
            string brandName = "Svoucher";
            //string SmsType = "4";
            string SmsType = "2";
            //string url = $"http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone={phone}&Content={msgSent}&ApiKey={ApiKey}&SecretKey={SecretKey}&SmsType={SmsType}&brandname=Verify";
            //string url = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone="+phone+"&Content="+msgSent+"&ApiKey="+ApiKey+"&SecretKey="+SecretKey+"&SmsType="+SmsType+"&brandname=Verify";
            string url = "http://rest.esms.vn/MainService.svc/json/SendMultipleMessage_V4_get?Phone=" + phone + "&Content=" + msgSent + "&ApiKey=" + ApiKey + "&SecretKey=" + SecretKey + "&SmsType=" + SmsType + "&brandname=" + brandName;
            NLogManager.LogMessage("SendMessage: " + url);
            string result = WebRequest(Method.GET, url);
            NLogManager.LogMessage("SendMessage result: " + result);
            //string result = HttpUtils.GetStringHttpResponse(url);

        }

        public enum Method { GET, POST, PUT };

        public static string WebRequest(Method method, string url, string postData = "")
        {
            if (string.IsNullOrEmpty(url))
                return "";
            HttpWebRequest webRequest = null;
            StreamWriter requestWriter = null;
            string responseData = "";

            webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
            webRequest.Method = method.ToString();
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.UserAgent = "";
            webRequest.Timeout = 300000;

            if (method == Method.POST)
            {
                webRequest.ContentType = "application/x-www-form-urlencoded";

                //POST the data.
                requestWriter = new StreamWriter(webRequest.GetRequestStream());

                try
                {
                    requestWriter.Write(postData);
                }
                catch (Exception ex)
                {
                    NLogManager.LogError("ERROR WebRequest: " + url + "\r\n" + ex);
                }

                finally
                {
                    requestWriter.Close();
                    requestWriter = null;
                }
            }

            responseData = WebResponseGet(webRequest);
            webRequest = null;
            return responseData;
        }

        public static string WebResponseGet(HttpWebRequest webRequest)
        {
            StreamReader responseReader = null;
            string responseData = "";

            try
            {
                responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
                responseData = responseReader.ReadToEnd();
            }
            catch
            {
                throw;
            }
            finally
            {
                webRequest.GetResponse().GetResponseStream().Close();
                responseReader.Close();
                responseReader = null;
            }

            return responseData;
        }

        private static readonly string[] VietnameseSigns = new string[]
        {
          "aAeEoOuUiIdDyY",
          "áàạảãâấầậẩẫăắằặẳẵ",
          "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
          "éèẹẻẽêếềệểễ",
          "ÉÈẸẺẼÊẾỀỆỂỄ",
          "óòọỏõôốồộổỗơớờợởỡ",
          "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
          "úùụủũưứừựửữ",
          "ÚÙỤỦŨƯỨỪỰỬỮ",
          "íìịỉĩ",
          "ÍÌỊỈĨ",
          "đ",
          "Đ",
          "ýỳỵỷỹ",
          "ÝỲỴỶỸ"
         };

        public static string RemoveSign4VietnameseString(string s)
        {
            string str = convertToUnSign3(s);
            //Tiến hành thay thế , lọc bỏ dấu cho chuỗi
            for (int i = 1; i < VietnameseSigns.Length; i++)
            {
                for (int j = 0; j < VietnameseSigns[i].Length; j++)
                    str = str.Replace(VietnameseSigns[i][j], VietnameseSigns[0][i - 1]);
            }
            return str;
        }

        public static string convertToUnSign3(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }
    }

    class SpeedSMSAPI
    {
        //public const int TYPE_QC = 1;
        //public const int TYPE_CSKH = 2;
        //public const int TYPE_BRANDNAME = 3;
        //public const int TYPE_BRANDNAME_NOTIFY = 4; // Gửi sms sử dụng brandname Notify
        //public const int TYPE_GATEWAY = 5; // Gửi sms sử dụng app android từ số di động cá nhân, download app tại đây: https://play.google.com/store/apps/details?id=com.speedsms.gateway
        //public const int TYPE_FIXNUMBER = 6; //sms gui bang dau so co dinh 0901756186
        //public const int TYPE_OWN_NUMBER = 7; //sms gui bang dau so rieng da duoc dang ky voi SpeedSMS
        //public const int TYPE_TWOWAY_NUMBER = 8; //sms gui bang dau so co dinh 2 chieu

        //const String rootURL = "https://api.speedsms.vn/index.php";
        //private String accessToken = "";

        //public SpeedSMSAPI()
        //{

        //}

        //public SpeedSMSAPI(String token)
        //{
        //    this.accessToken = token;
        //}

        //private string EncodeNonAsciiCharacters(string value)
        //{
        //    System.Text.StringBuilder sb = new System.Text.StringBuilder();
        //    foreach (char c in value)
        //    {
        //        if (c > 127)
        //        {
        //            // This character is too big for ASCII
        //            string encodedValue = "\\u" + ((int)c).ToString("x4");
        //            sb.Append(encodedValue);
        //        }
        //        else
        //        {
        //            sb.Append(c);
        //        }
        //    }
        //    return sb.ToString();
        //}

        //public String getUserInfo()
        //{
        //    String url = rootURL + "/user/info";
        //    NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
        //    WebClient client = new WebClient();
        //    client.Credentials = myCreds;
        //    Stream data = client.OpenRead(url);
        //    StreamReader reader = new StreamReader(data);
        //    return reader.ReadToEnd();
        //}

        //public String sendSMS(String[] phones, String content, int type, String sender)
        //{
        //    String url = rootURL + "/sms/send";
        //    if (phones.Length <= 0)
        //        return "";
        //    if (content.Equals(""))
        //        return "";

        //    if (type == TYPE_BRANDNAME && sender.Equals(""))
        //        return "";
        //    if (!sender.Equals("") && sender.Length > 11)
        //        return "";

        //    NetworkCredential myCreds = new NetworkCredential(accessToken, ":x");
        //    WebClient client = new WebClient();
        //    client.Credentials = myCreds;
        //    client.Headers[HttpRequestHeader.ContentType] = "application/json";

        //    string builder = "{\"to\":[";

        //    for (int i = 0; i < phones.Length; i++)
        //    {
        //        builder += "\"" + phones[i] + "\"";
        //        if (i < phones.Length - 1)
        //        {
        //            builder += ",";
        //        }
        //    }
        //    builder += "], \"content\": \"" + EncodeNonAsciiCharacters(content) + "\", \"type\":" + type + ", \"sender\": \"" + sender + "\"}";

        //    String json = builder.ToString();
        //    return client.UploadString(url, json);
        //}
    }
}