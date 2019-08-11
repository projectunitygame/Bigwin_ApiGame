using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using MiniGame.SuperNovaServer.Models;
using Studio.WebGame.SupperNova.Models;
using Enums = Intecom.Software.RDTech.SlotMachine.DataAccess.DTO.Enums;

namespace Studio.WebGame.SupperNova.Controllers
{
    public abstract class AbstractGameHandler
    {
        /// <summary>
        /// Get số dư theo Loại tiền chơi 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected long GetAccountBalance(GamePlayer player)
        {
            long balance = 0;
            if (player.BetType == (int)Enums.BetType.STAR)
                balance = player.Account.TotalStar;

            else if (player.BetType == (int)Enums.BetType.COIN)
                balance = player.Account.Coin;
            return balance;
        }

        /// <summary>
        /// Cập nhật Số dư
        /// Cập nhật Số dư
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Balance"></param>
        protected void UpdateAccountBalance(GamePlayer player, long Balance)
        {
            if (player.BetType == (int)Enums.BetType.STAR && Balance != player.Account.TotalStar)
                player.Account.TotalStar = Balance;
            else if (player.BetType == (int)Enums.BetType.COIN && Balance != player.Account.Coin)
                player.Account.Coin = Balance;
        }

        #region Join Game
        public abstract void PlayNow(GamePlayer player);

        #endregion Join Game

        #region Do Spin
        public abstract SlotMachineSpinData PlaySpin(int roomId, MoneyType moneyType);

        #endregion End Do Spin
    }
}