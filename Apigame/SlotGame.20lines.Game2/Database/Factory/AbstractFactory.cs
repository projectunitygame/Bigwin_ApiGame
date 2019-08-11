
using System;
using SlotGame._20lines.Game2.Database.DAO;
using Utilities.Log;

namespace SlotGame._20lines.Game2.Database.Factory
{
    public abstract class AbstractFactory
    {
        public static AbstractFactory Instance()
        {
            try
            {
                return new Factory();
            }
            catch (Exception e)
            {
                NLogManager.PublishException(e);
                throw new Exception("Countn't Created AbstractFactory");
            }
        }

        public abstract ITreaSureIslandDao CreateTreaSureIslandDao();

    }
}
