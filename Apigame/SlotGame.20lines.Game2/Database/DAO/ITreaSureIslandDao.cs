using System.Collections.Generic;
using System.Data;
using SlotGame._20lines.Game2.Database.DTO;
using SlotGame._20lines.Game2.Models;
using SlotGame._20Lines.Game1.Database.DTO;

namespace SlotGame._20lines.Game2.Database.DAO
{
    public interface ITreaSureIslandDao
    {
        AccountInfo GetAccountInfo(int accountID, string username, int roomId, MoneyType moneyType);

        IEnumerable<Jackpot> GetJackpot(MoneyType moneyType);

        SpinData Spin(int accountID, string username,
            string linesData, int roomId, string clientIP, MoneyType moneyType);

        long FinisBonusGame(MoneyType moneyType, long spinId, ref long totalPrizeValue, ref long balance);

        DataTable GetHistory(MoneyType moneyType, long accountId, int top);

        List<SpinDetail> GetSpinDetail(MoneyType type, long spinId, out string lineData);
        JackpotHistoryList GetJackpotHistory(MoneyType type, int currentPage, int pageSize);

        int SetTestData(string accountName, string slotsData);

        string GetTestData(string accountName);

        DataTable GetTop2Jackpot();
    }
}
