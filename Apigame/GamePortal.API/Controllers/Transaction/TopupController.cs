using GamePortal.API.DataAccess;
using GamePortal.API.Models.Topup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Utilities.Log;

namespace GamePortal.API.Controllers.Transaction
{
    public class TopupController : ApiController
    {
        [HttpOptions, HttpGet, Authorize]
        public List<TopupType> Types()
        {
            try
            {
                return TransactionDAO.GetTopupTypes();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }
        [HttpOptions, HttpGet, Authorize]
        public List<CardType> CardTypes()
        {
            try
            {
                List<CardType> types = TransactionDAO.GetCardTypes();
                List<CardTopup> prices = TransactionDAO.GetListCardPrices();
                types.ForEach((i) =>
                {
                    i.Prices = prices.Where(x => x.CardType == i.Type).ToList();
                });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            return null;
        }

    }
}
