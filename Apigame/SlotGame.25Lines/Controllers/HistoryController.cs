using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Data;
using System.Globalization;
using SlotGame._25Lines.Database.DAO;
using SlotGame._25Lines.Database.DAOImpl;
using SlotGame._25Lines.Database.DTO;
using SlotGame._25Lines.Handlers;
using Utilities.Log;
using Utilities.Session;

namespace SlotGame._25Lines.Controllers
{
    public class HistoryController : ApiController
    {
        private static readonly ISlotMachine25Lines _dao = new SlotMachine25LinesImpl();
        [HttpOptions, HttpGet]
        [ActionName("GetHistory")]
        public DataTable GetHistory(MoneyType moneyType)
        {
            var accountId = AccountSession.AccountID;
            return accountId < 1 ? null : _dao.GetHistory(moneyType, accountId, 100);
        }

        /// <summary>
        /// API lay danh sach jackpot
        /// </summary>
        /// <returns></returns>
        [HttpOptions, HttpGet]
        [ActionName("GetHistoryJackPot")]
        public JackpotHistoryList GetHistoryJackPot(MoneyType moneyType, int currentPage, int pageSize)
        {
            return _dao.GetJackpotHistory(moneyType, currentPage, pageSize);
        }

        [HttpOptions, HttpGet]
        [ActionName("GetSystemNotify")]
        public List<SystemNotify> GetSystemNotify()
        {
            var dataResult = HonorHandler.Instance.NotifyList;
            var responseList = new List<SystemNotify>(dataResult);
            responseList.Reverse();
            foreach (var item in responseList)
            {
                var sTime = string.Empty;

                var time = (DateTime.Now - item.CreatedDate);
                if (time.Hours <= 0)
                {
                    if (time.Minutes <= 0)
                    {
                        sTime = (time.Seconds + 1) + " giây";
                    }
                    else
                    {
                        sTime = time.Minutes + " phút";
                    }
                }
                else
                {
                    sTime = time.Hours + " giờ";
                }
                if (time.Days > 0)
                    sTime = time.Days + " ngày " + sTime;

                item.Message = " " + sTime + " trước";
            }
            return dataResult;
        }

        [HttpOptions, HttpGet]
        [ActionName("GetTop2Jackpot")]
        public DataTable GetTop2Jackpot()
        {
            DataTable result = _dao.GetTop2Jackpot();
            DataColumn column = result.Columns.Add("Message", typeof(string));
            foreach (DataRow item in result.Rows)
            {
                var sTime = string.Empty;

                var time = (DateTime.Now - DateTime.Parse(item["CreatedTime"].ToString()));
                if (time.Hours <= 0)
                {
                    if (time.Minutes <= 0)
                    {
                        sTime = (time.Seconds + 1) + " giây";
                    }
                    else
                    {
                        sTime = time.Minutes + " phút";
                    }
                }
                else
                {
                    sTime = time.Hours + " giờ";
                }
                if (time.Days > 0)
                    sTime = time.Days + " ngày " + sTime;

                item["Message"] += " " + sTime + " trước";

            }
            return result;
        }

        public static string FormatMoney(long inputValue)
        {
            return inputValue.ToString("C0", CultureInfo.CurrentCulture).Replace("$", "").Replace(",", ".");
        }

    }
}
