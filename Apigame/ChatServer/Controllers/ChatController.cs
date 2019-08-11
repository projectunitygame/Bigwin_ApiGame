using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System.Web.Hosting;
using System.Text.RegularExpressions;
using ChatServer.Models;
using Utilities.Log;
using ChatServer.Hubs;
using Utilities.Cache;
using ChatServer.Helper;
using Utilities.Session;

namespace ChatServer.Controllers
{
    public class ChatController
    {
        private readonly static Lazy<ChatController> _instance = new Lazy<ChatController>(() => new ChatController(GlobalHost.ConnectionManager.GetHubContext<ChatHub>()));
        private static readonly object _lockChannel = new object();
        private static readonly object _lockUser = new object();
        private const int _lockTime = 5000; //ms

        /// <summary>
        /// Quản lý danh sách user Online
        /// </summary>
        private static readonly ConcurrentDictionary<long, ChatUser> UserOnlines = new ConcurrentDictionary<long, ChatUser>();
        /// <summary>
        /// Quản lý các kênh Chat
        /// </summary>
        private static readonly ConcurrentDictionary<string, ChatChannel> Channels = new ConcurrentDictionary<string, ChatChannel>();
        /// <summary>
        /// Quản lý các Connection
        /// </summary>
        private static readonly ConcurrentDictionary<string, long> ConnectionIdAccountId = new ConcurrentDictionary<string, long>();

        private readonly static string ADMINS_STR = ConfigurationManager.AppSettings["ADMINS"];
        public static List<string> ADMINS = new List<string>();

        private readonly static string TIME_INTERVAL_STR = ConfigurationManager.AppSettings["TIME_INTERVAL"];
        public static int TIME_INTERVAL = 60;

        /// <summary>
        /// Số lượng tối đa client online trong 1 kênh chat
        /// </summary>
        private readonly static string MAX_IDLE_USER_ONLINE_STR = ConfigurationManager.AppSettings["MAX_IDLE_USER_ONLINE"];
        public static int MAX_IDLE_USER_ONLINE = 600;

        /// <summary>
        /// Số lượng message tối đa trong 1 kênh chat
        /// </summary>
        private readonly static string MAX_MESSAGE_IN_CHANNEL_STR = ConfigurationManager.AppSettings["MAX_MESSAGE_IN_CHANNEL"];
        public static int MAX_MESSAGE_IN_CHANNEL = 100;

        /// <summary>
        /// Số ký tự tối đa của nội dung Chat
        /// </summary>
        //private readonly static string MAX_MESSAGE_LENGTH_STR = BicXML.ToString("MaxLengthMessage", "SpamBlock");
        private readonly static string MAX_MESSAGE_LENGTH_STR = "50";
        public static int MAX_MESSAGE_LENGTH = 50;

        /// <summary>
        /// Thời gian chát 1 tin nhắn
        /// </summary>
        private static string MIN_MESSAGE_TIME_SECOND = "5";

        /// <summary>
        /// Số User tối thiểu của một kênh Chat
        /// </summary>
        private readonly static string MIN_USER_INACTIVE_IN_CHANNEL_STR = ConfigurationManager.AppSettings["MIN_USER_INACTIVE_IN_CHANNEL"];
        public static int MIN_USER_INACTIVE_IN_CHANNEL = 10;

        private readonly static string ENABLE_FAKENAME_LV1_IN_CHANNEL_STR = ConfigurationManager.AppSettings["ENABLE_FAKENAME_LV1_IN_CHANNEL"];
        public static int ENABLE_FAKENAME_LV1_IN_CHANNEL;
        private readonly static string ENABLE_FAKENAME_LV2_IN_CHANNEL_STR = ConfigurationManager.AppSettings["ENABLE_FAKENAME_LV2_IN_CHANNEL"];
        public static int ENABLE_FAKENAME_LV2_IN_CHANNEL;
        private readonly static string TWO_MESSAGE_DURATION_STR = ConfigurationManager.AppSettings["TWO_MESSAGE_DURATION"];
        public static int TWO_MESSAGE_DURATION = 2;
        private readonly static string TEN_MESSAGE_DURATION_STR = ConfigurationManager.AppSettings["TEN_MESSAGE_DURATION"];
        public static int TEN_MESSAGE_DURATION = 30;
        private readonly static string DUPLICATE_MESSAGE_DURATION_STR = ConfigurationManager.AppSettings["DUPLICATE_MESSAGE_DURATION"];
        public static int DUPLICATE_MESSAGE_DURATION = 300;
        private readonly static string GLOBAL_TEN_SAME_MESSAGE_DURATION_STR = ConfigurationManager.AppSettings["GLOBAL_TEN_SAME_MESSAGE_DURATION"];

        private static readonly string BLACKLIST_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/BlackList.xml");
        private static readonly string KEYWORDREPLACE_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/KeywordReplace.xml");
        private static readonly string ACCOUNTBLOCK_FILE = HostingEnvironment.MapPath("~/App_Data/Chat/Data/AccountBlock.xml");
        private readonly static string CMSAllGame = ConfigurationManager.AppSettings["CMSAllGame"];

        public static int GLOBAL_TEN_SAME_MESSAGE_DURATION = 1800;

        private static IHubContext HubContext;

        private static Timer _timerClean;
        private static System.Timers.Timer aTimer;
        public static ChatController Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private List<ChatMessage> notifyList;

        /// <summary>
        /// Constructor - private
        /// </summary>
        /// <param name="hubContext"></param>
        private ChatController(IHubContext hubContext)
        {
            notifyList = new List<ChatMessage>();
            HubContext = hubContext;

            Int32.TryParse(TIME_INTERVAL_STR, out TIME_INTERVAL);
            Int32.TryParse(MAX_IDLE_USER_ONLINE_STR, out MAX_IDLE_USER_ONLINE);

            Int32.TryParse(MAX_MESSAGE_IN_CHANNEL_STR, out MAX_MESSAGE_IN_CHANNEL);
            Int32.TryParse(MAX_MESSAGE_LENGTH_STR, out MAX_MESSAGE_LENGTH);
            Int32.TryParse(MIN_USER_INACTIVE_IN_CHANNEL_STR, out MIN_USER_INACTIVE_IN_CHANNEL);

            Int32.TryParse(ENABLE_FAKENAME_LV1_IN_CHANNEL_STR, out ENABLE_FAKENAME_LV1_IN_CHANNEL);
            Int32.TryParse(ENABLE_FAKENAME_LV2_IN_CHANNEL_STR, out ENABLE_FAKENAME_LV2_IN_CHANNEL);

            Int32.TryParse(TWO_MESSAGE_DURATION_STR, out TWO_MESSAGE_DURATION);
            Int32.TryParse(TEN_MESSAGE_DURATION_STR, out TEN_MESSAGE_DURATION);
            Int32.TryParse(DUPLICATE_MESSAGE_DURATION_STR, out DUPLICATE_MESSAGE_DURATION);
            Int32.TryParse(GLOBAL_TEN_SAME_MESSAGE_DURATION_STR, out GLOBAL_TEN_SAME_MESSAGE_DURATION);

            TimerCallback cbClean = new TimerCallback(RemoveInactive);
            _timerClean = new Timer(cbClean, null, TIME_INTERVAL * 1000, TIME_INTERVAL * 1000); // 10 minutes

            //LoadListAdmin();
        }

        #region [Get XML -- huandh 2016.03.25]

        private void LoadListAdmin()
        {
            ADMINS = LoadDataConfig.LoadListAdmin();
        }
        private void LoadTimeSendMessage()
        {
            //MIN_MESSAGE_TIME_SECOND = BicXML.ToString("LimitSecond", "SpamBlock");
            MIN_MESSAGE_TIME_SECOND = "5";
        }
        #endregion


        /// <summary>
        /// Check kết nối
        /// </summary>
        /// <param name="hubCallerContext"></param>
        public void PingPong(HubCallerContext hubCallerContext)
        {
            try
            {
                long accountId = GetAccountId(hubCallerContext);
                if (accountId < 1)
                {
                    return;
                }

                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                    chatUser.LastActivity = DateTime.Now;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        /// <summary>
        /// Gửi message chat
        /// </summary>
        /// <param name="hubCallerContext"></param>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public bool SendMessage(HubCallerContext hubCallerContext, string message, string channelId)
        {
            NLogManager.LogMessage(">>Start SendMessage");
            try
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CloseChatServer"].ToString()))
                {
                    return false;
                }

                if (MAX_MESSAGE_LENGTH < message.Length)
                {
                    broadcastMessage(hubCallerContext.ConnectionId, string.Format("Nội dung chát không quá {0} kí tự!", MAX_MESSAGE_LENGTH));
                    return false;
                }

                //NLogManager.LogMessage(string.Format(">>Statr chat:{0}", channelId));
                LoadListAdmin();
                LoadTimeSendMessage();
                long accountId = GetAccountId(hubCallerContext);
                if (accountId < 1)
                {
                    NLogManager.LogMessage(string.Format("Sending message: not authenticated accountId: {0} - channel: {1} - content={2}", accountId, channelId, message));
                    return false;
                }
                ChatChannel chatChannel = GetChannel(channelId);
                if (chatChannel == null)
                {
                    NLogManager.LogMessage(string.Format("Sending message: accountId: {0} - not has channel: {1} - content={2}", accountId, channelId, message));
                    return false;
                }
                if (message.IndexOf('/') == 0)
                {
                    string notice;
                    bool ret = RunCommand(message, out notice);

                    if (!string.IsNullOrEmpty(notice))
                        Instance.ClientSystemMessage(channelId, notice, 0);

                    if (ret)
                    {
                        NLogManager.LogMessage(string.Format("Account: {0} - run command: {1}", accountId, message));
                        return true;
                    }
                }
                ChatUser chatUser = GetUser(accountId);
                if (chatUser == null)
                {
                    NLogManager.LogMessage(string.Format("Sending message: not chat user: {0} in channel={1} - content={2}", accountId, channelId, message));
                    return false;
                }
                if (CMSAllGame != "1")
                {
                    if (!ADMINS.Contains(chatUser.UserName))
                    {
                        try
                        {
                            object level = CacheHandler.Get(chatUser.UserName + "_" + channelId);
                            if (level == null)
                            {
                                SetCacheLevel(chatUser.UserName, (int)chatUser.AccountID, channelId);
                            }
                            else if (int.Parse(level.ToString()) == 0)
                            {
                                NLogManager.LogMessage(string.Format("Tổng đặt trong 7 ngày của bạn chưa đủ 2.000.000 Sao để tham gia chat!: {0} ", chatUser.UserName));
                                broadcastMessage(hubCallerContext.ConnectionId, string.Format("Tổng đặt trong 7 ngày của bạn chưa đủ 2.000.000 Sao để tham gia chat!"));
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            NLogManager.LogMessage(string.Format("{0} Null Cache level", chatUser.UserName));
                            SetCacheLevel(chatUser.UserName, (int)chatUser.AccountID, channelId);
                        }
                    }
                }

                string filteredMessage = message;
                string tempFilteredMessage = message;
                bool flag = false;

                //WaitCallback callBack = new WaitCallback(InsertChat);
                //var chatchat = new Insert_Chat();
                //chatchat.AccountID = int.Parse(chatUser.AccountID.ToString());
                //chatchat.UserName = string.Format("{0}({1})", chatUser.UserName, chatUser.NickName);
                //chatchat.channelId = channelId;
                //chatchat.filteredMessage = filteredMessage;
                //ThreadPool.QueueUserWorkItem(callBack, chatchat);

                //khong phai admin thi loc bad link
                if (!ADMINS.Contains(chatUser.UserName))
                {
                    ChatFilter.RemoveBadLinks(tempFilteredMessage, out flag);
                }
                //Check thời gian chát theo quy định

                if (!ADMINS.Contains(chatUser.UserName))
                {
                    try
                    {
                        object value = CacheHandler.Get(chatUser.UserName);
                        if (value == null)
                        {
                            CacheHandler.Add(chatUser.UserName, DateTime.Now.ToString());
                        }
                        else
                        {
                            if (DateTime.Now != DateTime.Parse(value.ToString()))
                            {
                                var lastTime = (DateTime.Now - DateTime.Parse(value.ToString())).TotalSeconds;
                                var a = MIN_MESSAGE_TIME_SECOND;
                                if (lastTime < int.Parse(MIN_MESSAGE_TIME_SECOND))
                                {
                                    broadcastMessage(hubCallerContext.ConnectionId, string.Format("Chưa hết thời gian chờ {0}s, t/g chờ còn {1}s", MIN_MESSAGE_TIME_SECOND, System.Math.Round((int.Parse(MIN_MESSAGE_TIME_SECOND) - lastTime), 2)));
                                    return false;
                                }
                            }
                            CacheHandler.Add(chatUser.UserName, DateTime.Now.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        NLogManager.LogMessage(string.Format("{0} Null Cache TimeChat", chatUser.UserName));
                        CacheHandler.Add(chatUser.UserName, DateTime.Now.ToString());
                    }
                }



                if (CheckBanUsers(chatUser.UserName))
                {
                    NLogManager.LogMessage(string.Format(">> {0} - Tài khoản đang bị Block! ", chatUser.UserName));
                    broadcastMessage(hubCallerContext.ConnectionId, ReturnCheckBanUsers(chatUser.UserName));
                    return false;
                }

                //nếu nội dung chat có bad links thì block gói tin và ghi log
                if (flag)
                {
                    chatUser.CountBadLinks += 1;
                    filteredMessage = "***";
                    if (chatUser.CountBadLinks > 5)
                    {
                        ChatFilter.BanUser(chatUser.UserName);
                    }

                    NLogManager.LogMessage(string.Format("User sent bad link: accountId={0}, username={1} - channelId={2} - content={3} - \n\rAgent: {4}", chatUser.AccountID, chatUser.UserName, channelId, message, chatUser.UserAgent));
                }
                else
                {
                    //khong phai admin thi loc bad word
                    if (!ADMINS.Contains(chatUser.UserName))
                        filteredMessage = ChatFilter.RemoveBadWords(message, out flag);

                    NLogManager.LogMessage(flag
                        ? string.Format(
                            "User sent bad word: accountId={0}, username={1} - channelId={2} - content={3}",
                            chatUser.AccountID, chatUser.UserName, channelId, message)
                        : string.Format("User sent message: accountId={0}, username={1} - channelId={2} - content={3}",
                            chatUser.AccountID, chatUser.UserName, channelId, message));
                }

                //khong phai admin thi loc bad word
                if (!ADMINS.Contains(chatUser.UserName))
                    filteredMessage = RemoveBadWords(message, out flag);

                //Thay thế từ khóa
                if (!ADMINS.Contains(chatUser.UserName))
                    filteredMessage = ReplaceKeyword(filteredMessage);


                //admin thì text chat màu đỏ
                ChatMessage chatMessage = new ChatMessage(channelId, chatUser.AccountID, !ADMINS.Contains(chatUser.UserName) ? chatUser.NickName : string.Format("<span style='color:red; font-weight:bold'>ADMIN ({0})</span>", chatUser.NickName), !ADMINS.Contains(chatUser.UserName) ? filteredMessage : string.Format("{0}", filteredMessage));
                bool canSend;
                if (!ADMINS.Contains(chatUser.UserName))
                {
                    canSend = chatChannel.AddMessage(chatMessage);
                }
                else
                {
                    canSend = chatChannel.AddMessageAdmin(chatMessage);
                }

                if (canSend)
                    canSend = chatUser.AddMessage(channelId, chatMessage);

                if (canSend)
                {
                    ClientReceiveMessage(chatMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

            NLogManager.LogMessage(string.Format(">>Gửi tin nhắn không thành công! "));
            return false;
        }
        public string RemoveBadWords(string input, out bool Flag)
        {
            Flag = false;
            var BadWords = LoadDataConfig.LoadListBadWords();
            int bwLength = BadWords.Count;
            for (int i = 0; i < bwLength; i++)
            {
                try
                {
                    input = input.Replace(BadWords[i].ToString(), "***");
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
            }

            return input;
        }
        public string ReplaceKeyword(string input)
        {
            var KeywordReplace = LoadDataConfig.LoadListKeywordReplace();
            //NLogManager.LogMessage(JsonConvert.SerializeObject(KeywordReplace));
            if (KeywordReplace.Count > 0)
            {
                foreach (var item in KeywordReplace)
                {
                    input = input.Replace(item.text, item.replace);
                }
            }
            return input;
        }
        public bool CheckBanUsers(string username)
        {
            var ListAccountBlock = LoadDataConfig.LoadListAccountBlock();
            if (ListAccountBlock.Count > 0)
            {
                foreach (var item in ListAccountBlock)
                {
                    if (item.name.Trim() == username.Trim())
                    {
                        if (DateTime.Now >= DateTime.Parse(item.endtimeblock))
                        {
                            DeleteAccountBlock(item.key);
                            return false;
                        }
                        return true;
                    }
                }
            }
            return false;
            //Bỏ Return đọc từ file txt cũ - huandh
            //return BanUsers.Contains(username);
        }
        public string ReturnCheckBanUsers(string username)
        {
            string message = "";
            var ListAccountBlock = LoadDataConfig.LoadListAccountBlock();
            if (ListAccountBlock.Count > 0)
            {
                foreach (var item in ListAccountBlock)
                {
                    if (item.name == username)
                    {
                        message = string.Format("<span style='color:red; font-weight:bold'>Tài khoản đang bị Block, lý do: {0} - Thời hạn đến: {1}</span>", item.namereasonblock, item.endtimeblock);
                    }
                }
            }
            return message;
        }
        private void DeleteAccountBlock(string key)
        {
            try
            {
                XDocument xmldoc = XDocument.Load(ACCOUNTBLOCK_FILE);
                XElement xmlelement = xmldoc.Element("AccountBlock").Elements("key").Single(x => (string)x.Attribute("key") == key);
                xmlelement.Remove();
                xmldoc.Save(ACCOUNTBLOCK_FILE);
                //BicCache.Remove("ACCOUNTBLOCK_FILE");
                LoadDataConfig.ForceGetListAccBlock();
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }

        }
        public void broadcastMessage(string connectionId, string message)
        {
            HubContext.Clients.Client(connectionId).broadcastMessage(message);
        }

        private void SetCacheLevel(string AccountName, int AccountID, string channelId)
        {
            try
            {
                object value = null;
                try
                {
                    value = CacheHandler.Get(AccountName + "_" + channelId);
                }
                catch (Exception ex)
                {
                    value = null;
                }
                bool _Enable = true;
                if (value == null)
                {
                    //AbstractDAOFactory.Instance().CreateAccountInfoDao().SP_Chat_Check(AccountID, out _Enable);
                    NLogManager.LogMessage(string.Format(">>> SetCacheLevel AccountName: {0} | AccountID: {1} | Enable: {2}", AccountName, AccountID, _Enable));
                    CacheHandler.Add(AccountName + "_" + channelId, _Enable == true ? 1 : 0);
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage(">>> Exception SetCacheLevel:" + ex.Message);
            }
        }



        /// <summary>
        /// Đăng ký kênh chat
        /// </summary>
        /// <param name="hubCallerContext"></param>
        /// <param name="channelId"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        public bool RegisterChat(HubCallerContext hubCallerContext, string channelId, string nickName = "")
        {
            try
            {
                HubContext.Groups.Add(hubCallerContext.ConnectionId, channelId);

                long accountId = GetAccountId(hubCallerContext);
    
                ChatChannel chatChannel = GetChannel(channelId, true);
                if (accountId < 1)
                {
                    ClientListLastMessages(hubCallerContext, chatChannel);
                    return false;
                }

                ChatUser chatUser = GetUser(accountId, channelId, true);
                //var dataUser = AbstractDAOFactory.Instance().CreateMessageDao().SP_Account_Get_Infor(int.Parse(accountId.ToString()));
                //if (dataUser != null)
                //{
                //    nickName = string.IsNullOrEmpty(dataUser.NickName) ? StringUtil.MaskUserName(chatUser.UserName) : dataUser.NickName;
                //}
                //else
                //{
                //    if (string.IsNullOrEmpty(nickName))
                //        nickName = StringUtil.MaskUserName(chatUser.UserName);
                //}
                nickName = AccountSession.AccountName;
                if (nickName.Trim() == "System" || nickName.Trim() == "system")
                    return false;
                chatUser.NickName = nickName;
                chatUser.SetActive(true);
                chatUser.SetChannelConnectionId(channelId, hubCallerContext.ConnectionId);

                chatChannel.AddUser(chatUser);
                SetCacheLevel(chatUser.UserName, (int)chatUser.AccountID, channelId);
                NLogManager.LogMessage(string.Format("User join channel: {0}:{1} - ChannelId={2}", chatUser.AccountID, chatUser.UserName, channelId));

                ClientListUserOnlines(hubCallerContext, chatChannel);
                ClientListLastMessages(hubCallerContext, chatChannel);
                ClientAddUserOnline(channelId, chatUser);

                #region Fake user in channel chat
                ThreadPool.QueueUserWorkItem(__ =>
                {
                    int countUserOnline = chatChannel.UserOnlines.Count;
                    //neu it them mot user fake
                    if (countUserOnline < ENABLE_FAKENAME_LV1_IN_CHANNEL)
                    {
                        Thread.Sleep(3000);
                        chatChannel.AddFakeUser();
                        //neu it qua them mot user fake nua
                        if (countUserOnline < ENABLE_FAKENAME_LV2_IN_CHANNEL)
                        {
                            Thread.Sleep(2000);
                            chatChannel.AddFakeUser();
                        }
                    }
                    else if (countUserOnline > MIN_USER_INACTIVE_IN_CHANNEL)
                    {
                        chatChannel.RemoveFakeUser();
                    }
                });
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        public bool UnregisterChat(HubCallerContext hubCallerContext, string channelId)
        {
            try
            {
                HubContext.Groups.Remove(hubCallerContext.ConnectionId, channelId);

                long accountId = GetAccountId(hubCallerContext);

                if (accountId < 1)
                    return false;

                ChatChannel chatChannel = GetChannel(channelId);
                if (chatChannel != null)
                {
                    ChatUser chatUser = GetUser(accountId);
                    if (chatUser != null)
                    {
                        chatUser.SetActive(false);
                        NLogManager.LogMessage(string.Format("User leave channel: {0}:{1} - ChannelId={2}", chatUser.AccountID, chatUser.UserName, channelId));
                    }

                    //neu phong chat it hon 20 nguoi khong remove user khoi danh sach onlines (fake so luong user online)
                    if (chatChannel.UserOnlines.Count > MIN_USER_INACTIVE_IN_CHANNEL)
                    {
                        chatChannel.RemoveUser(accountId);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return false;
        }

        public void ClientReceiveMessage(ChatMessage chatMessage)
        {
            HubContext.Clients.Group(chatMessage.ChannelId).receiveMessage(chatMessage);
        }

        public void ClientSystemMessage(string channelId, string message, int type)
        {
            HubContext.Clients.Group(channelId).systemMessage(message, type);
        }

        public void ClientListLastMessages(HubCallerContext hubCallerContext, string channelId)
        {
            ChatChannel chatChannel = GetChannel(channelId);
            ClientListLastMessages(hubCallerContext, chatChannel);
           
        }

        public void ClientListLastMessages(HubCallerContext hubCallerContext, ChatChannel chatChannel)
        {
            NLogManager.LogMessage("ClientListLastMessages : " + chatChannel);

            if (chatChannel != null)
            {
                HubContext.Clients.Client(hubCallerContext.ConnectionId).listLastMessages(JsonConvert.SerializeObject(chatChannel.LastMessages.ToArray()));
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    Thread.Sleep(3000);
                    HubContext.Clients.Client(hubCallerContext.ConnectionId).receiveMessage(new ChatMessage(chatChannel.ChannelId, 0, "System", "Hiện tại game chưa xây dựng hệ thống đại lý, vui lòng cẩn thận tránh bị lừa đảo"));
                });
            }
        }

        public void ClientListUserOnlines(HubCallerContext hubCallerContext, string channelId)
        {
            ChatChannel chatChannel = GetChannel(channelId);
            ClientListUserOnlines(hubCallerContext, chatChannel);
        }

        public void ClientListUserOnlines(HubCallerContext hubCallerContext, ChatChannel chatChannel)
        {
            if (chatChannel != null)
            {
                HubContext.Clients.Client(hubCallerContext.ConnectionId).listUserOnlines(JsonConvert.SerializeObject(chatChannel.UserOnlines.Values.ToArray()));
            }
        }

        public void ClientAddUserOnline(string channelId, ChatUser chatUser)
        {
            //send message new user join chat to other users in channel
            HubContext.Clients.Group(channelId + "_admin").addUserOnline(chatUser);
            //send all user in channel message 
            //ClientSystemMessage(channelId, string.Format("{0} đã vào game.", chatUser.NickName), 0);
        }

        public void ClientRemoveUserOnline(string channelId, ChatUser chatUser)
        {
            //send message user left chat to other users in channel
            HubContext.Clients.Group(channelId + "_admin").removeUserOnline(chatUser);
            //send all user in channel message 
            //ClientSystemMessage(channelId, string.Format("{0} đã rời game.", chatUser.NickName), 1);
        }

        public void OnConnected(HubCallerContext hubCallerContext)
        {

            long accountId = GetAccountId(hubCallerContext);
            NLogManager.LogMessage("OnConnected: " + accountId );

            if (accountId < 1)
                return;

            if (!Monitor.TryEnter(_lockUser, _lockTime))
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                {
                    chatUser.SetActive(true);
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Add(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public void OnReconnected(HubCallerContext hubCallerContext)
        {
            long accountId = GetAccountId(hubCallerContext);

            if (accountId < 1)
                return;
            if (!Monitor.TryEnter(_lockUser, _lockTime))
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (chatUser != null)
                {
                    chatUser.SetActive(true);
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Add(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public void OnDisconnected(HubCallerContext hubCallerContext)
        {
            long accountId = GetAccountId(hubCallerContext);

            if (accountId < 1)
                return;

            try
            {
                ChatUser chatUser = GetUser(accountId);
                if (Monitor.TryEnter(_lockUser, _lockTime))
                {
                    if (chatUser != null)
                    {
                        chatUser.SetActive(false);
                    }
                }
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }

            //group theo accountId để gửi message cho toàn bộ các connectionId cùng accountId
            //HubContext.Groups.Remove(hubCallerContext.ConnectionId, accountId.ToString());
        }

        public ChatChannel GetChannel(string channelId, bool AutoCreate = false)
        {
            if (!Monitor.TryEnter(_lockChannel, _lockTime)) return null;
            try
            {
                ChatChannel chatChannel = null;
                if (AutoCreate)
                {
                    chatChannel = Channels.GetOrAdd(channelId, _channel => new ChatChannel(channelId));
                    NLogManager.LogMessage(string.Format("Create channel chat: {0}", channelId));
                    return chatChannel;
                }

                Channels.TryGetValue(channelId, out chatChannel);
                return chatChannel;
            }
            finally
            {
                Monitor.Exit(_lockChannel);
            }
        }

        public ChatUser GetUser(long accountId, string channelId = "all_world", bool AutoCreate = false)
        {
            if (!Monitor.TryEnter(_lockUser, _lockTime)) return null;

            try
            {
                ChatUser chatUser = null;
                if (AutoCreate)
                {
                    string username = AccountSession.AccountName;
                    chatUser = UserOnlines.GetOrAdd(accountId, _user => new ChatUser(accountId, username, "", channelId));

                    NLogManager.LogMessage(string.Format("User join chat: {0} : {1}", chatUser.AccountID, chatUser.UserName));

                    return chatUser;
                }

                UserOnlines.TryGetValue(accountId, out chatUser);
                return chatUser;
            }
            finally
            {
                Monitor.Exit(_lockUser);
            }
        }

        public List<ChatUser> GetAllUserChanel(string channelId = "all_world")
        {
            var info = new List<ChatUser>();
            try
            {
                foreach (var userOnline in UserOnlines)
                {
                    var userInfo = userOnline.Value;
                    info.Add(userInfo);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return info;
        }

        private bool RunCommand(string message, out string notice)
        {
            string username = AccountSession.AccountName;
            bool ret = false;
            notice = null;

            if (ADMINS.Contains(username))
            {
                string[] pars = message.Split(' ');
                if (pars.Length > 1)
                {
                    string cmd = pars[0];
                    string par = pars[1];
                    string args = "";
                    if (pars.Length > 2)
                    {
                        for (int i = 2; i < pars.Length; i++)
                            args += pars[i] + " ";
                    }

                    if (cmd.Equals("/banuser"))
                    {
                        string reason = "Tài khoản {0} bị treo nick, lí do: tuyên truyền website hack";
                        if (!string.IsNullOrEmpty(args))
                            reason = args;

                        ret = ChatFilter.BanUser(par);
                        notice = string.Format(reason, par);
                    }
                    else if (cmd.Equals("/addbadlink"))
                    {
                        ret = ChatFilter.AddBadLink(par);
                    }
                    else if (cmd.Equals("/addbadword"))
                    {
                        if (pars.Length > 2)
                        {
                            for (int i = 2; i < pars.Length; i++)
                                par += " " + pars[i];
                        }
                        ret = ChatFilter.AddBadWord(par);
                    }
                    else if (cmd.Equals("/message"))
                    {
                        notice = par + " " + args;
                        ret = true;
                    }
                }
            }
            return ret;
        }

        private long GetAccountId(HubCallerContext context)
        {
            return ConnectionIdAccountId.GetOrAdd(context.ConnectionId, _accountId => AccountSession.GetAccountID(context));
        }

        private void RemoveInactive(object obj)
        {
            try
            {
                //Xóa các user inactive trong 10p khỏi bộ nhớ hệ thống
                if (UserOnlines != null && UserOnlines.Count > 0)
                {
                    //Những user có kênh chat inactive trong 10p
                    DateTime now = DateTime.Now;
                    ConcurrentDictionary<long, ChatUser> list = new ConcurrentDictionary<long, ChatUser>(UserOnlines.Where(_ => _.Value != null && _.Value.LastActivity.AddSeconds(MAX_IDLE_USER_ONLINE) < now));//((!_.Value.IsActive && _.Value.LastMessageSentTime.AddSeconds(TIME_INTERVAL * 3) < now) || _.Value.LastActivity.AddSeconds(MAX_IDLE_USER_ONLINE) < now)));
                    if (list != null && list.Count > 0)
                    {
                        foreach (ChatUser chatUser in list.Values)
                        {
                            foreach (string channelId in chatUser.ChannelConnectionIds.Keys)
                            {
                                string oldConnectionId;
                                if (chatUser.RemoveChannelConnectionId(channelId, out oldConnectionId))
                                    HubContext.Groups.Remove(oldConnectionId, channelId);

                                ChatChannel chatChannel = GetChannel(channelId);
                                if (chatChannel != null)
                                {
                                    NLogManager.LogMessage(string.Format("ChatChannel --> RemoveUser: {0} - {1}:{2}", channelId, chatUser.AccountID, chatUser.UserName));
                                    chatChannel.RemoveUser(chatUser.AccountID);
                                }
                            }

                            if (chatUser.ChannelConnectionIds.Keys.Count == 0)
                            {
                                if (Monitor.TryEnter(_lockUser, _lockTime))
                                {
                                    try
                                    {
                                        ChatUser outChatUser = null;
                                        UserOnlines.TryRemove(chatUser.AccountID, out outChatUser);

                                        if (outChatUser != null)
                                        {
                                            ConcurrentDictionary<string, long> listConn = new ConcurrentDictionary<string, long>(ConnectionIdAccountId.Where(__ => __.Value == chatUser.AccountID));
                                            if (listConn != null && listConn.Count > 0)
                                            {
                                                long accId;
                                                foreach (string connId in listConn.Keys)
                                                    ConnectionIdAccountId.TryRemove(connId, out accId);
                                            }

                                            NLogManager.LogMessage(string.Format("InActive User: {0} - {1}", outChatUser.AccountID, outChatUser.UserName));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        NLogManager.PublishException(e);
                                    }
                                    finally
                                    {
                                        Monitor.Exit(_lockUser);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NLogManager.LogMessage("Inactive 1");
                NLogManager.PublishException(e);
            }

            try
            {
                //xóa các kênh chat không hoạt động
                if (Channels != null && Channels.Count > 0)
                {
                    ConcurrentDictionary<string, ChatChannel> list = new ConcurrentDictionary<string, ChatChannel>(Channels.Where(_ => _.Value != null && _.Value.UserOnlines.Count == 0));
                    if (list != null && list.Count > 0)
                    {
                        foreach (ChatChannel chatChannel in list.Values)
                        {
                            if (Monitor.TryEnter(_lockChannel, _lockTime))
                            {
                                try
                                {
                                    ChatChannel outChatChannel = null;
                                    Channels.TryRemove(chatChannel.ChannelId, out outChatChannel);
                                    NLogManager.LogMessage(string.Format("Remove ChatChannel: {0}", chatChannel.ChannelId));
                                }
                                catch (Exception e)
                                {
                                    NLogManager.PublishException(e);
                                }
                                finally
                                {
                                    Monitor.Exit(_lockChannel);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                NLogManager.LogMessage("Inactive 2");
                NLogManager.PublishException(e);
            }
        }

        public static void ClearMessage(string channelId)
        {
            try
            {
                ChatChannel chatChannel = null;
                chatChannel = Channels.GetOrAdd(channelId, _channel => new ChatChannel(channelId));
                if (chatChannel != null)
                {
                    chatChannel.LastMessages.Clear();
                }
            }
            catch (Exception ex)
            {
                NLogManager.LogMessage(">> Ex ClearMessage:" + ex.Message);
            }
        }

        //private static void InsertChat(object state)
        //{
        //    var data = (Insert_Chat)state;

        //    AbstractDAOFactory.Instance().CreateMessageDao().SP_Chat_Insert(data.AccountID, data.UserName, data.channelId, data.filteredMessage);
        //}

    }
    public class Insert_Chat
    {
        public int AccountID { get; set; }
        public string UserName { get; set; }
        public string channelId { get; set; }
        public string filteredMessage { get; set; }
    }

}