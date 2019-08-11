using Cardgame.DiskShaking.Database;
using Cardgame.DiskShaking.Models.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public class Player
    {
        public long AccountId { get; private set; }
        public string AccountName { get; private set; }
        public long Gold { get; private set; }
        public long Coin { get; private set; }
        public int AvatarID { get; private set; }
        [JsonIgnore]
        public long SessionId { get; set; }
        [JsonIgnore]
        public long RoomId { get; set; }
        [JsonIgnore]
        public bool AllowQuit { get; set; }
        private DateTime _lastActiveTime;

        public Player(long accountId, string accountName, long gold, long coin, int avatarId)
        {
            AccountId = accountId;
            AccountName = accountName;
            AvatarID = avatarId;
            Gold = gold;
            Coin = coin;
            SessionId = -1;
            RoomId = -1;
            SetActive();
        }

        public void LeaveGame()
        {
            SessionId = -1;
            RoomId = -1;
        }

        public void SetActive()
        {
            _lastActiveTime = DateTime.Now;
        }

        public bool IsTimeout()
        {
            return DateTime.Now.Subtract(_lastActiveTime).TotalSeconds >= 30;
        }

        public void UpdateBalance(long balance, MoneyType moneyType)
        {
            if (moneyType == MoneyType.GOLD)
                Gold = balance;
            else Coin = balance;
        }

        public void IncreaseBalance(long change, MoneyType moneyType)
        {
            if (moneyType == MoneyType.GOLD)
                Gold += change;
            else Coin += change;
        }
    }
}