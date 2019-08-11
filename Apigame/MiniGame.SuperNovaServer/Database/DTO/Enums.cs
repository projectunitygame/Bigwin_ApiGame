namespace Intecom.Software.RDTech.SlotMachine.DataAccess.DTO
{
    public class Enums
    {
        public enum ConnectionStatus
        {
            DISCONNECTED = 0,
            CONNECTED = 1,
            REGISTER_LEAVE_GAME = 2
        }

        public enum PlayerStatus
        {
            None = -0,
            Player = 1,
            TryPlayer = 0
        }

        public enum BetType
        {
            STAR = 1,
            COIN = 2
        }

        public enum GameStatus
        {
            SPIN = 1,
            BONUS = 3,
            FREESPIN = 5
        }

        public enum PrizeId
        {
            X8000 = 1,       // 5 Hòm
            BONUS_X1 = 2,       // 3 Hòm
            BONUS_X5 = 3,       // 4 Hòm

            Jackpot = 4,       // 5 Người
            X30 = 5,       // 4 người
            X5 = 6,       // 3 người

            X500 = 7,       // 5 Ngựa
            X20 = 8,       // 4 Ngựa
            X4 = 9,       // 3 Ngựa

            X200 = 10,      // 5 Nhẫn
            X15 = 11,      // 4 Nhẫn
            X3 = 12,      // 3 Nhẫn

            X75 = 13,      // 5 Áo
            X10 = 14,      // 4 Áo
            X2 = 15,      // 3 Áo

            X30_VK = 16,      // 5 Vũ khí
            X6 = 17,      // 4 Vũ kí

            F15 = 131,     // 5 Đồ ăn
            F5 = 132,     // 4 Đồ ăn
            F1 = 133      // 3 Đồ ăn
        }

        public enum BonusId
        {
            Key = 210,
            Gold = 220,
            GoldJar_10 = 201,
            GoldJar_15 = 202,
            GoldJar_20 = 203
        }

        public class GameTypeText
        {
            public static readonly string SPIN = "";
            public static readonly string PAYLINE = "Payline";
            public static readonly string JACKPOT = "Nổ quỹ";
            public static readonly string BONUS = "Bonus";
            public static readonly string FREESPIN = "FreeSpin";
        }

        public enum ErrorCode
        {
            Success = 1,
            Exception = -99,
            AccountNotExists = -51, //Account không đủ tiền
            LinesError = -232, //Số line chọn không hợp lệ
            NotAuthen = -1001,
            NotPlayer = -1002,
            DuplicateSpin = -1003
        }
    }
}