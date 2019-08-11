using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Hubs;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Log;
using Utilities;
using System.Text;
using System.Xml.Serialization;
using PTCN.CrossPlatform.Minigame.LuckyDice.Handlers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.EventBetKing;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Controllers
{
    public class LuckyDiceGameLoop
    {
        //private static Lazy<LuckyDiceGameLoop> _instance = new Lazy<LuckyDiceGameLoop>(() => new LuckyDiceGameLoop());

        //public static LuckyDiceGameLoop Instance
        //{
        //    get
        //    {
        //        return _instance.Value;
        //    }
        //}

        /// <summary>
        /// Bot Handler
        /// </summary>
        private BotManager _botManager;

        #region game logic
        private object _locker = new object();
        private bool _dateStatus;

        IHubContext Clients = GlobalHost.ConnectionManager.GetHubContext<LuckyDiceHub>();
        /// <summary>
        /// Trang thai hien tai cua session
        /// </summary>

        public GameState CurrentState = GameState.PrepareNewRound;
        /// <summary>
        /// Ma phien
        /// </summary>
        public long SessionID { get; set; }
        /// <summary>
        /// Thoi gian con lai
        /// </summary>
        public int Ellapsed
        {
            get;
            set;
        }
        /// <summary>
        /// Tra ve data khi thay doi luong tien
        /// </summary>
        private bool _returningData { get; set; }

        public int TotalTai
        {
            get
            {
                return BetTai.Count;
            }
        }

        public int TotalXiu
        {
            get
            {
                return BetXiu.Count;
            }
        }

        public long TotalBetTai
        {
            get
            {
                return BetTai.Sum(x => x.Value);
            }
        }

        public long TotalBetXiu
        {
            get
            {
                return BetXiu.Sum(x => x.Value);
            }
        }

        private bool _isBetKingEvent = false;

        public DiceResult Result = new DiceResult();

        private ConcurrentDictionary<long, BetKingState> _eventBetKing = new ConcurrentDictionary<long, BetKingState>();
        private ConcurrentDictionary<long, long> BetTai = new ConcurrentDictionary<long, long>();
        private ConcurrentDictionary<long, long> BetXiu = new ConcurrentDictionary<long, long>();
        private ConcurrentDictionary<long, long> LogSumary = new ConcurrentDictionary<long, long>();

        private ConcurrentStack<BetInfo> _betInfo = new ConcurrentStack<BetInfo>();
        private  ConcurrentStack<BetInfo> _overBets = new ConcurrentStack<BetInfo>();
        private ConcurrentStack<BetInfo> _underBets = new ConcurrentStack<BetInfo>();


        private Timer _sessionTimer;
        private Timer _elapsedTimer;

        public int MoneyType { get; private set; }

        private Cache _cache;
        private ConnectionHandler _connection;
        string _createdTime;

        public LuckyDiceGameLoop(int moneyType, Cache cache, ConnectionHandler connection)
        {
            _botManager = new BotManager();
            if (moneyType == 1)
            {
                _botManager.Start();
            }
            
            MoneyType = moneyType;
            _cache = cache;
            _connection = connection;
            //timer danh cho game loop
            _sessionTimer = new Timer(new TimerCallback(SessionCallBack), null, 3000, -1);
            _elapsedTimer = new Timer(new TimerCallback((o) =>
            {
                if (Ellapsed > 0)
                {
                    Ellapsed--;
                }

                //Debug.WriteLine(JsonConvert.SerializeObject(this));
                ///send object game loop toi tat ca clients 1s 1 phat de dong bo


                if (_returningData && CurrentState == GameState.Betting)
                {
                    _returningData = false;
                    Clients.Clients.Clients(_connection.GetAll()).OnChangeBetting(MoneyType, TotalTai, TotalBetTai, TotalXiu, TotalBetXiu);
                    //Clients.Clients.Clients(_connection.GetAll()).SessionInfo(MoneyType, this);
                }
            }), null, -1, -1);
        }

        private void StartBettingPhrase()
        {
            RefreshSession();
            CurrentState = GameState.Betting;
        }
        private void StartEndBettingPhrase()
        {
            try
            {
                CurrentState = GameState.EndBetting;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
        private void StartShowResultPhrase()
        {
            try
            {
                CurrentState = GameState.ShowResult;

                Result.GetNewResult();
                var totalBotBet = 0l;
                if (_botManager.EnableRunBot() && MoneyType == 1)
                {
                    try
                    {
                        var sumaryTai = _overBets.Sum(x => x.Money);
                        var sumaryXiu = _underBets.Sum(x => x.Money);

                        if (sumaryTai != sumaryXiu)
                        {
                            var diff = Math.Abs(sumaryTai - sumaryXiu);
                            if (sumaryTai > sumaryXiu) // Neu ve tai thi refund can cua
                            {
                                while (diff > 0)
                                {
                                    _overBets.TryPop(out var result);

                                    if (result == null)
                                        continue;

                                    var diff1 = diff - result.Money;

                                    if (diff1 > 0)
                                    {
                                        diff = diff1;
                                        continue;
                                    }

                                    if (diff1 <= 0)
                                    {
                                        _overBets.Push(new BetInfo(){Money = Math.Abs(diff1), IsBotBet = result.IsBotBet});
                                        break;
                                    }
                                }
                            }
                            else // Neu ve xiu thi refund can cua
                            {
                                while (diff > 0)
                                {
                                    _underBets.TryPop(out var result);

                                    if (result == null)
                                        continue;

                                    long diff1 = diff - result.Money;

                                    if (diff1 > 0)
                                    {
                                        diff = diff1;
                                        continue;
                                    }

                                    if (diff1 <= 0)
                                    {
                                        _underBets.Push(new BetInfo() { Money = Math.Abs(diff1), IsBotBet = result.IsBotBet });
                                        break;
                                    }
                                }
                            }
                        }

                        var acceptedOver = _overBets.Sum(x => x.Money); // tổng số tiền cân cửa tài
                        var acceptedUnder = _underBets.Sum(x => x.Money); // tổng số tiền cân cửa xỉu

                        var totalBetTai = _overBets.Where(x => x.IsBotBet == 1).Sum(x => x.Money); // tổng số tiền bot đặt tài khi cân cửa
                        var totalBetXiu = _underBets.Where(x => x.IsBotBet == 1).Sum(x => x.Money); // tổng số tiền bot đặt xỉu khi cân cửa

                        var playerMoneyOver = acceptedOver - totalBetTai;
                        var playerMoneyUnder = acceptedUnder - totalBetXiu;

                        var botMoneyChange = Math.Abs(totalBetTai - totalBetXiu); // số tiền bot cân với người chơi
                        var playerMoneyChange = Math.Abs(playerMoneyOver - playerMoneyUnder); // số tiền người chơi cân với bot                      

                        var botWin = false; // Neu bot thang

                        // Kiếm tra kết quả
                        if (acceptedOver == totalBetTai && acceptedUnder == totalBetXiu)
                        {
                            NLogManager.LogMessage("Players dont play");
                        }
                        else
                        {
                            var sumDice = Result.Dice1 + Result.Dice2 + Result.Dice3;

                            if (sumDice > 10) // Về tài
                            {
                                botWin = true;
                                if (totalBetXiu > totalBetTai) // Nếu bot đặt xỉu nhiều hơn
                                {
                                    botWin = false;
                                    if (_botManager.Fund - botMoneyChange < playerMoneyChange) // Quỹ bot âm
                                    {
                                        while (Result.Dice1 + Result.Dice2 + Result.Dice3 > 10) // Chuyển kết quả về xỉu
                                        {
                                            Result.GetNewResult();
                                        }

                                        botWin = true;
                                    }                                       
                                }
                            }
                            else // Về xỉu
                            {
                                botWin = true;
                                if (totalBetTai > totalBetXiu) // Nếu bot đặt tài nhiều hơn
                                {
                                    botWin = false;
                                    if (_botManager.Fund - botMoneyChange < playerMoneyChange) // Quỹ bot âm
                                    {                                             
                                        while (Result.Dice1 + Result.Dice2 + Result.Dice3 <= 10) // Chuyển kết quả về tài
                                        {
                                            Result.GetNewResult();
                                        }
                                        botWin = true;
                                    }

                                }
                            }
                        }

                        totalBotBet = botWin ? botMoneyChange : -botMoneyChange;

                        NLogManager.LogMessage($"Session:{SessionID}|BotWin:{botWin}|TotalBotBet:{totalBotBet}|acceptedOver:{acceptedOver}|acceptedUnder{acceptedUnder}|botOver{totalBetTai}|botunder:{totalBetXiu}|playerOver:{playerMoneyOver}|playerUnder:{playerMoneyUnder}");
                    }
                    catch (Exception ex)
                    {
                        NLogManager.PublishException(ex);
                    }
                }

                var res = this.Result.Clone() as DiceResult;
                FinishSession(res, s =>
                {
                    _botManager.UpdateFund(s, totalBotBet);
                });
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }
        private void StartPreparePhrase()
        {
            NLogManager.LogMessage("Prepare new round");
            RefreshSession();
            CurrentState = GameState.PrepareNewRound;
        }

        public int Bet(string connectionId, long accountId, string username, string clientIP, BetSide betSide, long betAmount, out long sumaryBet, out long newBalance, out string messageError, bool botBet = false)
        {
            
            newBalance = 0;
            sumaryBet = 0;
            messageError = string.Empty;
            try
            {
                if (Monitor.TryEnter(_locker, 60000))
                {
                    try
                    {
                        if (betAmount <= 100)//toi thieu dat cuoc 100 xu
                        {
                            messageError = "Sai số đặt cược!";
                            return -999;
                        }

                        if (CurrentState != GameState.Betting)
                        {
                            messageError = "Không thể đặt cửa vào lúc này!";
                            return -998;
                        }

                        if (betSide == BetSide.Tai)
                        {
                            if (BetXiu.ContainsKey(accountId))
                            {
                                messageError = "Bạn chỉ có thể đặt một cửa!";
                                return -997;
                            }
                        }
                        else
                        {
                            if (BetTai.ContainsKey(accountId))
                            {
                                messageError = "Bạn chỉ có thể đặt một cửa!";
                                return -996;
                            }
                        }

                        long logId = -1;
                        long logSumId = -1;
                        bool addLog = false;

                        if (betSide == BetSide.Tai)
                            addLog = !BetTai.ContainsKey(accountId);
                        else addLog = !BetXiu.ContainsKey(accountId);

                        if (!addLog)
                            logSumId = LogSumary[accountId];

                        int response = Lddb.Instance.Bet(SessionID, accountId, username, clientIP, (int)betSide, betAmount, out newBalance, out logId, addLog, ref logSumId, botBet, long.Parse($"{_createdTime}{accountId}"));

                        switch (response)
                        {
                            case -1:
                                messageError = "Phiên không tồn tại!";
                                return response;
                            case -51:
                                messageError = "Số dư không đủ!";
                                return response;
                        }

                        if (response < 0)
                        {
                            messageError = "Lỗi không xác định!";
                            return response;
                        }

                        if (MoneyType == 1)
                        {
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                long addedValue = Lddb.Instance.AddRecentBetting(betAmount, accountId);
                                if (addedValue > 0)
                                    _cache.UpdateRecentBetting(accountId, username, addedValue, true);
                            });
                        }

                        if (addLog)
                            LogSumary.AddOrUpdate(accountId, logSumId, (k, v) => v = logSumId);


                        if (betSide == BetSide.Tai)
                        {
                            sumaryBet = BetTai.AddOrUpdate(accountId, betAmount, (k, v) => v += betAmount);
                            _betInfo.Push(new BetInfo { AccountID = accountId, AccountName = username, Money = betAmount, Side = 0, CreatedTime = DateTime.Now, LogId = logId, LogSumId = logSumId, IsBotBet = botBet ? 1 : 0 });
                            _overBets.Push((new BetInfo { AccountID = accountId, AccountName = username, Money = betAmount, Side = 0, CreatedTime = DateTime.Now, LogId = logId, LogSumId = logSumId , IsBotBet = botBet ? 1 : 0}));
                        }
                        else
                        {
                            sumaryBet = BetXiu.AddOrUpdate(accountId, betAmount, (k, v) => v += betAmount);
                            _betInfo.Push(new BetInfo { AccountID = accountId, AccountName = username, Money = betAmount, Side = 1, CreatedTime = DateTime.Now, LogId = logId, LogSumId = logSumId, IsBotBet = botBet ? 1 : 0 });
                            _underBets.Push((new BetInfo { AccountID = accountId, AccountName = username, Money = betAmount, Side = 1, CreatedTime = DateTime.Now, LogId = logId, LogSumId = logSumId, IsBotBet = botBet ? 1 : 0 }));
                        }

                        if (_isBetKingEvent)
                            _eventBetKing.AddOrUpdate(accountId, new BetKingState
                            {
                                ID = $"{_createdTime}{accountId}",
                                Award = 0,
                                Lose = betAmount
                            }, (k, v) => v = new BetKingState
                            {
                                ID = v.ID,
                                Award = v.Award,
                                Lose = v.Lose += betAmount
                            });

                        OnChangedBettingData();

                        return 1;
                    }
                    finally
                    {
                        Monitor.Exit(_locker);
                    }
                }
                else
                {
                    messageError = "Hệ thống của chúng tôi đang bận, xin bạn vui lòng thử lại!";
                    return -995;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                messageError = ex.Message;
                return -994;
            }
        }
        public long GetTotalBet(long accountId, out int side)
        {
            side = -1;

            if (BetTai.ContainsKey(accountId))
            {
                side = (int)BetSide.Tai;
                return BetTai[accountId];
            }
            else if (BetXiu.ContainsKey(accountId))
            {
                side = (int)BetSide.Xiu;
                return BetXiu[accountId];
            }
            return 0;
        }
        private void OnChangedBettingData()
        {
            if (!_returningData) _returningData = true;
        }
        private void SessionCallBack(Object o)
        {
            try
            {
                _elapsedTimer.Change(1000, 1000);
                switch (CurrentState)
                {
                    case GameState.PrepareNewRound:
                        var session = Lddb.Instance.CreateSession(MoneyType);
                        if (session == null)
                            throw new Exception("Init new session error!");
                        _createdTime = $"{DateTime.Now.Year.ToString("D4")}{DateTime.Now.Month.ToString("D2")}{DateTime.Now.Day.ToString("D2")}";
                        if (MoneyType == 1)//event
                            _isBetKingEvent = Lddb.Instance.IsEventBetKing();
                        SessionID = session.SessionId;
                        _dateStatus = session.DateStatus;
                        Ellapsed = Models.Config.TimeConfig[(int)CurrentState];
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                Clients.Clients.Clients(_connection.GetAll()).SessionInfo(MoneyType, this);
                            }
                            catch
                            {

                            }
                        });
                        Thread.Sleep(Models.Config.TimeConfig[(int)CurrentState] * 1000);
                        StartBettingPhrase();
                        StartTimer();
                        break;
                    case GameState.Betting:
                       
                        Ellapsed = Models.Config.TimeConfig[(int)CurrentState];
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                Clients.Clients.Clients(_connection.GetAll()).SessionInfo(MoneyType, this);
                            }
                            catch
                            {

                            }
                        });
                        //BotInstance.StartBotTimer();
                        if(MoneyType == 1)
                            _botManager.Bet();
                        Thread.Sleep(Models.Config.TimeConfig[(int)CurrentState] * 1000);
                        StartEndBettingPhrase();
                        StartTimer();
                        break;
                    case GameState.EndBetting:
                        Ellapsed = Models.Config.TimeConfig[(int)CurrentState];
                        //ThreadPool.QueueUserWorkItem(_ => {
                        //    try
                        //    {
                        //        Clients.Clients.Clients(_connection.GetAll()).SessionInfo(MoneyType, this);
                        //    }
                        //    catch
                        //    {

                        //    }
                        //});
                        Thread.Sleep(Models.Config.TimeConfig[(int)CurrentState] * 1000);

                        StartShowResultPhrase();

                        StartTimer();
                        break;
                    case GameState.ShowResult:
                        Ellapsed = Models.Config.TimeConfig[(int)CurrentState];
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            try
                            {
                                Clients.Clients.Clients(_connection.GetAll()).SessionInfo(MoneyType, this);
                            }
                            catch
                            {

                            }
                        });
                        Thread.Sleep(Models.Config.TimeConfig[(int)CurrentState] * 1000);
                        StartPreparePhrase();
                        StartTimer();
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
                StartPreparePhrase();
                StartTimer();
            }
        }
        /// <summary>
        /// Start timer, dueTime: seconds
        /// </summary>
        /// <param name="dueTime"></param>
        private void StartTimer()
        {
            //param 2 = -1 => ko lap lai timer, chay xong 1 lan la stop ko chay nua, neu != -1 thi no se lap lai sau .
            _sessionTimer.Change(0, -1);
        }
        private void StopTimer()
        {
            _sessionTimer.Change(-1, -1);
        }
        private void RefreshSession()
        {
            BetTai.Clear();
            _eventBetKing.Clear();
            BetXiu.Clear();
            Result.ClearResult();
            _betInfo.Clear();
            _overBets.Clear();
            _underBets.Clear();
            LogSumary.Clear();
        }
        #endregion
        private void FinishSession(DiceResult Result , Action<long> BotFundUpdate)
        {
            Result.SessionId = SessionID;
            List<BetResultInfo> resultInfo = new List<BetResultInfo>();
            int diceResult = Result.Dice1 + Result.Dice2 + Result.Dice3;
            long totalAccountOdd = TotalXiu;
            long totalAccountEven = TotalTai;
            long totalBetOdd = TotalBetXiu;
            long totalBetEven = TotalBetTai;

            StringBuilder strQuery = new StringBuilder();
            strQuery.AppendLine("begin transaction");
            strQuery.AppendLine("begin try");

            int diceSide = diceResult >= 11 ? 0 : 1;

            long betMoneySideWin = diceSide == 0 ? totalBetEven : totalBetOdd;
            long betMoneySideLose = diceSide == 0 ? totalBetOdd : totalBetEven;

            strQuery.AppendLine($"update dbo.[LuckyDice.Session] set TotalAccountEven = {totalAccountEven}, TotalAccountOdd = {totalAccountOdd}, " +
                $"TotalMoneyEven = {totalBetEven}, TotalMoneyOdd = {totalBetOdd}, IsFinish = 1, " +
                $"FirstDice = {Result.Dice1}, SecondDice = {Result.Dice2}, ThirdDice = {Result.Dice3} " +
                $"where SessionId = {SessionID}");

            int dateStatus = _dateStatus == true ? 1 : 0;

            if (totalAccountOdd != 0 || totalAccountEven != 0)
            {
                long diff = Math.Abs(totalBetEven - totalBetOdd);

                while (!_betInfo.IsEmpty)
                {
                    BetInfo output = null;
                    long amount = 0;
                    long refundMoney = 0;
                    long prizeValue = 0;
                    long gainedPrize = 0;

                    if (_betInfo.TryPop(out output))
                    {
                        amount = output.Money;
                        if (diceSide == output.Side)
                        {
                            if (diff > 0 && betMoneySideWin > betMoneySideLose)
                            {
                                if (diff >= amount)
                                {
                                    diff -= amount;
                                    refundMoney = amount;
                                }
                                else if (diff < amount)
                                {
                                    refundMoney = diff;
                                    prizeValue = (long)Math.Floor((amount - diff) * 2 - (amount - diff) * 0.02);
                                    diff = 0;
                                    gainedPrize = (amount - diff);
                                }
                                else
                                {
                                    prizeValue = (long)Math.Floor(amount * 2 - amount * 0.02);
                                    gainedPrize = amount;
                                }

                                if (_isBetKingEvent)
                                {
                                    _eventBetKing[output.AccountID].Lose -= (refundMoney + gainedPrize);
                                    _eventBetKing[output.AccountID].Award += prizeValue;
                                    _eventBetKing[output.AccountID].Gained += gainedPrize;
                                }

                                strQuery.AppendLine($"exec LD_UpdateResult @_MoneyType = {MoneyType}, @_AccountId = {output.AccountID}, @_SessionId = {SessionID}, @_Dice1 = {Result.Dice1}, " +
                                    $"@_Dice2 = {Result.Dice2}, @_Dice3 = {Result.Dice3}, @_DateStatus = {dateStatus}, " +
                                    $"@_IsBotBet = {output.IsBotBet}, @_Refund = {refundMoney}, @_Prize = {prizeValue}, @_LogId = {output.LogId}, @_LogSumaryId = {output.LogSumId}");

                                var result1 = resultInfo.FirstOrDefault(x => x.AccountId == output.AccountID);
                                if (result1 != null)
                                {
                                    result1.Refund += refundMoney;
                                    result1.Bet += amount;
                                    result1.Award += prizeValue;
                                }
                                else
                                {
                                    resultInfo.Add(new BetResultInfo
                                    {
                                        AccountId = output.AccountID,
                                        AccountName = output.AccountName,
                                        Bet = amount,
                                        BetSide = output.Side,
                                        Refund = refundMoney,
                                        Award = prizeValue,
                                        CreatedTime = output.CreatedTime,
                                    });
                                }

                                continue;
                            }

                            prizeValue = (long)Math.Floor(amount * 2 - amount * 0.02);
                            gainedPrize = amount;

                            var result = resultInfo.FirstOrDefault(x => x.AccountId == output.AccountID);
                            if (result != null)
                            {
                                result.Refund += refundMoney;
                                result.Bet += amount;
                                result.Award += prizeValue;
                            }
                            else
                            {
                                resultInfo.Add(new BetResultInfo
                                {
                                    AccountId = output.AccountID,
                                    AccountName = output.AccountName,
                                    Bet = amount,
                                    BetSide = output.Side,
                                    Refund = refundMoney,
                                    Award = prizeValue,
                                    CreatedTime = output.CreatedTime,
                                });
                            }

                            if (_isBetKingEvent)
                            {
                                _eventBetKing[output.AccountID].Lose -= (refundMoney + gainedPrize);
                                _eventBetKing[output.AccountID].Award += prizeValue;
                                _eventBetKing[output.AccountID].Gained += gainedPrize;
                            }

                            strQuery.AppendLine($"exec LD_UpdateResult @_MoneyType = {MoneyType}, @_AccountId = {output.AccountID}, @_SessionId = {SessionID}, @_Dice1 = {Result.Dice1}, " +
                                $"@_Dice2 = {Result.Dice2}, @_Dice3 = {Result.Dice3}, @_DateStatus = {dateStatus}, " +
                                $"@_IsBotBet = {output.IsBotBet}, @_Refund = {refundMoney}, @_Prize = {prizeValue}, @_LogId = {output.LogId}, @_LogSumaryId = {output.LogSumId}");
                        }
                        else
                        {
                            if (diff > 0 && betMoneySideLose > betMoneySideWin)
                            {
                                if (diff >= amount)
                                {
                                    diff -= amount;
                                    refundMoney = amount;
                                }
                                else if (diff < amount)
                                {
                                    refundMoney = diff;
                                    diff = 0;
                                }

                                var result = resultInfo.FirstOrDefault(x => x.AccountId == output.AccountID);
                                if (result != null)
                                {
                                    result.Refund += refundMoney;
                                    result.Bet += amount;
                                    result.Award += prizeValue;
                                }
                                else
                                {
                                    resultInfo.Add(new BetResultInfo
                                    {
                                        AccountId = output.AccountID,
                                        AccountName = output.AccountName,
                                        Bet = amount,
                                        BetSide = output.Side,
                                        Refund = refundMoney,
                                        Award = prizeValue,
                                        CreatedTime = output.CreatedTime,
                                    });
                                }

                                if (_isBetKingEvent)
                                {
                                    _eventBetKing[output.AccountID].Lose -= (refundMoney + gainedPrize);
                                    _eventBetKing[output.AccountID].Award += prizeValue;
                                    _eventBetKing[output.AccountID].Gained += (amount - refundMoney);
                                }

                                strQuery.AppendLine($"exec LD_UpdateResult @_MoneyType = {MoneyType}, @_AccountId = {output.AccountID}, @_SessionId = {SessionID}, @_Dice1 = {Result.Dice1}, " +
                                    $"@_Dice2 = {Result.Dice2}, @_Dice3 = {Result.Dice3}, @_DateStatus = {dateStatus}, " +
                                    $"@_IsBotBet = {output.IsBotBet}, @_Refund = {refundMoney}, @_Prize = {prizeValue}, @_LogId = {output.LogId}, @_LogSumaryId = {output.LogSumId}");
                            }
                            else
                            {

                                var result = resultInfo.FirstOrDefault(x => x.AccountId == output.AccountID);
                                if (result != null)
                                {
                                    result.Refund += refundMoney;
                                    result.Bet += amount;
                                    result.Award += prizeValue;
                                }
                                else
                                {
                                    resultInfo.Add(new BetResultInfo
                                    {
                                        AccountId = output.AccountID,
                                        AccountName = output.AccountName,
                                        Bet = amount,
                                        BetSide = output.Side,
                                        Refund = refundMoney,
                                        Award = prizeValue,
                                        CreatedTime = output.CreatedTime,
                                    });
                                }

                                strQuery.AppendLine($"exec LD_UpdateResult @_MoneyType = {MoneyType}, @_AccountId = {output.AccountID}, @_SessionId = {SessionID}, @_Dice1 = {Result.Dice1}, " +
                                    $"@_Dice2 = {Result.Dice2}, @_Dice3 = {Result.Dice3}, @_DateStatus = {dateStatus}, " +
                                    $"@_IsBotBet = {output.IsBotBet}, @_Refund = {refundMoney}, @_Prize = {prizeValue}, @_LogId = {output.LogId}, @_LogSumaryId = {output.LogSumId}");

                                if (_isBetKingEvent)
                                {
                                    _eventBetKing[output.AccountID].Lose -= (refundMoney + gainedPrize);
                                    _eventBetKing[output.AccountID].Award += prizeValue;
                                    _eventBetKing[output.AccountID].Gained += (amount - refundMoney);
                                }
                            }
                        }
                    }
                }

                if (_isBetKingEvent)
                {
                    foreach (var data in _eventBetKing.Values)
                    {
                        if ((data.Award > 0 || data.Lose > 0) && data.Gained >= 5000)
                        {
                            strQuery.AppendLine($"exec [event].[UpdateResult] @_Id = {data.ID}, @_Award = {data.Award}, @_Lose = {data.Lose}, @_Gained = {data.Gained}");
                        }
                    }
                }
            }

            //strQuery.AppendLine($"DELETE FROM [dbo].[LuckyDice.Bet] WHERE SessionID = {SessionID}");
            strQuery.AppendLine("commit transaction");

            strQuery.AppendLine("end try");
            strQuery.AppendLine("begin catch");
            strQuery.AppendLine("if @@trancount > 0 begin rollback transaction end;");
            strQuery.AppendLine("throw 50000, 'sql exception', 1");
            strQuery.AppendLine("end catch");

            resultInfo.Reverse();
            NLogManager.LogMessage($"Update result => {strQuery.ToString()}" );
            _cache.PushResult(Result);

            _cache.AddSession(new SessionInfo
            {
                Result = Result,
                BetList = resultInfo
            });

            ThreadPool.QueueUserWorkItem((e) =>
            {
                var lstResponse = Lddb.Instance.FinishSession(strQuery.ToString());
                if (lstResponse)
                {
                    if (MoneyType == 1 && _botManager.EnableRunBot())
                        BotFundUpdate(SessionID);
                    foreach (var user in resultInfo)
                    {
                        if (user.Award + user.Refund > 0)
                        {
                            if (user.Award > 0)
                            {
                                _cache.UpdateCachedRank(new Rank { AccountName = user.AccountName, Award = user.Award }, MoneyType, dateStatus);
                            }
                            ///notice ve client
                            var connection = GameManager.GetAllConnectionById(user.AccountId);
                            if (connection != null && connection.Count > 0)
                                Clients.Clients.Clients(connection.ToList()).WinResult(new { MoneyType = MoneyType, Award = user.Award, Refund = user.Refund });
                        }
                        else
                        {
                            var connection = GameManager.GetAllConnectionById(user.AccountId);
                            if (connection != null && connection.Count > 0)
                                Clients.Clients.Clients(connection.ToList()).WinResult(new { MoneyType = MoneyType, Award = 0, Refund = 0 });
                        }
                    }
                }
            });
        }
        #region gamebot

        #endregion
    }

    public class SessionInfo
    {
        public DiceResult Result { get; set; }
        public List<BetResultInfo> BetList { get; set; }
    }

    public class BetResultInfo
    {
        [JsonIgnore, XmlIgnore]
        public long AccountId { get; set; }
        public DateTime CreatedTime { get; set; }
        public string AccountName { get; set; }
        public long Bet { get; set; }
        [JsonIgnore, XmlIgnore]
        public long Award { get; set; }
        public long Refund { get; set; }
        public int BetSide { get; set; }
    }

    public class BetInfo
    {
        public long AccountID { get; set; }
        public string AccountName { get; set; }
        public long Money { get; set; }
        public int Side { get; set; }
        [JsonIgnore]
        public int IsBotBet { get; set; }
        public DateTime CreatedTime { get; set; }
        public long LogId { get; set; }
        public long LogSumId { get; set; }
    }

    public class DiceResult : ICloneable
    {
        public int Dice1 { get; set; }
        public int Dice2 { get; set; }
        public int Dice3 { get; set; }
        public long SessionId { get; set; }
        public DiceResult()
        {
            ClearResult();
        }
        public void ClearResult()
        {
            Dice1 = Dice2 = Dice3 = -1;
        }
        public void GetNewResult()
        {
            Dice1 = 1 + RandomUtil.NextByte(6);
            Dice2 = 1 + RandomUtil.NextByte(6);
            Dice3 = 1 + RandomUtil.NextByte(6);
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}