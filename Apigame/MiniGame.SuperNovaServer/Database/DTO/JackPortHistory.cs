using System;
using System.Collections.Generic;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class JackPortHistory
    {
        public long RowNum { get; set; }

        public long SpinID { get; set; }

        public int RoomID { get; set; }

        public long BetValue { get; set; }

        public int AccountID { get; set; }

        public string Username { get; set; }

        public int PrizeID { get; set; }

        public long PrizeValue { get; set; }

        public DateTime CreatedTime { get; set; }
    }

    public class ListJackPortHistory
    {
        public List<JackPortHistory> ListJackPort { get; set; }
        public int ToTal { get; set; }
    }
}
