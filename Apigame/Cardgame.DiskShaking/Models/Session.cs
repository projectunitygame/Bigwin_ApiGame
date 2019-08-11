using Cardgame.DiskShaking.Container;
using Cardgame.DiskShaking.Controllers;
using Cardgame.DiskShaking.Database;
using Cardgame.DiskShaking.Hubs;
using Cardgame.DiskShaking.Models.Exceptions;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Utilities;
using Utilities.Log;

namespace Cardgame.DiskShaking.Models
{
    public class Session
    {
        public long Id { get; private set; }
        public long BetValue { get; private set; }
        public long SessionId { get; private set; }
        public int TotalPlayer
        {
            get
            {
                if(_players != null)
                    return _players.Count;
                return 0;
            }
        }
        public int MaxPlayer { get; private set; }
        public MoneyType MoneyType { get; private set; }
        public ConcurrentDictionary<int, Player> Sitting
        {
            get;
            private set;
        }
        public long Banker { get; private set; }
        private bool _deactive;
        public RoomType RoomType { get; private set; }
        private ConcurrentDictionary<long, Player> _players;
        private GameManager _gameManager;
        private GameLogic _logic;
        private object _locker;
        private Timer _timer;
        private State _nextState;
        private State _currentState;
        public State CurrentState
        {
            get
            {
                return _currentState;
            }
        }
        private DateTime _lastSync;
        private object _lockerBet;
        public int Result
        {
            get;
            private set;
        }
        IHubContext _hubContext;
        ConnectionHandler _connectionHandler;
        private ConcurrentQueue<long> _alivePlayers;
        private bool _startTimer;
        public int Elapsed
        {
            get
            {
                return Timing.GetElappsed(_currentState) - (int)(DateTime.Now.Subtract(_lastSync).TotalSeconds);
            }
        }
        private object _lockState;
        public ConcurrentQueue<int> History { get; private set; }
        public Session(long id, MoneyType moneyType, RoomType roomType, GameManager gameManager, long betValue)
        {
            this._hubContext = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            this._connectionHandler = GlobalHost.DependencyResolver.Resolve<ConnectionHandler>();
            this._gameManager = gameManager;
            this._locker = new object();
            this._lockerBet = new object();
            this.Id = id;
            this.MoneyType = moneyType;
            this.RoomType = roomType;
            this._players = new ConcurrentDictionary<long, Player>();
            this.Sitting = new ConcurrentDictionary<int, Player>();
            this.MaxPlayer = roomType == RoomType.TWELVE ? 12 : 50;
            if (roomType == RoomType.TWELVE)
                Enumerable.Range(0, 12).Select(i => { return Sitting.TryAdd(i + 1, null); }).ToList();
            else
                Enumerable.Range(0, 13).Select(i => { return Sitting.TryAdd(i + 1, null); }).ToList();
            this.SessionId = -1;
            this._deactive = false;
            this.BetValue = betValue;
            this.Banker = -1;
            this._lastSync = DateTime.Now;
            this.Result = -1;
            _timer = new Timer(new TimerCallback(Update), null, Timeout.Infinite, Timeout.Infinite);
            this._logic = new GameLogic(roomType, moneyType);
            this._alivePlayers = new ConcurrentQueue<long>();
            this.History = new ConcurrentQueue<int>();
            this._lockState = new object();
        }

        public Dictionary<int, BetInfo> GetSumaryBet(long accountId)
        {
            return _logic.GetSumaryBet(accountId);
        }

        public void SellGate(long accountId, Gate gate)
        {
            if (accountId != Banker)
                return;

            if (Monitor.TryEnter(_lockerBet, 5000))
            {
                try
                {
                    if (_currentState == State.SELL)
                    {
                        _logic.SellGate(gate, SessionId);
                        _hubContext.Clients.Group($"room_{Id}").bankerSellGate(gate);
                    }
                }
                finally
                {
                    Monitor.Exit(_lockerBet);
                }
            }
        }

        public void BuyGate(long accountId, string accountName, Gate gate)
        {
            if (accountId == Banker)
                return;

            if (Monitor.TryEnter(_lockerBet, 5000))
            {
                try
                {
                    if (_currentState == State.SELL)
                    {
                        _logic.BuyGate(gate, SessionId, accountId, accountName);
                        _hubContext.Clients.Group($"room_{Id}").userBuyGate(accountId, gate);
                    }
                }
                finally
                {
                    Monitor.Exit(_lockerBet);
                }
            }
        }

        public void Bet(long accountId, string accountName, List<BetGateData> gates)
        {
            if (gates == null || gates.Count == 0)
                return;

            if (Monitor.TryEnter(_lockerBet, 5000))
            {
                try
                {
                    if (_currentState == State.BETTING)
                    {
                        List<BetGateResponse> gateSuccess = new List<BetGateResponse>();
                        List<BetGateResponse> gateFail = new List<BetGateResponse>();

                        if (RoomType == RoomType.TWELVE && accountId == Banker)
                            throw new BankerCantBetException();

                        Player p = GetPlayer(accountId);
                        if (p == null)
                            throw new PlayerNotFoundException();

                        long balance = 0;

                        foreach (var g in gates)
                        {
                            if (g.amount < 1000)
                                continue;

                            long response = -99;
                            long gateSumary = 0;
                            if (RoomType == RoomType.FIFTY)
                                response = _logic.Bet(SessionId, accountId, accountName, g.amount, g.gate, out gateSumary);
                            else
                            {
                                if (_players.ContainsKey(Banker))
                                    response = _logic.Bet(SessionId, accountId, accountName, g.amount, g.gate, out gateSumary, Banker, _players[Banker].AccountName);
                                else
                                {
                                    NextState(State.SHOW_RESULT, true);
                                    response = -9;
                                }
                            }
                            if (response >= 0)
                            {
                                balance = response;
                                p.UpdateBalance(response, MoneyType);
                                gateSuccess.Add(new BetGateResponse
                                {
                                    amount = g.amount,
                                    gate = g.gate,
                                    error = 0,
                                    gateTotal = gateSumary
                                });
                            }
                            else if (response == -100)
                            {
                                gateFail.Add(new BetGateResponse
                                {
                                    amount = g.amount,
                                    gate = g.gate,
                                    error = -7
                                });
                            }
                            else if (response == -51)
                            {
                                gateFail.Add(new BetGateResponse
                                {
                                    amount = g.amount,
                                    gate = g.gate,
                                    error = -8
                                });
                            }
                            else
                            {
                                gateFail.Add(new BetGateResponse
                                {
                                    amount = g.amount,
                                    gate = g.gate,
                                    error = (int)response
                                });
                            }
                        }

                        var lstConnection = _connectionHandler.GetConnections(accountId);
                        _hubContext.Clients.Clients(lstConnection.ToList()).betInfo(gateSuccess, GetSumaryBet(accountId), gateFail, balance);
                        _hubContext.Clients.Group($"room_{Id}", lstConnection.ToArray()).playerBet(accountId, gateSuccess, balance);
                    }
                    else
                    {
                        throw new NotInBettingStateException();
                    }
                }
                finally
                {
                    Monitor.Exit(_lockerBet);
                }
            }
            else
            {
                throw new Exception("CANT ENTER BET LOCK EXCEPTION");
            }
        }

        private void Update(object o)
        {
            switch (_nextState)
            {
                case State.WAITING:
                    Waiting();
                    break;
                case State.SHAKING:
                    Shaking();
                    break;
                case State.BETTING:
                    Betting();
                    break;
                case State.SELL:
                    Sell();
                    break;
                case State.SHOW_RESULT:
                    ShowResult();
                    break;
            }
        }

        public void TryToStart()
        {
            lock (_locker)
            {
                if (!_startTimer && (RoomType == RoomType.FIFTY) || (FigureOutBanker() && RoomType == RoomType.TWELVE && _players.Count > 1))
                {
                    _timer.Change(1000, Timeout.Infinite);
                    _currentState = State.WAITING;
                    _startTimer = true;
                }
            }
        }

        private bool FigureOutBanker()
        {
            if (RoomType == RoomType.TWELVE)
            {
                if(Banker != -1)
                {
                    Player p;
                    if(_players.TryGetValue(Banker, out p))
                    {
                        long balance = this.MoneyType == MoneyType.GOLD ? p.Gold : p.Coin;
                        if (balance <= _logic.GetWinnerMulti(Gate.Even) * 2)
                            Banker = -1;
                        else return true;
                    }
                }

                if(Banker == -1)
                {
                    Player max = null;
                    long balance = -1;
                    foreach(var p in _players)
                    {
                        long balance1 = this.MoneyType == MoneyType.GOLD ? p.Value.Gold : p.Value.Coin;
                        if (balance1 >= balance)
                        {
                            balance = balance1;
                            max = p.Value;
                        }
                    }

                    if(max != null && balance >= _logic.GetWinnerMulti(Gate.Even) * 2)
                    {
                        Banker = max.AccountId;
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }

            return false;
        }

        public void Waiting()
        {
            this.Result = -1;
            this._alivePlayers = new ConcurrentQueue<long>();

            if (Banker < 0 &&  RoomType == RoomType.TWELVE)
                FigureOutBanker();

            Player banker = GetPlayer(Banker);
            this._logic.Refresh(banker);
            if (this._players.Count == 0)
            {
                this._deactive = true;
                this._timer.Change(-1, -1);
                _startTimer = false;
                _gameManager.DeactiveSession(this.Id);
                return;
            }
            else if((this._players.Count == 1 && RoomType != RoomType.FIFTY) || (Banker == -1 && RoomType == RoomType.TWELVE))
            {
                _startTimer = false;
                this._timer.Change(-1, -1);
                return;
            }

            _hubContext.Clients.Group($"room_{Id}").changeState(_nextState, Timing.GetElappsed(_nextState), SessionId, Banker);
            NextState(State.SHAKING);
        }

        public void Ready(long accountId)
        {
            if (_currentState == State.WAITING)
                 this._alivePlayers.Enqueue(accountId);
        }

        public Player GetPlayer(long accountId)
        {
            Player p;
            _players.TryGetValue(accountId, out p);
            return p;
        }

        public int GetSittingPosition(long accountId)
        {
            foreach(var p in Sitting)
            {
                if (p.Value != null && p.Value.AccountId == accountId)
                    return p.Key;
            }
            return 0;
        }

        public void Shaking()
        {
            RemoveDeactivePlayer();
            if (!FigureOutBanker() && RoomType == RoomType.TWELVE)
            {
                _startTimer = false;
                this._timer.Change(-1, -1);
                return;
            }
            SessionId = GameDAO.CreateSession();
            if (SessionId < 0)
                NextState(State.SHAKING);
            else
            {
                _hubContext.Clients.Group($"room_{Id}").changeState(_nextState, Timing.GetElappsed(_nextState), SessionId, Banker);
                NextState(State.BETTING);
            }
        }

        public void Betting()
        {
            _hubContext.Clients.Group($"room_{Id}").changeState(_nextState, Timing.GetElappsed(_nextState), SessionId, Banker);

            if (RoomType == RoomType.FIFTY)
            {
                NextState(State.SHOW_RESULT);
            }
            else
            {
                NextState(State.SELL);
            }
        }

        public void Sell()
        {
            _hubContext.Clients.Group($"room_{Id}").changeState(_nextState, Timing.GetElappsed(_nextState), SessionId, Banker);
            NextState(State.SHOW_RESULT);
        }

        public void ShowResult()
        {
            ThreadPool.QueueUserWorkItem(u =>
            {
                var data = FinishSession();
            });

            NextState(State.WAITING);
        }

        private async Task FinishSession()
        {
            try
            {
                if (Result > 0)
                    return;
                Result = RandomUtil.NextInt(2) + RandomUtil.NextInt(2) + RandomUtil.NextInt(2) + RandomUtil.NextInt(2);
                _logic.CalculateResult(Result, SessionId);
                string data = _logic.GetQuery();
                if (string.IsNullOrEmpty(data))
                    return;
                History.Enqueue(Result);
                if (History.Count > 50)
                    History.TryDequeue(out var r);
                await GameDAO.ExecuteAsync(data);
                NLogManager.LogMessage("FINISH SESION: => " + data);
                var rewards = _logic.GetReward();
                var listSit = Sitting.Values.ToList();
                List<object> sittings = new List<object>();
                foreach (var re in rewards)
                {
                    Player p = GetPlayer(re.Key);

                    if(p != null)
                    {
                        long totalLose = re.Value.Sum(x => x.Lose);
                        long totalwin = re.Value.Sum(x => x.Prize);
                        long totalrefund = re.Value.Sum(x => x.Refund);
                        if (re.Key == Banker)
                            p.IncreaseBalance(totalwin - totalLose, MoneyType);
                        else
                            p.IncreaseBalance(totalwin + totalrefund, MoneyType);
                        if (listSit.Exists(x => x?.AccountId == re.Key))
                        {
                            sittings.Add(new
                            {
                                AccountId = p.AccountId,
                                Balance = MoneyType == MoneyType.GOLD ? p.Gold : p.Coin,
                                TotalLose = totalLose,
                                TotalWin = totalwin,
                                TotalRefund = re.Key == Banker ? 0 : totalrefund,
                                TotalPrize = totalwin
                            });
                        }
                    }
                }

                foreach (var re in rewards)
                {
                    Player p = GetPlayer(re.Key);

                    if (p != null)
                    {
                        long balance = MoneyType == MoneyType.GOLD ? p.Gold : p.Coin;
                        var lstConnection = _connectionHandler.GetConnections(p.AccountId);
                        _hubContext.Clients.Clients(lstConnection.ToList()).showResult(Result, new
                        {
                            winLose = re.Value,
                            balance = balance
                        }, sittings);
                    }
                }

                var player = _players.Where(x => !rewards.ContainsKey(x.Key));

                foreach (var re in player)
                {
                        long balance = MoneyType == MoneyType.GOLD ? re.Value.Gold : re.Value.Coin;
                        var lstConnection = _connectionHandler.GetConnections(re.Value.AccountId);
                        _hubContext.Clients.Clients(lstConnection.ToList()).showResult(Result, new
                        {
                            winLose = new List<Reward>(),
                            balance = balance
                        }, sittings);
                }

            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        private void NextState(State state, bool instant = false)
        {
            lock(_lockState)
            {
                if (_nextState == state)
                    return;

                _lastSync = DateTime.Now;
                _currentState = _nextState;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (instant)
                {
                    _timer.Change(1000, Timeout.Infinite);
                }
                else
                {
                    _timer.Change(Timing.GetElappsed(_nextState) * 1000, Timeout.Infinite);
                }
                _nextState = state;
            }
        }

        public Player Sit(long accountId, int position)
        {
            var player = GetPlayer(accountId);
            if (player == null)
                throw new PlayerNotFoundException();
            lock (_locker)
            {
                if (Sitting.Values.ToList().Exists(x => x != null && x.AccountId == accountId))
                    throw new AlreadySitException();

                if(Sitting[position] == null)
                    Sitting.AddOrUpdate(position, player, (k, v) => v = player);
                else throw new AlreadySitException();

                return player;
            }
        }

        public int AddPlayer(Player player)
        {
            long balance = this.MoneyType == MoneyType.GOLD ? player.Gold : player.Coin;
            if (balance < BetValue)
                throw new NotEnoughMoneyException();
            lock (_locker)
            {
                if (this._deactive)
                    throw new RoomHasBeenDeactiveException();
                if (_players.Count == MaxPlayer)
                    throw new RoomFullException();
                player.RoomId = Id;
                player.SessionId = SessionId;
                _players.AddOrUpdate(player.AccountId, player, (k, v) => v = player);
                if(RoomType == RoomType.TWELVE & Banker == -1)
                {
                    if (balance >= _logic.GetWinnerMulti(Gate.Even) * 2)
                        Banker = player.AccountId;
                }
                this._alivePlayers.Enqueue(player.AccountId);
                foreach (var s in Sitting)
                {
                    if (s.Value == null)
                    {
                        Sitting.AddOrUpdate(s.Key, player, (k, v) => v = player);
                        return s.Key;
                    }
                }
                return 0;
            }
        }

        public void RemovePlayer(Player player, int reason)
        {
            lock (_locker)
            {
                if(_players.TryRemove(player.AccountId, out player))
                {
                    player.LeaveGame();

                    var lstConnection = _connectionHandler.GetConnections(player.AccountId);

                    foreach(var connection in lstConnection)
                        _hubContext.Groups.Remove(connection, $"room_{Id}").Wait();

                    _hubContext.Clients.Clients(lstConnection.ToList()).playerLeave(player.AccountId, reason, 0, 0);

                    bool found = false;

                    foreach (var s in Sitting)
                    {
                        if(s.Value != null && s.Value.AccountId == player.AccountId)
                        {
                            Player p = null;
                            Sitting.AddOrUpdate(s.Key, p, (k, v) => v = p);
                            _hubContext.Clients.Group($"room_{Id}").playerLeave(player.AccountId, reason, TotalPlayer, MaxPlayer);
                            found = true;
                            break;
                        }
                    }

                    if(RoomType == RoomType.TWELVE)
                    {
                        if (player.AccountId == Banker)
                            Banker = -1;
                    }

                    if (!found)
                        _hubContext.Clients.Group($"room_{Id}").ccu(TotalPlayer, MaxPlayer);

                    if (_players.Count == 0 && _currentState == State.WAITING)
                    {
                        this._deactive = true;
                        this._timer.Change(-1, -1);
                        _gameManager.DeactiveSession(this.Id);
                    }else if(_players.Count == 0)
                    {
                        NextState(State.SHOW_RESULT, true);
                    }
                }
            }
        }

        private void RemoveDeactivePlayer()
        {
            var lst = _alivePlayers.Distinct();
            var nonInroom = _players.Where(x => !lst.Contains(x.Key));
            foreach (var p in nonInroom)
            {
                try
                {
                    RemovePlayer(p.Value, 3);
                }catch(Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
            }
        }
    }
}