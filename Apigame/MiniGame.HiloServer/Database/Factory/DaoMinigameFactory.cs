using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigames.DataAccess.DAO;
using Minigames.DataAccess.DAOImpl;

namespace Minigames.DataAccess.Factory
{
    public class DaoMinigameFactory : AbstractDaoMinigame
    {
        public override IHiloDao CreateMiniHiloDao()
        {
            return new HiloDaoImpl();
        }
    }
}
