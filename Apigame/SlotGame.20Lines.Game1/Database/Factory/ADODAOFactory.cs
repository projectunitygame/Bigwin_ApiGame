
using SlotGame._20Lines.Game1.Database.DAO;
using SlotGame._20Lines.Game1.Database.DAOImpl;
namespace SlotGame._20Lines.Game1.Database.Factory
{
    public class ADODAOFactory : AbstractDAOFactory
    {

        public override ISlotMachineDAO CreateSlotMachineDAO()
        {
            return new SlotMachineDAOImpl();
        }
    }
}