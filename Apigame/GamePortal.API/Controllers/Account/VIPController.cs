using GamePortal.API.DataAccess;
using GamePortal.API.FinancialService;
using GamePortal.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Utilities.Log;
using Utilities.Session;

namespace GamePortal.API.Controllers.Account
{
    public class VIPController : ApiController
    {
        [Authorize, HttpOptions, HttpGet]
        public async Task<dynamic> GetVIP(bool getAllInfo = false)
        {
            try
            {
                var accountId = AccountSession.AccountID;
                var vipRankConfig = AccountDAO.GetVIPRankConfig();
                
                #region Tinh VP
                int vp = 0;
                using (var financial = new FinancialSoapClient())
                {
                    vp = await financial.GetAccountVIPPointAsync(accountId);
                }
                #endregion

                var data = AccountDAO.CheckVIP(accountId, vp, out int rank); // list phan thuong da nhan                
                if (getAllInfo)
                {
                    var vipReward = from config in vipRankConfig
                                    join item in data.DefaultIfEmpty() on config.Rank equals item?.Rank into results

                                    from result in results.DefaultIfEmpty()
                                    select new
                                    {
                                        config.Rank,
                                        config.Reward,
                                        Status = result?.Status ?? false,
                                    };
                    return new
                    {
                        CurrentRank = rank,
                        CurrentVP = vp,
                        RewardList = vipReward
                    };
                }
                else
                {
                    var nextVP = 0;
                    if (vipRankConfig != null)
                        nextVP = vipRankConfig.FirstOrDefault(x => (x.Rank == (rank + 1))).VP;
                    return new
                    {
                        CurrentRank = rank,
                        CurrentVP = vp,
                        NextVP = nextVP
                    };
                }

            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return null;
        }

        [Authorize, HttpOptions, HttpGet]
        public dynamic ReceiveReward(int rank)
        {
            try
            {
                var accountId = AccountSession.AccountID;
                return AccountDAO.ReceiveReward(rank, accountId);
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }
    }
}