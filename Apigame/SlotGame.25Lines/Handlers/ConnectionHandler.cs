using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Microsoft.AspNet.SignalR;
using Utilities.Log;


namespace SlotGame._25Lines.Handlers

{
    public class ConnectionHandler
    {
        private static readonly Lazy<ConnectionHandler> _instance =
               new Lazy<ConnectionHandler>(
                   () => new ConnectionHandler());

        private readonly ConcurrentDictionary<string, long> _mapHubAccount = new ConcurrentDictionary<string, long>();

        private readonly ConcurrentDictionary<long, List<string>> _mapAccountHub =
            new ConcurrentDictionary<long, List<string>>();
        
        
        private ConnectionHandler()
        {
          
        }

        public static ConnectionHandler Instance => _instance.Value;

        
        public string PlayerConnect(long accountId, string connection)
        {
            if (accountId < 1 || string.IsNullOrEmpty(connection))
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
                if (!_mapAccountHub.TryGetValue(accountId, out var list))
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
                    var first = list.FirstOrDefault();
                    //list.Clear();
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
            if (string.IsNullOrEmpty(connection))
            {
                return -1;
            }

            _mapHubAccount.TryRemove(connection, out var accountId);

            if (!_mapHubAccount.TryGetValue(connection, out accountId)) return accountId;
            _mapAccountHub.TryGetValue(accountId, out var list);
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

        public IReadOnlyList<string> GetConnections(long accountId)
        {
            IReadOnlyList<string> listReturn = new List<string>().AsReadOnly();
            if (accountId < 1 || !_mapAccountHub.ContainsKey(accountId))
            {
                return listReturn;
            }

            if (!_mapAccountHub.TryGetValue(accountId, out var trygetList)) return listReturn;
            return trygetList?.AsReadOnly() ?? listReturn;
        }
        
    }
}