using ChatServer.Controllers;
using ChatServer.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
 namespace ChatServer.Models
{
    public class ChatChannel
    {
        private const int _lockTime = 50; //ms

        public int Type;
        public string ChannelId;
        public ConcurrentDictionary<long, ChatUser> UserOnlines;
        public List<ChatMessage> LastMessages;

        public ChatChannel(string channelId)
        {
            ChannelId = channelId;
            UserOnlines = new ConcurrentDictionary<long, ChatUser>();
            LastMessages = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CloseChatServer"].ToString()))
            {
                ChatMessage chatMessage = new ChatMessage(channelId, 0, "System", "Hệ thống đang tạm thời bảo trì kênh chat. Vui lòng quay lại sau");
                LastMessages.Add(chatMessage);
            }
            
        }

        public bool AddMessage(ChatMessage chatMessage)
        {
            if (chatMessage == null)
                return false;

            DateTime now = DateTime.Now;
            string compareMessage = ChatFilter.CutOff(chatMessage.Content, " ,.-_():;/\\\'\"");
            //trong vòng 300s=5p không được gửi tin nhắn có nội dung giống nhau
            List<ChatMessage> lst = new List<ChatMessage>(LastMessages.Where(m => m != null && !string.IsNullOrEmpty(m.Content) && ChatFilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage) && m.CreatedDate.AddSeconds(ChatController.DUPLICATE_MESSAGE_DURATION) > now));
            if (lst.Count > 0)
                return false;

            if (LastMessages.Count > ChatController.MAX_MESSAGE_IN_CHANNEL)
            {
                if (Monitor.TryEnter(LastMessages, 500))
                {
                    LastMessages.RemoveRange(0, LastMessages.Count - ChatController.MAX_MESSAGE_IN_CHANNEL);
                    Monitor.Exit(LastMessages);
                }
            }

            LastMessages.Add(chatMessage);
            return true;
        }
        public bool AddMessageAdmin(ChatMessage chatMessage)
        {
            if (chatMessage == null)
                return false;

            DateTime now = DateTime.Now;
            string compareMessage = ChatFilter.CutOff(chatMessage.Content, " ,.-_():;/\\\'\"");
            //trong vòng 300s=5p không được gửi tin nhắn có nội dung giống nhau
            //List<ChatMessage> lst = new List<ChatMessage>(LastMessages.Where(m => m != null && !string.IsNullOrEmpty(m.Content) && ChatFilter.CutOff(m.Content, " ,.-_():;/\\\'\"").Equals(compareMessage) && m.CreatedDate.AddSeconds(ChatController.DUPLICATE_MESSAGE_DURATION) > now));
            //if (lst.Count > 0)
            //    return false;

            if (LastMessages.Count > ChatController.MAX_MESSAGE_IN_CHANNEL)
            {
                if (Monitor.TryEnter(LastMessages, 500))
                {
                    LastMessages.RemoveRange(0, LastMessages.Count - ChatController.MAX_MESSAGE_IN_CHANNEL);
                    Monitor.Exit(LastMessages);
                }
            }

            LastMessages.Add(chatMessage);
            return true;
        }

        public bool AddUser(ChatUser chatUser)
        {
            if (chatUser == null)
                return false;

            const bool ret = false;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    ChatUser newChatUser = new ChatUser(chatUser.AccountID, chatUser.UserName, chatUser.NickName,"abc1")
                    {
                        ClientIP = chatUser.ClientIP
                    };
                    UserOnlines.GetOrAdd(chatUser.AccountID, newChatUser);
                    return true;
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }
            return ret;
        }

        public ChatUser GetUser(long accountId)
        {
            ChatUser chatUser = null;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    UserOnlines.TryGetValue(accountId, out chatUser);
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }
            return chatUser;
        }

        public ChatUser RemoveUser(long accountId)
        {
            ChatUser chatUser = null;
            if (Monitor.TryEnter(UserOnlines, _lockTime))
            {
                try
                {
                    UserOnlines.TryRemove(accountId, out chatUser);
                    if (chatUser != null)
                    {
                        ChatController.Instance.ClientRemoveUserOnline(ChannelId, chatUser);
                    }
                }
                finally
                {
                    Monitor.Exit(UserOnlines);
                }
            }

            return chatUser;
        }

        #region Add fake user to chat channel functions
        private static ConcurrentDictionary<long, ChatUser> fakeUserList;
        public bool AddFakeUser()
        {
            if (fakeUserList == null || fakeUserList.Count == 0)
            {
                fakeUserList = new ConcurrentDictionary<long, ChatUser>();
                string filePath = HostingEnvironment.MapPath("~/App_Data/Chat/Data/fakeusers.txt"); 
                try
                {
                    if (File.Exists(filePath))
                    {
                        if (filePath != null)
                        {
                            string[] allText = File.ReadAllLines(filePath);

                            foreach (var text in allText)
                            {
                                try
                                {
                                    string[] acc = text.Split(',');
                                    if (acc.Length > 1)
                                    {
                                        long accountId = Int64.Parse(acc[0]);
                                        ChatUser c = new ChatUser(accountId, acc[1], acc[1],"abc2")
                                        {
                                            IsActive = false,
                                            UserAgent = "AUTOBOT"
                                        };
                                        fakeUserList.GetOrAdd(accountId, c);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
 
            return false;
        }

        public ChatUser RemoveFakeUser()
        { 
            return null;
        }
        #endregion
    }
}