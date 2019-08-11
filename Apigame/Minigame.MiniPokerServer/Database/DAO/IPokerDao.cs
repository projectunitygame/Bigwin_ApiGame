using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigame.MiniPokerServer.Database.DTO;

namespace Minigame.MiniPokerServer.Database.DAO
{
    public interface IPokerDao
    {
        MiniPokerSpinResponse Spin(
            long accountId, string accountName, int betType, int roomId,
            string ip, int sourceId, int merchantId, int mobilePl, bool isbot = false, bool nohu = false);

        long GetJackpot(int betType, int roomId);
        List<MiniPokerTopWinnerModel> GetTopWinners(int betType, int topCount);
        List<MiniPokerAccountHistory> GetAccountHistory(int accountId, int betType, int topCount);
        List<MiniPokerAccountHistoryDetail> GetAccountHistoryDetails(int accountId, int betType, long spinId);

        int SetTestData(string accountName, int cardType);

        int GetTestData(string accountName);
    }
}
