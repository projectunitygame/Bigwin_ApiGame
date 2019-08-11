
using Intecom.Software.RDTech.SlotMachine.DataAccess.DAO;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DAOImpl;
namespace Intecom.Software.RDTech.SlotMachine.DataAccess.Factory
{
    public class ADODAOFactory : AbstractDAOFactory
    {

        public override ISlotMachineDAO CreateSlotMachineDAO()
        {
            return (ISlotMachineDAO)new SlotMachineDAOImpl();
        }

    }
}