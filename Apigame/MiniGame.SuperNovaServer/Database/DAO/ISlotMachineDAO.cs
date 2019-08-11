using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using System.Collections.Generic;
using MiniGame.SuperNovaServer.Database.DTO;
using MiniGame.SuperNovaServer.Models;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DAO
{
    public interface ISlotMachineDAO
    {
        SlotMachineAccountInfo GetAccountInfo(int inputType, int accountID, string username, int roomTypeID);

        SlotMachineSpinData Spin(MoneyType betType, int gameId,int sourceId, string accessToken, int accountId, string username,
            string linesData, int roomTypeId, string clientIp);


        List<AccountHistory> GetTransactionHistory(long accountid, MoneyType moneyType);

        List<HonorHistory> GetHonorHistory(MoneyType moneyType);

        List<HonorHistory> GetJackpotHistory(MoneyType moneyType);

        BigJackpotCount GetBigJackpotCount(int roomId);
        ListJackPortHistory GetBigJackpotHistory(int page, int pageSize);

        int InsertSample(string username, string slotsdata);

        IEnumerable<Jackpot> GetJackpot(MoneyType moneyType);
    }
}