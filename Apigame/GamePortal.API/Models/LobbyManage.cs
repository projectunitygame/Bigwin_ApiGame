using GamePortal.API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamePortal.API.Models
{
    public static class LobbyManage
    {
        private static ConnectionHandler _connectionLobby = new ConnectionHandler();
        public static void Disconnect(long accountId, string connectionId)
        {
            _connectionLobby.PlayerDisconnect(accountId, connectionId);
        }

        public static void Connect(long accountId, LobbyHub client)
        {
            _connectionLobby.PlayerConnect(accountId, client);
        }

        public static List<string> GetAllConnectionById(long accountId)
        {
            List<string> connections = new List<string>();
            connections.AddRange(_connectionLobby.GetConnections(accountId));
            return connections;
        }

        public static LobbyHub GetClientById(long accountId)
        {
            LobbyHub client = new LobbyHub();
            client = _connectionLobby.GetClient(accountId);
            return client;
        }
    }
}