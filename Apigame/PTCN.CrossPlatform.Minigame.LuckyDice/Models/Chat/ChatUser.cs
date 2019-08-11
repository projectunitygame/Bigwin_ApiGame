using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat
{
    public class ChatUser
    {
        public long AccountId { get; set; }
        public string AccountName { get; set; }
        public long TotalRecentBetting { get; set; }
        public long LastActiveTime { get; set; }
        public long LastSpamTime { get; set; }
        public bool Locked { get; set; }
        private List<string> _lastMessage = new List<string>();
        private int _spamCount { get; set; }
        private object _lockSpam = new object();
        private long _lastMessageTime;

        public bool DetectSpam(string msg)
        {
            if(Monitor.TryEnter(_lockSpam, 5000))
            {
                try
                {
                    if (TimeSpan.FromTicks(DateTime.Now.Ticks - _lastMessageTime).TotalMinutes >= 1)
                        _lastMessage.Clear();
                    _lastMessageTime = DateTime.Now.Ticks;
                    _lastMessage.Add(msg);
                    if (_lastMessage.GroupBy(x => x).ToList().Exists(x => x.Count() > 5))
                    {
                        LastSpamTime = _lastMessageTime;
                        _lastMessage.Clear();
                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(_lockSpam);
                }
            }
            return false;
        }
    }
}