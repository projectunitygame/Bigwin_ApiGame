using SlotGame._20lines.Game2.Database.DAO;
using SlotGame._20lines.Game2.Database.DAOImpl;

namespace SlotGame._20lines.Game2.Database.Factory
{
    public class Factory : AbstractFactory
    {
        public override ITreaSureIslandDao CreateTreaSureIslandDao()
        {
            return new TreaSureIslandDaoImpl();
        }
    }
}
