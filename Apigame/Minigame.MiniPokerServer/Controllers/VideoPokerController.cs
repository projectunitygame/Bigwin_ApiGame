using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Minigame.MiniPokerServer.Database.DAO;
using Minigame.MiniPokerServer.Database.DTO;
using Minigame.MiniPokerServer.Database.Factory;
using Utilities.Session;
using Utilities.HttpHelper;
using Utilities.Log;

namespace Minigame.Public.Controllers
{
    public class VideoPokerController : ApiController
    {
        private readonly IPokerDao repository = AbstractDaoMinigame.Instance().CreateMiniPokerDao();

        static readonly string INVALID_DATA_MESSAGE = "Dữ liệu không đúng. Mời bạn thử lại.";
        static readonly string NOT_LOGIN_MESSAGE = "Bạn chưa đăng nhập. Mời thử lại.";
        static readonly string SESSION_NOT_FOUND_MESSAGE = "Không tìm thấy phiên. Mời thử lại.";
        static readonly string ACCOUNT_NOTFOUND_OR_INVALID_PASSWORD_MESSAGE = "Không tồn tại tài khoản {0} hoặc mật khẩu không đúng";//. Mời bạn thử lại.";

        [HttpGet, HttpOptions]
        [ActionName("GetAccountHistory")]
        public HttpResponseMessage GetAccountHistory(int betType, int topCount = 10)
        {
            if (Request.Method == HttpMethod.Options)
            {
                return HttpUtils.CreateResponse(HttpStatusCode.OK, "Accept HttpOptions", "Accept HttpOptions");
            }
            long accountId = AccountSession.AccountID;
            if (accountId < 1 || string.IsNullOrEmpty(AccountSession.AccountName))
            {
                return HttpUtils.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, NOT_LOGIN_MESSAGE, "Log in first");
            }

            try
            {
                if (betType < 1 || betType > 2)
                    return HttpUtils.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters", "Invalid parameters");
                if (topCount > 300)
                {
                    topCount = 300;
                }
                NLogManager.LogMessage("VideoPokerGetHistory:" + AccountSession.AccountID +
                    "|Bettype:" + betType + "topCount:" + topCount);
                var listGetAccountHistory = repository.GetAccountHistory((int)accountId, betType, topCount);
                if (listGetAccountHistory == null)
                {
                    listGetAccountHistory = new List<MiniPokerAccountHistory>();
                }
                return HttpUtils.CreateResponse(HttpStatusCode.OK,
                                                JsonConvert.SerializeObject(listGetAccountHistory),
                                                "GetAccountHistory Success");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return HttpUtils.CreateResponse(HttpStatusCode.NotFound, "NotFound", "GetAccountHistory Failure");
        }


        [HttpGet, HttpOptions]
        [ActionName("GetTopWinners")]
        public HttpResponseMessage GetTopWinners(int betType, int topCount = 10)
        {
            if (Request.Method == HttpMethod.Options)
            {
                return HttpUtils.CreateResponse(HttpStatusCode.OK, "Accept HttpOptions", "Accept HttpOptions");
            }

            if (AccountSession.AccountID < 1 || string.IsNullOrEmpty(AccountSession.AccountName))
            {
                return HttpUtils.CreateResponse(HttpStatusCode.NonAuthoritativeInformation, NOT_LOGIN_MESSAGE, "Log in first");
            }
            try
            {
                if (betType < 1 || betType > 2 || topCount < 1)
                    throw new InvalidOperationException();
                var listTopWinner = repository.GetTopWinners(betType, topCount);

                return HttpUtils.CreateResponse(HttpStatusCode.OK,
                                                JsonConvert.SerializeObject(listTopWinner),
                                                "GetTopWinners Success");
            }
            catch (InvalidOperationException ioe)
            {
                NLogManager.PublishException(ioe);
                return HttpUtils.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters", "Invalid parameters");
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return HttpUtils.CreateResponse(HttpStatusCode.NotFound, "NotFound", "GetTopWinners Failure");
        }
    }
}