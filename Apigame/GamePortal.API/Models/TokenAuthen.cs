using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Models
{
    /// <summary>
    /// TokenAuthen access game cá
    /// </summary>
    public class TokenAuthen
    {
        private static int timeout_cache = 18000;

        public string GetTokenAuthenTest()
        {
            string tokenAuthen = GetMd5Hash(Guid.NewGuid().ToString());
            Account acc = new Account()
            {
                AccountID = 999999,
                DisplayName = "Test account",
                tokenAuthen = tokenAuthen
            };

            CacheHandler.Add(tokenAuthen, acc, timeout_cache);
            return tokenAuthen;
        }

        public string GetTokenAuthen(Models.Account accountInfo)
        {
            try
            {
                if (accountInfo != null)
                {
                    string token = GetMd5Hash(Guid.NewGuid().ToString());
                    if (string.IsNullOrEmpty(accountInfo.tokenAuthen))
                    {
                        accountInfo.tokenAuthen = token;
                        CacheHandler.Add(token, accountInfo, timeout_cache);
                    }
                    else
                    {
                        var data = (Models.Account)CacheHandler.Get(accountInfo.tokenAuthen);
                        if (data != null)
                        {
                            CacheHandler.Remove(data.tokenAuthen);
                            accountInfo.tokenAuthen = token;
                            CacheHandler.Add(token, accountInfo, timeout_cache);
                        }
                        else
                        {
                            accountInfo.tokenAuthen = token;
                            CacheHandler.Add(token, accountInfo, timeout_cache);
                        }
                    }
                    return token;
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogError("ERROR GetTokenAuthen: " + ex);
            }
            return string.Empty;
        }

        public Models.Account AccessToken(string key)
        {
            try
            {
                var d = (Models.Account)CacheHandler.Get(key);
                if (d != null)
                {
                    CacheHandler.Remove(d.tokenAuthen);
                    d.tokenAuthen = string.Empty;
                }
                return d;
            }
            catch (Exception ex)
            {
                NLogManager.LogError("ERROR AccessToken: " + ex);
                return null;
            }
        }

        public string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.ASCII.GetBytes(input)); //md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                string hexValue = data[i].ToString("X").ToLower();
                sBuilder.Append((hexValue.Length == 1 ? "0" : "") + hexValue);

            }
            return sBuilder.ToString();
        }
    }

    public class CacheHandler
    {
        static readonly ObjectCache cache = MemoryCache.Default;
        #region Caching
        /// <summary>
        /// Add cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="seconds">thời gian xóa cache</param>
        public static void Add(string key, object value, int seconds)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds);
            cache.Set(key, value, policy);
        }

        public static void Add(string key, object value)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            cache.Set(key, value, policy);
        }

        public static object Get(string key)
        {
            return cache[key];
        }

        public static void Remove(string key)
        {
            cache.Remove(key);
        }
        #endregion
    }
}