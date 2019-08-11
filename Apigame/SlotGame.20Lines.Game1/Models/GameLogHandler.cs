using System;
using System.Collections.Generic;
using System.Configuration;
using Newtonsoft.Json;
using SlotMachine.TheThreeKingdoms.Models;
using System.Data;
using SlotGame._20Lines.Game1.Database.DAO;
using SlotGame._20Lines.Game1.Database.DTO;
using SlotGame._20Lines.Game1.Database.Factory;
using Utilities.Log;

namespace SlotGame._20Lines.Game1.Models
{
    public class GameLogHandler
    {
        private static readonly Lazy<GameLogHandler> _instance = new Lazy<GameLogHandler>(() => new GameLogHandler());

        private readonly int HONOR_100 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_100"] ?? "0");
        private readonly int HONOR_1000 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_1000"] ?? "0");
        private readonly int HONOR_5000 = Convert.ToInt32(ConfigurationManager.AppSettings["HORNOR_5000"] ?? "0");
        private readonly int HONOR_10000 = Convert.ToInt32(ConfigurationManager.AppSettings["HONOR_10000"] ?? "0");


        public List<SystemNotify> NotifyList;
        private int maxNotify = 30;
        private object _lock = new object();
        private ISlotMachineDAO _slotMachineDAO = ADODAOFactory.Instance().CreateSlotMachineDAO();

        static GameLogHandler()
        {
        }
        private GameLogHandler()
        {
            NotifyList = new List<SystemNotify>();
        }

        public static GameLogHandler Instance => _instance.Value;

        public void LogSpin(string accountName, int roomId, long totalPrizeValue, int winType)
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


        public DataTable GetHistory(MoneyType moneyType, long accountId, int topCount)
        {
            return _slotMachineDAO.GetHistory(moneyType, accountId, topCount);
        }


        public List<SpinDetail> GetSpinDetail(MoneyType moneyType, long spinId, out string lineData)
        {
            return _slotMachineDAO.GetSpinDetail(moneyType, spinId, out lineData);
        }

        public DataTable GetTop2Jackpot()
        {
            return AbstractDAOFactory.Instance().CreateSlotMachineDAO().GetTop2Jackpot();
        }


        public JackpotHistoryList GetJackpotHistory(MoneyType moneyType, int currentpage, int pageSize)
        {
            return _slotMachineDAO.GetJackpotHistory(moneyType, currentpage, pageSize);
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