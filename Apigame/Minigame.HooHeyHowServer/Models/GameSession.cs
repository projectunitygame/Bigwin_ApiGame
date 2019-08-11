using Microsoft.AspNet.SignalR;
using Minigame.HooHeyHowServer.Hubs;
using Minigame.HooHeyHowServer.Models.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace Minigame.HooHeyHowServer.Models
{
    public class GameSession
    {
        public static GameSession Session { get; private set; }

        IHubContext Clients;
        private GameLogic _logicGold;
        private GameLogic _logicCoin;
        private Timer _timer;
        private Timer _timerSync;
        private object _locker;
        private GameState _nextState;
        private GameState _currentState;
        private DateTime _lastSync;
        private bool _initFlag;
        private StringBuilder _query;

        public long SessionId { get; private set; }
        public int Elapsed
        {
            get
            {
                return Timing.GetElappsed(_currentState) - (int)(DateTime.Now.Subtract(_lastSync).TotalSeconds);
            }
        }
        public GameResult Result { get; private set; }
        public GameState State
        {
            get
            {
                return _currentState;
            }
        }
        private bool _synchBetGold;
        private bool _synchBetCoin;
        private ConnectionHandler _connectionGold;
        private ConnectionHandler _connectionCoin;

        public static void Init()
        {
            Session = new GameSession();
        }

        public GameSession()
        {
            Clients = GlobalHost.ConnectionManager.GetHubContext<HooHeyHowHub>();
            _connectionGold = new ConnectionHandler();
            _connectionCoin = new ConnectionHandler();
            Result = new GameResult();
            _query = new StringBuilder();
            _lastSync = DateTime.Now;
            _locker = new object();
            _synchBetGold = false;
            _synchBetCoin = false;
            _nextState = GameState.PREPAIRING;
            _currentState = GameState.PREPAIRING;
            SessionId = -1;
            _logicGold = new GameLogic(MoneyType.GOLD, GameDAO.GetFund(MoneyType.GOLD));
            _logicCoin = new GameLogic(MoneyType.COIN, GameDAO.GetFund(MoneyType.COIN));
            _timer = new Timer(new TimerCallback(Update), null, 3000, Timeout.Infinite);
            _timerSync = new Timer(new TimerCallback(Sync), null, 3000, 1000);
            _initFlag = true;
        }

        public GameLogic GetLogic(MoneyType moneyType)
        {
            if (moneyType == MoneyType.GOLD)
                return _logicGold;
            else return _logicCoin;
        }

        public void Disconnect(long accountId, string connectionId)
        {
            _connectionGold.PlayerDisconnect(accountId, connectionId);
            _connectionCoin.PlayerDisconnect(accountId, connectionId);
        }

        public void Connect(MoneyType moneyType, long accountId, string connectionId)
        {
            if (moneyType == MoneyType.GOLD)
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

        public long Bet(MoneyType moneyType, long accountId, string accountName, string betData)
        {
            if (Monitor.TryEnter(_locker, 5000))
            {
                try
                {
                    if (_currentState == GameState.BETTING)
                    {
                        long betResult = -99;
                        if (moneyType == MoneyType.GOLD)
                            betResult = _logicGold.Bet(SessionId, accountId, accountName, betData);
                        else betResult = _logicCoin.Bet(SessionId, accountId, accountName, betData);
                        if (moneyType == MoneyType.GOLD)
                        {
                            _synchBetGold = true;
                            if (betResult >= 0)
                                Clients.Clients.Clients(_connectionGold.GetConnections(accountId).ToList()).betSuccess(_logicGold.GetPlayerBettingInfo(accountId), betResult, moneyType);
                            else Clients.Clients.Clients(_connectionGold.GetConnections(accountId).ToList()).errorCode(betResult);
                        }
                        else
                        {
                            _synchBetCoin = true;
                            if (betResult >= 0)
                                Clients.Clients.Clients(_connectionCoin.GetConnections(accountId).ToList()).betSuccess(_logicCoin.GetPlayerBettingInfo(accountId), betResult, moneyType);
                            else Clients.Clients.Clients(_connectionCoin.GetConnections(accountId).ToList()).errorCode(betResult);
                        }
                        return betResult;
                    }
                    else
                    {
                        if (moneyType == MoneyType.COIN)
                        {
                            Clients.Clients.Clients(_connectionCoin.GetConnections(accountId).ToList()).errorCode(-1);
                        }
                        else
                        {
                            Clients.Clients.Clients(_connectionGold.GetConnections(accountId).ToList()).errorCode(-1);
                        }
                        return -1;
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
                finally
                {
                    Monitor.Exit(_locker);
                }
            }
            if (moneyType == MoneyType.COIN)
            {
                Clients.Clients.Clients(_connectionCoin.GetConnections(accountId).ToList()).errorCode(-99);
            }
            else
            {
                Clients.Clients.Clients(_connectionGold.GetConnections(accountId).ToList()).errorCode(-99);
            }
            return -99;
        }

        private List<string> GetAllConnectionById(long accountId)
        {
            List<string> connections = new List<string>();
            connections.AddRange(_connectionCoin.GetConnections(accountId));
            connections.AddRange(_connectionGold.GetConnections(accountId));
            return connections;
        }

        private void Sync(object o)
        {
            if (_synchBetGold)
            {
                Clients.Clients.Clients(_connectionGold.GetAll()).updateBetting(_logicGold.BetGates, _logicGold.BetGateCount);
                _synchBetGold = false;
            }

            if (_synchBetCoin)
            {
                Clients.Clients.Clients(_connectionCoin.GetAll()).updateBetting(_logicCoin.BetGates, _logicCoin.BetGateCount);
                _synchBetCoin = false;
            }
        }

        private void Update(object o)
        {
            if (_nextState == GameState.PREPAIRING)
                Preparing();
            else if (_nextState == GameState.SHAKING)
                Shaking();
            else Betting();
        }

        public Dictionary<int, BetLog> GetSessionReward(long accountId, MoneyType moneyType)
        {
            if (!_initFlag && State == GameState.PREPAIRING)
            {
                if (moneyType == MoneyType.GOLD)
                {
                    var data = _logicGold.GetAwards();
                    if (data != null)
                    {
                        Dictionary<int, BetLog> result;
                        if (data.TryGetValue(accountId, out result))
                            return result;
                    }
                }
                else
                {
                    var data = _logicCoin.GetAwards();
                    if (data != null)
                    {
                        Dictionary<int, BetLog> result;
                        if (data.TryGetValue(accountId, out result))
                            return result;
                    }
                }
            }

            return null;
        }

        private void Preparing()
        {
            //Debug.WriteLine("START PREPAIR");
            if (_initFlag)
            {
                NextState(GameState.SHAKING);
                _initFlag = false;
                return;
            }

            ThreadPool.QueueUserWorkItem(u =>
            {
                var data = FinishSession();
            });

            NextState(GameState.SHAKING);
        }

        private async Task FinishSession()
        {
            try
            {
                Result.GenerateResult();
                _query.Clear();
                _query.AppendLine("begin transaction");
                _query.AppendLine("begin try");
                _query.AppendLine(_logicGold.BuildResult(Result));
                _query.AppendLine(_logicCoin.BuildResult(Result));
                _query.AppendLine($"update dbo.Session set Status = 1, Dice1 = {(int)Result.Dice1}, Dice2 = {(int)Result.Dice2}, Dice3 = {(int)Result.Dice3} where SessionId = {SessionId}");
                _query.AppendLine("commit transaction");
                _query.AppendLine("end try");
                _query.AppendLine("begin catch");
                _query.AppendLine("if @@trancount > 0 begin rollback transaction end;");
                _query.AppendLine("throw 50000, 'sql exception', 1");
                _query.AppendLine("end catch");

                await GameDAO.ExecuteAsync(_query.ToString());

                var award = _logicGold.GetAwards();
                var awardCoin = _logicCoin.GetAwards();

                List<string> excludeConnection = new List<string>(
                        award.SelectMany(x => GetAllConnectionById(x.Key))
                            .Union(awardCoin.SelectMany(x => GetAllConnectionById(x.Key)))
                    );

                List<long> winner = new List<long>(
                        award.Select(x => x.Key).Union(awardCoin.Select(x => x.Key)).Distinct()
                    );

                var exclude = excludeConnection.Distinct();
                List<string> nonResultUser = new List<string>();
                nonResultUser.AddRange(_connectionGold.GetAll().Union(_connectionCoin.GetAll()).Except(exclude));
                await Clients.Clients.Clients(nonResultUser).showResult(Elapsed, Result, null, null);
                foreach (var w in winner)
                {
                    Dictionary<int, BetLog> gold;
                    Dictionary<int, BetLog> coin;
                    award.TryGetValue(w, out gold);
                    awardCoin.TryGetValue(w, out coin);
                    await Clients.Clients.Clients(GetAllConnectionById(w)).showResult(Elapsed, Result, gold, coin);
                }
            }
            catch (Exception ex)
            {
                _logicGold.ReloadRealFund();
                _logicCoin.ReloadRealFund();
                NLogManager.PublishException(ex);
            }
        }

        private void Shaking()
        {
            //Debug.WriteLine("START SHAKING");
            _logicCoin.Restart();
            _logicGold.Restart();
            Result.Refresh();
            SessionId = GameDAO.CreateSession();

            if (SessionId < 0)
                NextState(GameState.SHAKING);
            else
            {
                Clients.Clients.Clients(_connectionCoin.GetAll().Union(_connectionGold.GetAll()).Distinct().ToList()).changeState(_nextState, Timing.GetElappsed(_nextState), SessionId);
                NextState(GameState.BETTING);
            }
        }

        private void Betting()
        {
            //Debug.WriteLine("START BETTING");
            Clients.Clients.Clients(_connectionCoin.GetAll().Union(_connectionGold.GetAll()).Distinct().ToList()).changeState(_nextState, Timing.GetElappsed(_nextState), SessionId);
            NextState(GameState.PREPAIRING);
        }

        private void NextState(GameState state)
        {
            _lastSync = DateTime.Now;
            _currentState = _nextState;
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Change(Timing.GetElappsed(_nextState) * 1000, Timeout.Infinite);
            _nextState = state;
        }
    }
}