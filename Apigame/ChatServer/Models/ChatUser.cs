using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ChatServer.Controllers;
using ChatServer.Helper;
using Newtonsoft.Json;

namespace ChatServer.Models
{
    public class ChatUser
    {
        [JsonProperty("a")]
        public long AccountID;

        [JsonProperty("u")]
        public string UserName;

        [JsonProperty("n")]
        public string NickName;

        [JsonProperty("c")]
        public string ClientIP;

        [JsonIgnore]
        public string UserAgent;

        [JsonIgnore]
        public bool IsActive;

        [JsonIgnore]
        public DateTime LastActivity;

        [JsonProperty("l")]
        public DateTime LastMessageSentTime;

        [JsonIgnore]
        public List<ChatMessage> LastMessages;

        [JsonIgnore]
        public DateTime JoinChatTime;

        [JsonIgnore]
        public ConcurrentDictionary<string, string> ChannelConnectionIds;

        [JsonIgnore]
        public int CountBadLinks;

        [JsonProperty("i")]
        public string ChannelID;

        public ChatUser()
        {
            IsActive = true;
            LastActivity = DateTime.Now;
            JoinChatTime = DateTime.Now;
            LastMessageSentTime = DateTime.Now;
            LastMessages = new List<ChatMessage>();
            ChannelConnectionIds = new ConcurrentDictionary<string, string>();
            CountBadLinks = 0;
            ChannelID = "";
        }

        public ChatUser(long accountId)
            : this()
        {
            AccountID = accountId;
        }

        public ChatUser(long accountId, string username, string nickname,string channelid)
            : this()
        {
            AccountID = accountId;
            UserName = username;
            NickName = nickname;
            ChannelID = channelid;
        }

        public bool AddMessage(string channelId, ChatMessage chatMessage)
        {
            DateTime now = DateTime.Now;
            if (LastMessages.Count > 0)
            {

                if (DateTime.Compare(LastMessageSentTime.AddSeconds(ChatController.TWO_MESSAGE_DURATION), now) > 0)
                    return false;

                if (LastMessages.Count > 100)
                {
                    int pos = LastMessages.Count - 100;
                    if (LastMessages.ElementAt(pos).CreatedDate.AddSeconds(ChatController.TEN_MESSAGE_DURATION) > now)
                        return false;
                }

                string compareMessage = ChatFilter.CutOff(chatMessage.Content, " ,.-_():;/\\\'\"");
                List<ChatMessage> lst = new List<ChatMessage>(LastMessages.Where(m => m != null && !string.IsNullOrEmpty(m.Content) && ChatFilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage) && m.CreatedDate.AddSeconds(ChatController.GLOBAL_TEN_SAME_MESSAGE_DURATION) > now));
                if (lst.Count > 120)
                    return false;

                if (LastMessages.Count > 1200)
                {
                    if (Monitor.TryEnter(LastMessages, 50000))
                    {
                        LastMessages.RemoveRange(0, LastMessages.Count - 1200);
                        Monitor.Exit(LastMessages);
                    }
                }
            }

            LastActivity = now;
            LastMessageSentTime = now;
            LastMessages.Add(chatMessage);
            return true;
        }

        public void SetActive(bool active)
        {
            IsActive = active;

            if (active)
                LastActivity = DateTime.Now;
        }

        public void SetChannelConnectionId(string channelId, string connectionId)
        {
            if (Monitor.TryEnter(ChannelConnectionIds, 50000))
                try
                {
                    ChannelConnectionIds.TryAdd(channelId, connectionId);
                }
                finally
                {
                    Monitor.Exit(ChannelConnectionIds);
                }
        }

        public bool RemoveChannelConnectionId(string channelId, out string connectionId)
        {
            if (Monitor.TryEnter(ChannelConnectionIds, 50000))
                try
                {
                    return ChannelConnectionIds.TryRemove(channelId, out connectionId);
                }
                finally
                {
                    Monitor.Exit(ChannelConnectionIds);
                }
            connectionId = null;
            return false;
        }

    }
}