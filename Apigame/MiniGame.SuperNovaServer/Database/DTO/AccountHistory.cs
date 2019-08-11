using System.Collections.Generic;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class AccountHistory : Model
    {
        public long SpinID { get; set; }
        public int RoomID { get; set; }
        public long AccountID { get; set; }
        public string SlotsData { get; set; }
    }

}