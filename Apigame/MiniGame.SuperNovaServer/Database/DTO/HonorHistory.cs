namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class HonorHistory : Model
    {
        public long SpinID { get; set; }
        public int RoomID { get; set; }
        public int RoomValue { get; set; }
        public int Type { get; set; }
    }
}