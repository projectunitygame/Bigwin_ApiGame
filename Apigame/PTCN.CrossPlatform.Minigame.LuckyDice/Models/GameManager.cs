using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models
{
    public static class GameManager
    {
        private static ConnectionHandler _connectionGold = new ConnectionHandler();
        private static ConnectionHandler _connectionCoin = new ConnectionHandler();
        public static Cache _cacheGold = new Cache(true);
        public static Cache _cacheCoin = new Cache(false);
        private static LuckyDiceGameLoop _gameLoopGold = new LuckyDiceGameLoop(1, _cacheGold, _connectionGold);
        private static LuckyDiceGameLoop _gameLoopCoin;// = new LuckyDiceGameLoop(2, _cacheCoin, _connectionCoin);

        public static List<ChatMessage> GetRecentMessage()
        {
            return _cacheGold.GetMessage();
        }
        public static int CheckEnableChat(long accountId, string accountName, int userType, string message, out ChatMessage msg)
        {
            return _cacheGold.CheckEnableChat(accountId, accountName, message, userType, out msg);
        }

        public static void Init()
        {

        }

        public static void Disconnect(long accountId, string connectionId)
        {
            _connectionGold.PlayerDisconnect(accountId, connectionId);
            _connectionCoin.PlayerDisconnect(accountId, connectionId);
        }

        public static void Connect(int moneyType, long accountId, string connectionId)
        {
            if (moneyType == 1)
            {
                _connectionGold.PlayerConnect(accountId, connectionId);
                _connectionCoin.PlayerDisconnect(accountId, connectionId);
            }
            else
            {
                _connectionCoin.PlayerConnect(accountId, connectionId);
                _connectionGold.PlayerDisconnect(accountId, connectionId);
            }
        }

        public static List<string> GetAllConnectionById(long accountId)
        {
            List<string> connections = new List<string>();
            connections.AddRange(_connectionCoin.GetConnections(accountId));
            connections.AddRange(_connectionGold.GetConnections(accountId));
            return connections;
        }

        public static long GetTotalBet(int moneyType, long accountId, out int side)
        {
            if(moneyType == 1)
            {
                return _gameLoopGold.GetTotalBet(accountId, out side);
            }
            else
            {
                return _gameLoopCoin.GetTotalBet(accountId, out side);
            }
        }

        public static LuckyDiceGameLoop GetGameLoop(int moneyType)
        {
            if (moneyType == 1)
                return _gameLoopGold;
            else return _gameLoopCoin;
        }

        public static int Bet(int moneyType, string connectionId, long accountId, string username, string clientIP, BetSide betSide, long betAmount, out long sumaryBet, out long newBalance, out string messageError, bool botBet = false)
        {
            if (moneyType == 1)
                return _gameLoopGold.Bet(connectionId, accountId, username, clientIP, betSide, betAmount, out sumaryBet, out newBalance, out messageError);
            else return _gameLoopCoin.Bet(connectionId, accountId, username, clientIP, betSide, betAmount, out sumaryBet, out newBalance, out messageError);
        }

        public static List<DiceResult> GetRecentResult(int moneyType)
        {
            if (moneyType == 1)
                return _cacheGold.GetRecentResult(moneyType);
            else return _cacheCoin.GetRecentResult(moneyType);
        }

        public static List<Rank> GetRank(int moneyType)
        {
            if (moneyType == 1)
                return _cacheGold.GetRanks(moneyType);
            else return _cacheCoin.GetRanks(moneyType);
        }

        public static SessionInfo GetSessionInfo(int moneyType, long sessionId)
        {
            if (moneyType == 1)
                return _cacheGold.GetRecentSessionInfo(sessionId, moneyType);
            else return _cacheCoin.GetRecentSessionInfo(sessionId, moneyType);
        }
    }
}