using System;
using System.Collections.Generic;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class SlotMachineAccountTransaction
    {
        public DateTime CreatedTime { get; set; }
        public string ServiceName { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
    }

    public class SlotMachineAccountTransactions
    {
        public int AccountID { get; set; }
        public long ResponseStatus { get; set; }
        public List<SlotMachineAccountTransaction> Transactions { get; set; }
    }
}