using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ChatServer.Controllers;
using Microsoft.AspNet.SignalR;

namespace ChatServer.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// Kiem tra ket noi
        /// </summary>
        public void PingPong()
        {
            ChatController.Instance.PingPong(Context);
        }

        public bool SendMessage(string message, string channelId)
        {
            if (string.IsNullOrEmpty(channelId) || string.IsNullOrEmpty(message) || message.Length > ChatController.MAX_MESSAGE_LENGTH)
                return false;
            return ChatController.Instance.SendMessage(Context, message, channelId);
        }
        public bool RegisterChat(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return false;

            return ChatController.Instance.RegisterChat(Context, channelId);
        }
        public void RegisterAdmin(string channelId)
        {
            Groups.Add(Context.ConnectionId, channelId + "_admin");
        }

        public bool UnregisterChat(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
                return false;

            return ChatController.Instance.UnregisterChat(Context, channelId);
        }

        public override Task OnConnected()
        {
            ChatController.Instance.OnConnected(Context);
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool force)
        {
            ChatController.Instance.OnDisconnected(Context);
            return base.OnDisconnected(true);
        }

        public override Task OnReconnected()
        {
            ChatController.Instance.OnReconnected(Context);
            return base.OnReconnected();
        }
    }
}