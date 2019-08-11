using System;
using System.Collections.Generic;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using Newtonsoft.Json;

namespace Studio.WebGame.SupperNova.Models
{
    public class AbstractGamePlayer
    {
        public AbstractGamePlayer(Account account)
        {
            Account = account;
            PlayerStatus = (int)Enums.PlayerStatus.Player;
            ConnectionStatus = (int)Enums.ConnectionStatus.CONNECTED;
            RegisterLeaveRoom = false;
            LastActiveTime = DateTime.Now;
        }

        public Account Account { get; set; }

        public long AccountID { get; set; }

        public long SessionID { get; private set; }

        public int PlayerStatus { get; set; }

        [JsonIgnore]
        public List<long> DislikeRooms { get; set; }

        public bool RegisterLeaveRoom { get; set; }

        public string RemoteIP { get; set; }

        public DateTime LastActiveTime { get; set; }

        public int ConnectionStatus { get; private set; }

        public bool IsEnoughMoney(byte moneyType, long threshold)
        {
            switch (moneyType)
            {
                case 0:
                    return Account.Coin >= threshold;
                case 1:
                    return Account.TotalStar >= threshold;
                default:
                    return false;
            }
        }

        public void ChangeConnectionStatus(int status)
        {
            ConnectionStatus = status;
        }

        public void SetSessionID(long sessionId)
        {
            SessionID = sessionId;
        }

        public void ResetBeforeQuitRoom()
        {
            ClearData();
            SetSessionID(-1);
            PlayerStatus = (int)Enums.PlayerStatus.None;
        }

        public virtual void ClearData()
        {
            RegisterLeaveRoom = false;
        }

        public void SetActive()
        {
            LastActiveTime = DateTime.Now;
            ConnectionStatus = (int)Enums.ConnectionStatus.CONNECTED;
        }
    }
}