

using SlotGame._20Lines.Game1.Database.DTO;
using SlotMachine.TheThreeKingdoms.Models;

namespace SlotGame._20Lines.Game1.Models
{
    public interface IGameHandler
    {
        #region Join Game

        AccountInfo PlayNow(MoneyType moneyType, int roomId, long accountId, string accountName);

        SpinData Spin(long accountId, string accountName, string lines, MoneyType montype, int roomId);

        long FinishBonusGame(MoneyType moneyType, int spinId, ref long prizeValue, ref long balance);

        #endregion Bonus Game
    }
}