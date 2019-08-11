using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Minigame.MiniPokerServer.Database.DAO;
using Minigame.MiniPokerServer.Database.DAOImpl;

namespace Minigame.MiniPokerServer.Database.Factory
{
    public class DaoMinigameFactory : AbstractDaoMinigame
    {

        public override IPokerDao CreateMiniPokerDao()
        {
            return new PokerDaoImpl();
        }
    }
}
