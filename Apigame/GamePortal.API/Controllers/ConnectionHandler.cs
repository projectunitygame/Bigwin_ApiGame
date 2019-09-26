using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Utilities.Log;

namespace GamePortal.API.Controllers
{
    public class ConnectionHandler
    {
        private ConcurrentDictionary<long, List<string>> mapAccountContext = new ConcurrentDictionary<long, List<string>>();
        private ConcurrentDictionary<long, LobbyHub> _ListClients = new ConcurrentDictionary<long, LobbyHub>();
        public List<string> GetAll()
        {
            return mapAccountContext.Values.Select(x => x.AsReadOnly()).SelectMany(x => x).ToList();
        }

        public void PlayerConnect(long accountId, LobbyHub client)
        {
            List<string> connections = new List<string>();

            if (!mapAccountContext.ContainsKey(accountId))
            {
                mapAccountContext.TryAdd(accountId, connections);
                _ListClients.TryAdd(accountId, client);
            }
            else
            {
                //đã tồn tại lấy connectionID
                mapAccountContext.TryGetValue(accountId, out connections);
                LobbyHub c = new LobbyHub();
                _ListClients.TryGetValue(accountId, out c);
                if (c != null)
                {
                    c.Disconnect();
                    _ListClients.TryRemove(accountId, out c);
                    //PlayerDisconnect(accountId, c.Context.ConnectionId);
                }
                _ListClients.TryAdd(accountId, client);
            }
            ///lock list, trong qua trinh dang su dung, ko co ai update vao list nay ddc
            if (Monitor.TryEnter(connections, 5000))
            {
                try
                {
                    if (!connections.Contains(client.connectionID))
                    {
                        connections.Add(client.connectionID);
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
            try
            {
                NLogManager.LogMessage("PlayerDisconnect: " + accountId + ", " + connectionId);
                List<string> connections = new List<string>();
                List<string> connections1 = new List<string>();
                LobbyHub accountHub = new LobbyHub();
                if (!mapAccountContext.ContainsKey(accountId))
                {
                    return;
                }
                else
                {
                    mapAccountContext.TryGetValue(accountId, out connections);
                    //var client = GetClient(accountId);
                    //if (client != null)
                    //{
                    //    client.Clients.Caller.KickUser("kick user");
                    //    NLogManager.LogMessage("send KickUser client success");
                    //}
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
                            _ListClients.TryRemove(accountId, out accountHub);
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
            catch (Exception)
            {

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

        public LobbyHub GetClient(long accountId)
        {
            LobbyHub client = new LobbyHub();
            _ListClients.TryGetValue(accountId, out client);

            return client;
        }
    }
}