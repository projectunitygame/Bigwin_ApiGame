using System;
using System.IO;
using System.Configuration;
using System.Web;
using System.Web.Http;
using DataAccess.Factory;

namespace MinigameVuabai.SignalR.Controllers
{
    public class TestApiController : ApiController
    {
        // GET api/<controller>
        private static readonly string AccountTestFile = ConfigurationManager.AppSettings["AccountTestFile"];
        private static readonly string DataTestFile = ConfigurationManager.AppSettings["DataTestFile"];
        private static readonly bool IsDataTest = !string.IsNullOrEmpty(DataTestFile);

        [HttpOptions, HttpPost]
        [ActionName("createSampleData")]
        public int CreateSampleData([FromBody]
                                         dynamic data)
        {
            if (!IsDataTest)
                return -99;

            var accountId = 0;
            var accountName = string.Empty;
            try
            {
                string accountInfo = HttpContext.Current.User.Identity.Name;
                if (accountInfo.Split('|').Length >= 4)
                {
                    accountId = int.Parse(accountInfo.Split('|')[0]);
                    accountName = string.Format(accountInfo.Split('|')[1]);
                    NLogLogger.LogInfo("acc: {0} - access: {1}");
                }

                if (accountId <= 0)
                {
                    return -2;
                }

                string l = string.Empty;
                string accountTest = string.Empty;

                var accountFile = new StreamReader(HttpContext.Current.Server.MapPath(AccountTestFile));
                while ((l = accountFile.ReadLine()) != null)
                {
                    accountTest += l + ",";
                }
                accountFile.Close();

                //chi nhung tai khoan test moi duoc su dung data test
                if (accountTest.IndexOf("," + accountName + ",", StringComparison.Ordinal) < 0)
                    return -1;

                string cards = data.card;
                int betType = data.betType;
                var result = AbstractDaoFactory.Instance().CreateMiniGame().SetSlotData(accountId, cards, betType);
                return result;

            }
            catch (Exception ex)
            {
                NLogLogger.PublishException(ex);
                return -99;
            }
        }

        [HttpGet]
        [ActionName("getSampleData")]
        public string GetSampleData(int betType)
        {
            if (!IsDataTest)
                return "Tính năng đã disabled";
            var accountId = 0;
            var accountName = string.Empty;
            try
            {
                string accountInfo = HttpContext.Current.User.Identity.Name;
                if (accountInfo.Split('|').Length >= 4)
                {
                    accountId = int.Parse(accountInfo.Split('|')[0]);
                    accountName = string.Format(accountInfo.Split('|')[1]);
                    NLogLogger.LogInfo("acc: {0} - access: {1}");
                }

                if (accountId <= 0)
                {
                    return string.Empty;
                }


                string l = string.Empty;
                string accountTest = string.Empty;

                var accountFile = new StreamReader(HttpContext.Current.Server.MapPath(AccountTestFile));
                while ((l = accountFile.ReadLine()) != null)
                {
                    accountTest += l + ",";
                }
                accountFile.Close();

                //chi nhung tai khoan test moi duoc su dung data test
                if (accountTest.IndexOf("," + accountName + ",", StringComparison.Ordinal) < 0)
                    return "Tài khoản của bạn có quyền truy cập";
                string dataTest = string.Empty;
                dataTest = AbstractDaoFactory.Instance().CreateMiniGame().GetSlotData(accountId, betType);
                //neu du lieu trong file test khong co
                if (dataTest.Length <= 0)
                    return "";

                return dataTest;
            }
            catch (Exception ex)
            {
                NLogLogger.PublishException(ex);
                return "Lỗi:" + ex.Message;
            }
        }
    }
}
