using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlotGame._25Lines.Database.DTO;

namespace SlotGame._25Lines.Models
{
    public interface IChecker
    {
        bool CheckLines(string lines);
        bool CheckRoom(int roomId);
        bool CheckMoneyType(MoneyType moneyType);
  
    }

    public class Checker : IChecker
    {

        public bool CheckLines(string lines)
        {
            try
            {
                var arr = lines.Split(',').ToList();
                //return arr.TrueForAll(x => SlotManager.Instance.Lines.Any(y => y.LineId == int.Parse(x)));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckRoom(int roomId)
        {
            return roomId >= 1 && roomId <= 4;
        }

        public bool CheckMoneyType(MoneyType moneyType)
        {
            return moneyType == MoneyType.Gold || moneyType == MoneyType.Coin;
        }
    }
}
