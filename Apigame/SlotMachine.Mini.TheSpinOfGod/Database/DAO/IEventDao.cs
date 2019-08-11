using System.Collections.Generic;
using System.Data;
using DataAccess.DAOImpl;
using DataAccess.DTO;
using System;

namespace DataAccess.DAO
{
    public interface IEventDao
    {
        SpinData SP_SlotsKingPoker_Spin(InputSpin valueSpin);

        void SP_SlotsKingPoker_GetJackpot(int roomId, int betType, ref long jackpot);
        
        List<HistoryInfor> SP_SlotKingPocker_GetHisrorySpin(long accountId, int topCount, int betType);
        
        List<HistoryInfor> SP_SlotKingPocker_GetNotification(int betType);

        DataTable GetHistory(long accountId, int top);
        DataTable GetSpinDetails(int spinId);
        DataTable GetJackpotHistory(int topCount);
        DataTable GetBigWinData(int topCount);
        string GetSlotsDataTest(string accountName);
        int SetSlotsDataTest(string accountName, string slotData);
    }
}
