using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class NotificationsData
    {
        public long SpinId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Message { get; set; }        
    }
}
