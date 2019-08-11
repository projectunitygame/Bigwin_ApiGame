using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Studio.WebGame.SupperNova.Controllers;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;

namespace Studio.WebGame.SupperNova.Models
{
    public class GamePlayer : AbstractGamePlayer, IDisposable
    {
        private readonly int DEFAULT_BETVALUE = Int32.Parse(ConfigurationManager.AppSettings["DEFAULT_BETVALUE"]);
        private readonly int DEFAULT_BETTYPE = Int32.Parse(ConfigurationManager.AppSettings["DEFAULT_BETTYPE"]);
        private readonly int DEFAULT_ROOMID = Int32.Parse(ConfigurationManager.AppSettings["DEFAULT_ROOMID"]);

        #region Attribute
        /// <summary>
        /// Mã Phòng: 1: 100; 2: 1.000; 3: 10.000
        /// </summary>
        public int RoomId { get; set; }

        /// <summary>
        /// Loại Phòng: 1: Sao; 2: Xu
        /// </summary>
        public int BetType { get; set; }

        /// <summary>
        /// IsPlayTry = true: Quay thử
        /// </summary>
        public bool IsPlayTry { get; set; }

        /// <summary>
        /// JackPort hiện tại theo RoomId
        /// </summary>
        public long CurrentJackPort { get; set; }

        /// <summary>
        /// Mệnh giá phòng: 100, 1.000, 10.000 .....
        /// </summary>
        public int BetValue { get; set; }

        /// <summary>
        /// Trạng thái tự động quay: 1: Tự quay
        /// </summary>
        public bool AutoSpin { get; set; }

        /// <summary>
        /// Trạng thái game: 1: Spin; 3: Bonus
        /// </summary>
        public int GameStatus { get; set; }

        /// <summary>
        /// Thông tin chi tiết lượt quay
        /// </summary>
        public SlotMachineSpinData SpinData { get; set; }

        /// <summary>
        /// Số dư tài khoản
        /// </summary>
        public long Balance { get; set; }

        /// <summary>
        /// Thời gian quay lần cuối
        /// </summary>
        public DateTime LastPlaySpin { get; set; }
        #endregion

        public bool PlayNow(int betType, int roomId)
        {
            BetType = betType;
            RoomId = roomId;
            return true;
        }

        public void SetAutoSpin(bool autoSpin, string lines)
        {
            AutoSpin = autoSpin;
        }

        public GamePlayer(Account account)
            : base(account)
        {
            BetValue = DEFAULT_BETVALUE;
            BetType = DEFAULT_BETTYPE;
            RoomId = DEFAULT_ROOMID;
            AutoSpin = false;
            IsPlayTry = false;
            
        }

        public void Clear()
        {
            SetSessionID(0);
            AutoSpin = false;
            BetValue = 0;

            SpinData = new SlotMachineSpinData();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}