using Intecom.Software.RDTech.SlotMachine.DataAccess.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intecom.Software.RDTech.SlotMachine.DataAccess.Factory
{
    public abstract class AbstractDAOFactory
  {
      public static AbstractDAOFactory Instance()
      {
          try
          {
              return (AbstractDAOFactory)new ADODAOFactory();
          }
          catch (Exception)
          {
              throw new Exception("Couldn't create AbstractDAOFactory: ");
          }
      }
    
      public abstract ISlotMachineDAO CreateSlotMachineDAO();
    }
}
