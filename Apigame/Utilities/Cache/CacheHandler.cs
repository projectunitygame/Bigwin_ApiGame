using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Cache
{
    public static class CacheHandler
    {
        static readonly ObjectCache cache = MemoryCache.Default;
        #region Caching
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

        /// <summary>
        /// Check Account Action
        /// </summary>
        /// <param name="accountName"></param>
        public static int AddAccountAction(string accountName, string action, int seconds)
        {
            string key = "P_" + accountName + "_" + action;
            object value = Get(key);
            int counter = 0;
            counter = value == null ? 1 : Convert.ToInt32(value) + 1;
            Add(key, counter, seconds);
            return counter;
        }

        public static int CheckAccountAction(string accountName, string action)
        {
            string key = "P_" + accountName + "_" + action;
            object value = Get(key);
            return value == null ? 0 : Convert.ToInt32(value);
        }

        public static void RemoveAccountAction(string accountName, string action)
        {
            var key = "P_" + accountName + "_" + action;
            Remove(key);
        }

        #endregion
    }
}
