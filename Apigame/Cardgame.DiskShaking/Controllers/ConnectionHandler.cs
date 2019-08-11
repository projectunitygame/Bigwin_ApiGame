using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Utilities.Log;

namespace Cardgame.DiskShaking.Controllers
{
    public class ConnectionHandler
    {
        private ConcurrentDictionary<long, List<string>> mapAccountContext = new ConcurrentDictionary<long, List<string>>();

        public List<string> GetAll()
        {
            return mapAccountContext.Values.Select(x => x.AsReadOnly()).SelectMany(x => x).ToList();
        }

        public void PlayerConnect(long accountId, string connectionId)
        {
            List<string> connections = new List<string>();

            if (!mapAccountContext.ContainsKey(accountId))
            {
                mapAccountContext.TryAdd(accountId, connections);
            }
            else
            {
                mapAccountContext.TryGetValue(accountId, out connections);
            }
            ///lock list, trong qua trinh dang su dung, ko co ai update vao list nay ddc
            if (Monitor.TryEnter(connections, 5000))
            {
                try
                {
                    if (!connections.Contains(connectionId))
                    {
                        connections.Add(connectionId);
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
                finally
                {
                    Monitor.Exit(connections);
                }
            }
        }

        public void PlayerDisconnect(long accountId, string connectionId)
        {
            List<string> connections = new List<string>();
            List<string> connections1 = new List<string>();

            if (!mapAccountContext.ContainsKey(accountId))
            {
                return;
            }
            else
            {
                mapAccountContext.TryGetValue(accountId, out connections);
            }
            ///lock list, trong qua trinh dang su dung, ko co ai update vao list nay ddc
            if (Monitor.TryEnter(connections, 5000))
            {
                try
                {
                    if (connections.Contains(connectionId))
                    {
                        connections.Remove(connectionId);
                    }

                    if (connections.Count == 0)
                    {
                        mapAccountContext.TryRemove(accountId, out connections1);
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
                finally
                {
                    Monitor.Exit(connections);
                }
            }
        }

        public IReadOnlyList<string> GetConnections(long accountId)
        {
            List<string> connections = new List<string>();


            mapAccountContext.TryGetValue(accountId, out connections);

            if (connections == null)
                connections = new List<string>();

            return connections.AsReadOnly();
        }
    }
}