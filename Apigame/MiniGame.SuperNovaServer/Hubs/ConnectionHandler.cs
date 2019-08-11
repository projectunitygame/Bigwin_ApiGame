using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Utilities.Log;

namespace Studio.WebGame.SupperNova.Hubs
{
    public class ConnectionHandler
    {
        private static readonly Lazy<ConnectionHandler> _instance =
               new Lazy<ConnectionHandler>(() => new ConnectionHandler(GlobalHost.ConnectionManager.GetHubContext<GameHub>()));

        private readonly ConcurrentDictionary<string, long> _mapHubAccount = new ConcurrentDictionary<string, long>();

        private readonly ConcurrentDictionary<long, List<string>> _mapAccountHub = new ConcurrentDictionary<long, List<string>>();

        public IHubContext HubContext { get; private set; }

        private ConnectionHandler(IHubContext hubContext)
        {
            HubContext = hubContext;
        }

        public static ConnectionHandler Instance
        {
            get { return _instance.Value; }
        }

        public void AddGroup(string connectionId, string groupName)
        {
            HubContext.Groups.Add(connectionId, groupName);
        }

        public void RemoveGroup(string connectionId, string groupName)
        {
            HubContext.Groups.Remove(connectionId, groupName);
        }

        public void UpdateJackpot(string groupName, long jackpot)
        {
            HubContext.Clients.Group(groupName).UpdateJackPot(jackpot);
        }

        public void SendMessageToClient(long accountid, string message)
        {
            IReadOnlyList<string> list = Instance.GetConnections(accountid);
            foreach (var str in list)
            {
                Instance.HubContext.Clients.Client(str).message(message, 3);
            }
        }

        public void SendMessageToClient(long accountid, object message)
        {
            IReadOnlyList<string> list = Instance.GetConnections(accountid);
            foreach (var str in list)
            {
                Instance.HubContext.Clients.Client(str).message(message, 3);
            }
        }

        public string PlayerConnect(long accountId, string connection)
        {
            if (accountId < 1 || String.IsNullOrEmpty(connection))
            {
                return string.Empty;
            }
            _mapHubAccount.TryAdd(connection, accountId);

            if (!_mapAccountHub.ContainsKey(accountId))
            {
                List<string> list = new List<string> { connection };
                _mapAccountHub.TryAdd(accountId, list);
            }
            else
            {
                List<string> list = null;
                if (!_mapAccountHub.TryGetValue(accountId, out list))
                {
                    return string.Empty;
                }

                if (!Monitor.TryEnter(list, 2000)) return string.Empty;
                if (list.Count < 1)
                {
                    return string.Empty;
                }
                try
                {
                    string first = list.FirstOrDefault();
                    list.Clear();
                    list.Add(connection);
                    return first;
                }
                finally
                {
                    Monitor.Exit(list);
                }
            }
            return string.Empty;
        }

        public long PlayerDisconnect(string connection)
        {
            if (String.IsNullOrEmpty(connection))
            {
                return -1;
            }
            long accountId = 0;
            _mapHubAccount.TryRemove(connection, out accountId);

            if (!_mapHubAccount.TryGetValue(connection, out accountId)) return accountId;
            List<string> list = null;
            _mapAccountHub.TryGetValue(accountId, out list);
            {
                if (list == null)
                {
                    return accountId;
                }

                if (!Monitor.TryEnter(list, 2000)) return accountId;
                try
                {
                    if (list.Contains(connection))
                    {
                        list.Remove(connection);
                    }
                    if (list.Count == 0)
                    {
                        _mapAccountHub.TryRemove(accountId, out list);
                    }
                }
                finally
                {
                    Monitor.Exit(list);
                }
            }
            return accountId;
        }

        public long GetAccountIdByConnectionId(string connectionId)
        {

            long current = 0;
            _mapHubAccount.TryGetValue(connectionId, out current);
            return current;
        }

        public IReadOnlyList<string> GetConnections(long accountId)
        {
            IReadOnlyList<string> listReturn = new List<string>().AsReadOnly();
            if (accountId < 1 || !_mapAccountHub.ContainsKey(accountId))
            {
                return listReturn;
            }

            List<string> trygetList = null;
            if (!_mapAccountHub.TryGetValue(accountId, out trygetList)) return listReturn;
            return trygetList != null ? trygetList.AsReadOnly() : listReturn;
        }

        public long GetAccountId(string connectionId)
        {
            const long accountId = 0;
            try
            {
                if (string.IsNullOrEmpty(connectionId)) return accountId;
                var lst = _mapHubAccount.ToList();
                var player = lst.FindAll(c => c.Key == connectionId);
                if (player.Count <= 0) return accountId;
                return player.FirstOrDefault().Value;
            }
            catch (Exception exception)
            {
               NLogManager.PublishException(exception);
            }
            return accountId;
        }

        public string Replace(long accountid, string connectionid)
        {
            if (String.IsNullOrEmpty(connectionid) || accountid < 1)
            {
                return string.Empty;
            }
            if (!_mapAccountHub.Keys.Contains(accountid))
            {
                return string.Empty;
            }

            List<string> list = null;
            if (!_mapAccountHub.TryGetValue(accountid, out list)) return string.Empty;
            if (list.Count < 1)
            {
                return string.Empty;
            }

            if (!Monitor.TryEnter(list, 2000)) return string.Empty;
            try
            {
                if (list.Contains(connectionid))
                {
                    return string.Empty;
                }
                else
                {
                    string first = list.FirstOrDefault();
                    list.Clear();
                    list.Add(connectionid);
                    return first;
                }
            }
            finally
            {
                Monitor.Exit(list);
            }

        }
    }
}