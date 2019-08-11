using System;
using System.Collections.Generic;
using System.Web.Http;
using SlotGame._20lines.Game2.Models;
using SlotGame._20lines.Game2.Database.DTO;
using SlotGame._20lines.Game2.Database.Factory;
using System.Data;
using System.Globalization;
using Utilities.Log;
using Utilities.Session;

namespace SlotGame._20lines.Game2.Controllers
{
    public class HistoryController : ApiController
    {

        [HttpOptions, HttpGet]
        [ActionName("GetHistory")]
        public DataTable GetHistory(MoneyType moneyType)
        {
            long accountId = AccountSession.AccountID;
            if (accountId < 1)
                return null;
            return GameLogHandler.Instance.GetHistory(moneyType, accountId, 100);
        }

        /// <summary>
        /// API lay danh sach jackpot
        /// </summary>
        /// <returns></returns>
        [HttpOptions, HttpGet]
        [ActionName("GetHistoryJackPot")]
        public JackpotHistoryList GetHistoryJackPot(MoneyType moneyType, int currentPage, int pageSize)
        {
            return GameLogHandler.Instance.GetJackpotHistory(moneyType, currentPage, pageSize);
        }

        [HttpOptions, HttpGet]
        [ActionName("GetSpinDetail")]
        public SpinDetailList GetSpinDetail(MoneyType moneyType, long spinId)
        {
            var dataResult = new SpinDetailList();
            var lineData = string.Empty;
            dataResult.DetailSpin = GameLogHandler.Instance.GetSpinDetail(moneyType, spinId, out lineData);
            dataResult.LineData = lineData;
            return dataResult;
        }

        [HttpOptions, HttpGet]
        [ActionName("GetSystemNotify")]
        public List<SystemNotify> GetSystemNotify()
        {
            var dataResult = GameLogHandler.Instance.NotifyList;
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
            DataTable result = GameLogHandler.Instance.GetTop2Jackpot();
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
