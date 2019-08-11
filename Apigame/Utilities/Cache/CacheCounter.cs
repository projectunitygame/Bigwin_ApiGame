using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.IP;

namespace Utilities.Cache
{
    public static class CacheCounter
    {
        /// <summary>
        /// Kiểm tra ip thực hiện 1 hành động trong số giây (tự cộng số lượt mỗi lần gọi hàm check)
        /// Không ăn theo tài khoản
        /// </summary>
        /// <param name="totalSecond">Số giây kiểm tra</param>
        /// <param name="action">tên hành động</param>
        /// <returns>số lượt gọi hành động</returns>
        public static int CheckIpPostFrequency(int totalSecond, string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("P" + ip.ToLower() + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("P" + ip.ToLower() + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("P" + ip.ToLower() + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Đếm số lượt của hành động dựa trên IP
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static int IpActionCounter(string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("P" + ip.ToLower() + "_" + action);
            return Convert.ToInt32(cacheCounter);
        }


        /// <summary>
        /// Xóa ip action cache
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static void IpActionDelete(string action)
        {
            string ip = IPAddressHelper.GetClientIP();
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            cache.Remove("P" + ip.ToLower() + "_" + action);
        }


        /// <summary>
        /// Kiểm tra tài khoản thực hiện 1 hành động trong số giây (tự cộng số lượt mỗi lần gọi hàm check)
        /// </summary>
        /// <param name="accountName">Tên tài khoản</param>
        /// <param name="totalSecond">Số giây kiểm tra</param>
        /// <param name="action">tên hành động</param>
        /// <returns>số lượt gọi hành động</returns>
        public static int CheckAccountActionFrequency(string accountName, int totalSecond, string action)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            System.Runtime.Caching.CacheItemPolicy policy = new System.Runtime.Caching.CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(totalSecond)
            };
            object cacheCounter = cache.Get("P" + accountName + "_" + action);
            if (cacheCounter == null)
            {
                cache.Set("P" + accountName + "_" + action, 1, policy);
                return 0;
            }
            cache.Set("P" + accountName + "_" + action, Convert.ToInt32(cacheCounter) + 1, policy);
            return Convert.ToInt32(cacheCounter);
        }


        /// <summary>
        /// Đếm số lượt của hành động dựa trên tài khoản
        /// </summary>
        /// <param name="accountName">truyền vào tên tài khoản</param>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static int AccountActionCounter(string accountName, string action)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            object cacheCounter = cache.Get("P" + accountName + "_" + action);
            return Convert.ToInt32(cacheCounter);
        }

        /// <summary>
        /// Xóa Account action cache
        /// </summary>
        /// <param name="action">truyền vào tên action</param>
        /// <returns></returns>
        public static void AccountActionDelete(string accountName, string action)
        {
            System.Runtime.Caching.ObjectCache cache = System.Runtime.Caching.MemoryCache.Default;
            cache.Remove("P" + accountName + "_" + action);
        }


    }
}
