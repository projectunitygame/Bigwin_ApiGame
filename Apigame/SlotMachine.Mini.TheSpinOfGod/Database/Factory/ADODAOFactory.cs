using DataAccess.DAO;
using DataAccess.DAOImpl;

namespace DataAccess.Factory
{
    public class AdodaoFactory : AbstractDaoFactory
    {

        public override IEventDao CreateEventDao()
        {
            return new EventDaoImpl();
        }
        public override IMinigame CreateMiniGame()
        {
            return new MiniGameImpl();
        }
    }
}
