using System;
using Newtonsoft.Json;

namespace ChatServer.Models
{
    public class ChatMessage
    {
        [JsonProperty("i")]
        public string ChannelId;

        [JsonProperty("a")]
        public long AccountID;

        [JsonProperty("n")]
        public string NickName;

        [JsonProperty("c")]
        public string Content;

        [JsonProperty("d"), JsonIgnore]
        public DateTime CreatedDate;

        [JsonProperty("l"), JsonIgnore]
        public DateTime LastModifiedDate;

        public ChatMessage()
        {
            CreatedDate = DateTime.Now;
        }

        public ChatMessage(string channelId, long accountID, string username, string content)
        {
            ChannelId = channelId;
            AccountID = accountID;
            NickName = username;
            Content = content;
            CreatedDate = DateTime.Now;
        }
    }
}