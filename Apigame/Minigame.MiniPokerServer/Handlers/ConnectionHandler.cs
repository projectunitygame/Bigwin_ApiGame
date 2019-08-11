using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MiniPoker.WebServer.Controllers;
using Utilities.Log;

namespace MiniPoker.WebServer.Handlers
{
    public class ConnectionHandler
    {
        private readonly static Lazy<ConnectionHandler> _instance;

        public IHubConnectionContext<dynamic> Clients
        {
            get;
            private set;
        }

        public IGroupManager Groups
        {
            get;
            private set;
        }

        public static ConnectionHandler Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        static ConnectionHandler()
        {
            _instance = new Lazy<ConnectionHandler>(() => new ConnectionHandler(GlobalHost.ConnectionManager.GetHubContext<MinipokerHub>()));
        }

        private ConnectionHandler(IHubContext hubContext)
        {
            Clients = hubContext.Clients;
            Groups = hubContext.Groups;
        }

        public void AddGroup(string connectionId, string groupName)
        {
            try
            {
                Groups.Add(connectionId, groupName);
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }

        }

        public string GetGroupName(byte betType, byte roomID = 0, string game = "")
        {
            object[] objArray = { betType, "_", roomID, "_", game };
            return string.Concat(objArray);
        }

        public void RemoveGroups(string connectionId, ICollection<string> groups)
        {
            try
            {
                foreach (string group in groups)
                {
                    if (!string.IsNullOrEmpty(group))
                    {
                        Groups.Remove(connectionId, group);
                    }
                }
            }
            catch (Exception exception)
            {
                NLogManager.PublishException(exception);
            }

        }
    }

}