using Game.Events.Database.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.Factory
{
    public abstract class AbstractDAOFactory
    {
        public static AbstractDAOFactory Instance()
        {
            try
            {
                return (AbstractDAOFactory)new ADODAOFactory();
            }
            catch(Exception)
            {
                throw new Exception("Couldn't create AbstractDAOFactory");
            }
        }

        public abstract IJackpot CreateJackpotDAO();
        public abstract IBigJackpotEvent CreateTamQuocBigJackpotDAO();
        public abstract IBigJackpotEvent CreateVoLamBigJackpotDAO();
        public abstract IBigJackpotEvent CreateSlotGodBigJackpotDAO();

        public abstract IBigJackpotEvent CreateSuperNovaBigJackpotDAO();

        public abstract IBigJackpotEvent CreateMiniPokerBigJackpotDAO();

        public abstract IBigJackpotEvent CreateGame25LinesBigJackpotDAO();

        public abstract IGate CreateGateDAO();
    }
}