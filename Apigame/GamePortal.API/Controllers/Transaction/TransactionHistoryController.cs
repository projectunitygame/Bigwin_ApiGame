using GamePortal.API.DataAccess;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Transaction
{
    public class TransactionHistoryController : ApiController
    {
        [HttpOptions, HttpGet, Authorize]
        public List<PlayLog> GetPlayLog()
        {
            try
            {
                var accountId = AccountSession.AccountID;


                var trans = TransactionDAO.GetGameGoldTransaction_v1(accountId, 200);
                return trans.Select(x => new PlayLog()
                {
                    Amount = x.Amount,
                    Balance = x.Balance,
                    CreatedTime = x.CreatedTime,
                    ID = x.ID,
                    GameName = x.GameName,
                    Type = x.Type
                }).ToList();



                //var games = GameDAO.GameList(accountId);

                //var linq = (from item in trans
                //           join game in games on item.GameId equals game.ID
                //           into plays

                //           from play in plays.DefaultIfEmpty()
                //           select new
                //           {
                //               ID = item.ID,
                //               GameName = play.Name,
                //               CreatedTime = item.CreatedTime,
                //               Amount = item.Amount,
                //               Balance = item.Balance,
                //               Type = item.Type
                //           }).Select(x => new PlayLog {
                //               ID = x.ID,
                //               GameName = x.GameName,
                //               CreatedTime = x.CreatedTime,
                //               Amount = x.Amount,
                //               Balance = x.Balance,
                //               Type = x.Type
                //           });

                //return linq.ToList();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                NLogManager.LogError("ERROR GetPlayLog: " + ex);
            }

            return null;
        }

        [HttpOptions, HttpGet, Authorize]
        public List<TopupGold> GetTopupLog()
        {
            try
            {
                var accountId = AccountSession.AccountID;
                return TransactionDAO.GetTopupGold(accountId, 200);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [HttpOptions, HttpGet, Authorize]
        public List<DeductGold> GetDeductLog()
        {
            try
            {
                var accountId = AccountSession.AccountID;
                return TransactionDAO.GetDeductGold(accountId, 200);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [HttpOptions, HttpGet, Authorize]
        public List<TransferLog> GetTransferLog()
        {
            try
            {
                var accountId = AccountSession.AccountID;
                return TransactionDAO.GetTransferLog(accountId);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }
    }
}
