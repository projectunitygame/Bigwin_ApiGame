using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.DTO;

namespace DataAccess.DAO
{
    public interface IMinigame
    {
        void SP_TET2015_E1_SetBet(int _AccountID, string _Username, out int _LuckyTurn, out string _LotteryNumber, out int _ResponseStatus);
        
        /// <summary>
        /// Lịch sử gieo quẻ
        /// </summary>
        /// <param name="_AccountID"></param>
        /// <param name="_CurrentPage"></param>
        /// <param name="_PageSize"></param>
        /// <param name="_TotalRecord"></param>
        /// <returns></returns>
        List<HistoryGieoQue> SP_TET2015_E1_GetAccountHistory(int _AccountID, int _CurrentPage, int _PageSize, out int _TotalRecord);
        
        /// <summary>
        /// Lấy phiên nấu bánh chưng
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_GameStatus"></param>
        /// <param name="_RemainWaiting"></param>
        /// <param name="_RemainRacing"></param>
        void SP_TET2015_E2_GetCurrentData(out int _GameSessionID, out int _GameStatus, out int _RemainWaiting, out int _RemainRacing);

        /// <summary>
        /// Danh sách phòng nấu bánh chưng
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_GameStatus"></param>
        /// <returns></returns>
        List<RoomNauBanh> SP_TET2015_E2_GetRoomInfo(int _GameSessionID, int _RoomID, out int _GameStatus);
        
        /// <summary>
        /// Săn thẻ nấu bánh chưng
        /// </summary>
        /// <param name="_AccountID"></param>
        /// <param name="_Username"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_CardID"></param>
        /// <param name="_CardValue"></param>
        /// <param name="_ResponseStatus"></param>
        void SP_TET2015_E2_GetCard(int _AccountID, string _Username, int _RoomID, out int _CardID, out int _CardValue, out int _ResponseStatus);
        
        /// <summary>
        /// Share nhận thưởng nấu bánh chưng
        /// Vinh.vu - 27.01.2015
        /// </summary>
        /// <param name="_GameSessionID"></param>
        /// <param name="_RoomID"></param>
        /// <param name="_AccountID"></param>
        /// <param name="_ResponseStatus"></param>
        void SP_TET2015_E2_Finish(int _GameSessionID, int _RoomID, int _AccountID, out int _ResponseStatus);

        /// <summary>
        /// Set dữ liệu Test
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="slotData"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        int SetSlotData(int accountId, string slotData, int betType);

        /// <summary>
        /// Lấy dữ liệu test
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="betType"></param>
        /// <returns></returns>
        string GetSlotData(int accountId, int betType);
    }
}
