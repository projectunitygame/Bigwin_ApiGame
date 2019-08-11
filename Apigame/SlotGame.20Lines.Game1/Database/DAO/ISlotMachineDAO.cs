using SlotGame._20Lines.Game1.Database.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SlotGame._20Lines.Game1.Models;


namespace SlotGame._20Lines.Game1.Database.DAO
{
    public interface ISlotMachineDAO
    {
        AccountInfo GetAccountInfo(int accountID, string username, int roomId, MoneyType moneyType);

        IEnumerable<Jackpot> GetJackpot(MoneyType moneyType);

        SpinData Spin(int accountID, string username,
            string linesData, int roomId, string clientIP, MoneyType moneyType);

        long FinishBonusGame(MoneyType moneyType, long spinId, ref long totalPrizeValue, ref long balance);

        DataTable GetHistory(MoneyType moneyType, long accountId, int top);

        List<SpinDetail> GetSpinDetail(MoneyType type, long spinId, out string lineData);
        JackpotHistoryList GetJackpotHistory(MoneyType type, int currentPage, int pageSize);

        int SetTestData(string accountName, string slotsData);

        string GetTestData(string accountName);

        DataTable GetTop2Jackpot();

    }
}
