using Game.Events.Database.DAO;
using Game.Events.Database.DAOImpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Game.Events.Database.Factory
{
    public class ADODAOFactory :AbstractDAOFactory
    {
        public override IJackpot CreateJackpotDAO()
        {
            return new JackpotImpl();
        }

        public override IBigJackpotEvent CreateTamQuocBigJackpotDAO()
        {
            return new TamQuocBigJackpotImpl();
        }

        public override IBigJackpotEvent CreateVoLamBigJackpotDAO()
        {
            return new VoLamBigJackpotImpl();
        }

        public override IBigJackpotEvent CreateSlotGodBigJackpotDAO()
        {
            return new SlotGodBigJackpotImpl();
        }

        public override IBigJackpotEvent CreateSuperNovaBigJackpotDAO()
        {
            return new SuperNovaBigJackpotImpl();
        }

        public override IBigJackpotEvent CreateMiniPokerBigJackpotDAO()
        {
            return new MiniPokerBigJackpotImpl();
        }

        public override IBigJackpotEvent CreateGame25LinesBigJackpotDAO()
        {
            return new Game25LinesBigJackpotImpl();
        }


        public override IGate CreateGateDAO()
        {
            return new GateImpl();
        }
    }
}