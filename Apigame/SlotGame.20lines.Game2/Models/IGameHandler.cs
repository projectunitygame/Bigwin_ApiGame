using SlotGame._20lines.Game2.Models;
using SlotGame._20lines.Game2.Database.DTO;

namespace SlotGame._20lines.Game2.Models
{
    public interface IGameHandler
    {


        AccountInfo PlayNow(MoneyType moneyType, int roomId, long accountId, string accountName);

        SpinData Spin(long accountId, string accountName, string lines, MoneyType montype, int roomId);

        long FinishBonusGame(MoneyType moneyType, int spinId, ref long prizeValue, ref long balance);
    }
}