using System;

namespace GamePortal.API.Models
{
    public class GameGoldTransaction
    {
        public long ID { get; set; }
        public int GameId { get; set; }
        public DateTime CreatedTime { get; set; }
        public long Amount { get; set; }
        public long Balance { get; set; }
        public int Type { get; set; }
        public string GameName { get; set; }
    }
}