using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace MinigameVuabai.SignalR.Controllers
{
    public class CachingHandler
    {
        #region Caching
        public static void SetOrUpdateCache<T>(string key, T value, int? seconds)
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            if (seconds.HasValue)
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(seconds.Value);
            else
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30);
            cache.Set(key, value, policy);
        }

        public static object GetCacheValue(string key)
        {
            ObjectCache cache = MemoryCache.Default;
            return cache[key];
        }
        /// <summary>
        /// Check Account Action
        /// </summary>
        /// <param name="accountName"></param>
        public static int AddAccountAction(string accountName, string action, int seconds)
        {
            string key = "P_" + accountName + "_" + action;
            object value = GetCacheValue(key);
            int counter = 0;
            counter = value == null ? 1 : Convert.ToInt32(value) + 1;
            SetOrUpdateCache<int>(key, counter, seconds);
            return counter;
        }

        public static int CheckAccountAction(string accountName, string action)
        {
            string key = "P_" + accountName + "_" + action;
            object value = GetCacheValue(key);
            return value == null ? 0 : Convert.ToInt32(value);
        }

        #endregion
    }
}