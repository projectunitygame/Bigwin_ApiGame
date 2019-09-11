using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Utilities.Database;
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
                TokenAuthen = tokenAuthen
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
                    if (string.IsNullOrEmpty(accountInfo.TokenAuthen))
                    {
                        accountInfo.TokenAuthen = token;
                        CacheHandler.Add(token, accountInfo, timeout_cache);
                    }
                    else
                    {
                        var data = (Models.Account)CacheHandler.Get(accountInfo.TokenAuthen);
                        if (data != null)
                        {
                            CacheHandler.Remove(data.TokenAuthen);
                            accountInfo.TokenAuthen = token;
                            CacheHandler.Add(token, accountInfo, timeout_cache);
                        }
                        else
                        {
                            accountInfo.TokenAuthen = token;
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

        public Models.UserInfo AccessToken(string key)
        {
            try
            {
                DBHelper db = new DBHelper(GateConfig.DbConfig);

                List<SqlParameter> pars = new List<SqlParameter>
                {
                    new SqlParameter("@Token", key),
                    new SqlParameter("@DisplayName", System.Data.SqlDbType.NVarChar, 50) { Direction = System.Data.ParameterDirection.Output },
                    new SqlParameter("@AccountID", System.Data.SqlDbType.BigInt) { Direction = System.Data.ParameterDirection.Output },
                    new SqlParameter("@Code", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output }
                };
                db.ExecuteNonQuerySP("SP_GetAccountByToken", pars.ToArray());

                int response = Convert.ToInt32(pars[3].Value);
                if (response > 0)
                {
                    return new UserInfo()
                    {
                        username = pars[1].Value.ToString(),
                        userid = pars[2].Value.ToString()
                    };
                }
                else
                    return null;
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