using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Utilities.Log;

namespace SlotGame._25Lines.Handlers
{

    public class HonorHandler
    {
        private static readonly Lazy<HonorHandler> _instance = new Lazy<HonorHandler>(() => new HonorHandler());

        private readonly int HONOR_100 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_100"] ?? "0");
        private readonly int HONOR_1000 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_1000"] ?? "0");
        private readonly int HONOR_5000 = Convert.ToInt32(ConfigurationManager.AppSettings["HORNOR_5000"] ?? "0");
        private readonly int HONOR_10000 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_10000"] ?? "0");


        public List<SystemNotify> NotifyList;
        private int maxNotify = 30;
        private object _lock = new object();

        static HonorHandler()
        {
        }
        private HonorHandler()
        {
            NotifyList = new List<SystemNotify>();
        }

        public static HonorHandler Instance => _instance.Value;

        public void SaveHonor(string accountName, int roomId, long totalPrizeValue, int winType)
        {
            try
            {
                try
                {
                    lock (_lock)
                    {
                        long honorValue = (roomId == 4 ? HONOR_10000 : roomId == 3 ? HONOR_5000 : roomId == 2 ? HONOR_1000 : HONOR_100);
                        // Vinh danh nổ quỹ và thắng lớn
                        if (totalPrizeValue > 0)
                        {
                            if (totalPrizeValue <= honorValue) return;
                            var notify = new SystemNotify
                            {
                                RoomId = roomId,
                                AccountName = accountName,
                                PrizeValue = totalPrizeValue,
                                CreatedDate = DateTime.Now,
                                WinType = winType
                            };

                            NotifyList.Add(notify);
                            while (NotifyList.Count > maxNotify)
                            {
                                NotifyList.RemoveAt(0);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
            }
        }
    }

    public class SystemNotify
    {
        public string AccountName { get; set; }
        public int RoomId { get; set; }
        public long PrizeValue { get; set; }
        [JsonIgnore]
        public DateTime CreatedDate { get; set; }
        public int WinType { get; set; }
        public string Message { get; set; }
    }
}