using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Minigame.MiniPokerServer.Database.DAO;

namespace Minigame.MiniPokerServer.Database.Factory
{
    public abstract class AbstractDaoMinigame
    {

        public static AbstractDaoMinigame Instance()
        {
            try
            {
                return new DaoMinigameFactory();
            }
            catch (Exception)
            {
                throw new Exception("Couldn't create AbstractDAOFactory: ");
            }
        }

        public abstract IPokerDao CreateMiniPokerDao();
    }
}
