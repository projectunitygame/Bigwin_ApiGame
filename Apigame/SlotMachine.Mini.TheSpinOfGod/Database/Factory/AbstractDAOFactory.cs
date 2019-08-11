using System;
using DataAccess.DAO;

namespace DataAccess.Factory
{
    public abstract class AbstractDaoFactory
    {
        public static AbstractDaoFactory Instance()
        {
            try
            {
                return new AdodaoFactory();
            }
            catch (Exception)
            {
                throw new Exception("Couldn't create AbstractDAOFactory: ");
            }
        }

        public abstract IEventDao CreateEventDao();
        public abstract IMinigame CreateMiniGame();
  
    }
}
