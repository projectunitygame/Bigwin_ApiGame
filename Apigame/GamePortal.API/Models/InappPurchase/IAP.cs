using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Utilities.Encryption;
using Utilities.Log;

namespace GamePortal.API.Models.InappPurchase
{
    public class IAP
    {
        private static string GOOGLE_CLIENT_ID = "628013364150-stbg660v5fsjft7m8lrdna0refr4euv7.apps.googleusercontent.com";
        private static string GOOGLE_CLIENT_SECRET = "Mwbjx7BlrHFWcMDcLBZF9NrG";
        private static string REFRESH_TOKEN = "1/IvkscAKrjcrhMvm8FKCmIdQRaIVNDbSC6Z3MbNHcTMAxl3QbMiEJiiWN-JBO8t1p";
        private static string LastToken = string.Empty;
        private static DateTime LastGetToken = DateTime.Now;
        /// <summary>
        /// input with receiptCipherPayLoad Ios
        /// 21000 The App Store could not read the JSON object you provided.
        /// 21002 The data in the receipt-data property was malformed or missing.
        /// 21003 The receipt could not be authenticated.
        /// 21004 The shared secret you provided does not match the shared secret on file for your account. Only returned for iOS 6 style transaction receipts for auto-renewable subscriptions.
        /// 21005 The receipt server is not currently available.
        /// 21006 This receipt is valid but the subscription has expired. When this status code is returned to your server, the receipt data is also decoded and returned as part of the response. - Only returned for iOS 6 style transaction receipts for auto-renewable subscriptions.
        /// 21007 This receipt is from the test environment, but it was sent to the production environment for verification. Send it to the test environment instead.
        /// 21008 This receipt is from the production environment, but it was sent to the test environment for verification. Send it to the production environment instead.
        /// </summary>
        /// <param name="receiptInput"></param>
        /// <returns></returns>
        //public static IOSTransaction GetStatusIosTransaction(string receiptInput)
        //{
        //    string responseStr = null;

        //    string uri = ConfigurationManager.AppSettings["ios_iap"];

        //    string receiptData = receiptInput;// Get your receipt from wherever you store it

        //    var json = new JObject(new JProperty("receipt-data", receiptData)).ToString();

        //    using (var httpClient = new HttpClient())
        //    {

        //        if (receiptData != null)
        //        {
        //            HttpContent content = new StringContent(json);
        //            try
        //            {
        //                HttpResponseMessage getResponse = httpClient.PostAsync(uri, content).Result;
        //                responseStr = getResponse.Content.ReadAsStringAsync().Result;
        //            }
        //            catch (Exception e)
        //            {
        //                NLogManager.PublishException(e);
        //            }
        //        }
        //    }

        //    if (responseStr == null)
        //        return null;

        //    return JsonConvert.DeserializeObject<IOSTransaction>(responseStr);
        //}
        public static AndroidTransaction GetStatusAndroidTransaction(string packageName, string productId, string purchaseToken)
        {
            string token = GetGoogleAccessToken();
            if (string.IsNullOrEmpty(token))
                return null;
            string requestUrl = string.Format("https://www.googleapis.com/androidpublisher/v1.1/applications/{0}/inapp/{1}/purchases/{2}?access_token={3}",
                packageName, productId, purchaseToken, token);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage getResponse = httpClient.GetAsync(requestUrl).Result;
                    String RES = getResponse.Content.ReadAsStringAsync().Result;
                    AndroidTransaction result = JsonConvert.DeserializeObject<AndroidTransaction>(RES);

                    if (string.IsNullOrEmpty(result.kind) || result.purchaseTime == 0 || string.IsNullOrEmpty(result.OrderId))
                    {
                        return null;
                    }
                    result.OrderId = Security.MD5Encrypt(purchaseToken);
                    return result;
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }
            }
            return null;
        }
        #region token_refresh
        private static string GetGoogleAccessToken()
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", GOOGLE_CLIENT_ID),
                new KeyValuePair<string, string>("client_secret", GOOGLE_CLIENT_SECRET),
                new KeyValuePair<string, string>("refresh_token", REFRESH_TOKEN)
            });

            using (var httpClient = new HttpClient())
            {
                try
                {
                    if (LastToken == string.Empty || DateTime.Now.Subtract(LastGetToken).TotalMinutes > 3000)
                    {
                        HttpResponseMessage getResponse = httpClient.PostAsync("https://accounts.google.com/o/oauth2/token", content).Result;
                        string res = getResponse.Content.ReadAsStringAsync().Result;
                        JToken t = JToken.Parse(res);
                        LastToken = t["access_token"].ToString();
                        return LastToken;
                    }
                    else
                    {
                        return LastToken;
                    }
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }

            }
            return null;
        }
        #endregion
    }
}