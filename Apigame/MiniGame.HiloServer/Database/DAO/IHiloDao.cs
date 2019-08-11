using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigames.DataAccess.DTO;

namespace Minigames.DataAccess.DAO
{
    public interface IHiloDao
    {
        HiLoGetAccountInfoResponse GetAccountInfo(int accountId, string username);
        HiLoSetBetResponse SetBet(int accountId, string username, int roomId, int betType, int stepType, int locationId,
            string clientIp, int sourceId, int merchantId);
        long GetJackpot(int betType, int roomId);
        List<HiLoGetTopAccount> GetTopAccount(int betType, int topCount);
        List<HiLoGetAccountHistory> GetAccountHistory(int accountId, int betType, int topCount);
    }
}
