using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlotGame._25Lines.Database.DTO;

namespace SlotGame._25Lines.Database.DAO
{
    public interface ISlotMachine25Lines
    {
        AccountInfo GetAccountInfo(long accountId, string accountName, int roomId, MoneyType moneyType, out int response);

        OutputSpinData Spin(InputSpinData inputData);

        int UpdateSlotsData(int spinId, string slotsData);

        void CreateBonusGame(MoneyType money, int spinId, int roomId, long accountId, string accountName,
            int totalBetValue, string bonusGameData, int multi, int totalPrizeValue, out int response);

        void FinishBonusGame(MoneyType money, int spinId, out int totalPrizeValue, out long balance, out int response);

        IEnumerable<Jackpot> GetJackpot(MoneyType moneyType);      
        
        // trò chơi x2
        LuckyGame PlayLuckyGame(MoneyType money, long accountId, string accountName, int roomId, int spinId, X2Game step, int result);

       

        // Game History
        DataTable GetHistory(MoneyType moneyType, long accountId, int topCount);

        JackpotHistoryList GetJackpotHistory(MoneyType moneyType, int currentPage, int pageSize);

        DataTable GetTop2Jackpot();
    }
}
