using Cardgame.DiskShaking.Database;
using Cardgame.DiskShaking.Models.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;

namespace Cardgame.DiskShaking.Models
{
    public class GameLogic
    {
        private RoomType _roomType;
        private MoneyType _moneyType;
        private decimal _fee;
        ConcurrentDictionary<int, GateBet> _bets;
        ConcurrentDictionary<long, List<Reward>> _rewards;
        StringBuilder _query;
        List<BetLog> _bankerLocks;
        private Player _banker;
        private bool _calculated;
        public GameLogic(RoomType roomType, MoneyType moneyType)
        {
            this._fee = 0.98M;
            this._roomType = roomType;
            this._moneyType = moneyType;
            this._rewards = new ConcurrentDictionary<long, List<Reward>>();
            this._query = new StringBuilder();
            this._bets = new ConcurrentDictionary<int, GateBet>();
            this._bankerLocks = new List<BetLog>();
            Refresh();
        }

        public ConcurrentDictionary<long, List<Reward>> GetReward()
        {
            return DeepClone<ConcurrentDictionary<long, List<Reward>>>(_rewards); ;
        }

        private T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public long Bet(long sessionId, long accountId, string accountName, long amount, Gate gate, out long gateSumary, long banker = -1, string bankerName = "", bool skipDAO = false)
        {
            if (_roomType == RoomType.FIFTY)
                return BetNornal(sessionId, accountId, accountName, amount, gate, out gateSumary, skipDAO);
            else return BetWithBanker(sessionId, accountId, accountName, amount, gate, banker, bankerName, out gateSumary, skipDAO);
        }
        
        public long SellGate(Gate gate, long sessionId)
        {
            if (gate != Gate.Even && gate != Gate.Odd)
                throw new SellGateException();

            string money = _moneyType == MoneyType.GOLD ? "Vàng" : "Xu";
            var g = _bets[(int)gate];
            if (g.GateState == -1)
            {
                long refundAmount = g.Logs.Sum(x => x.betAmount) * GetWinnerMulti(gate);
                if (refundAmount > 0)
                {
                    long refund = GameDAO.Refund(sessionId, _banker.AccountId, _banker.AccountName, $"Hoàn tiền bán cửa: {gate.ToString()}, {refundAmount} {money}", refundAmount, (int)_moneyType);
                    if (refund >= 0)
                    {
                        g.GateState = 0;
                    }
                    return refund;
                }
                throw new SellGateException();
            }
            else throw new GateSoldOutException();
        }

        public long BuyGate(Gate gate, long sessionId, long accountId, string accountName)
        {
            string money = _moneyType == MoneyType.GOLD ? "Vàng" : "Xu";
            var g = _bets[(int)gate];
            if(g.GateState == 0)
            {
                long betResult = -99;
                long buyAmount = g.Logs.Sum(x => x.betAmount) * GetWinnerMulti(gate);
                betResult = GameDAO.Bet(sessionId, accountId, accountName, $"Mua cửa: {gate.ToString()}, {buyAmount} {money}", buyAmount, (int)_moneyType);
                if (betResult >= 0)
                {
                    g.Owner = accountId;
                    g.GateState = 1;
                }
                else
                {
                    throw new BuyGateException();
                }
                return betResult;
            }
            else
            {
                throw new BuyGateException();
            }
        }

        private long BetWithBanker(long sessionId, long accountId, string accountName, long amount, Gate gate, long banker, string bankerName, out long gateSumary, bool skipDAO = false)
        {
            gateSumary = 0;
            long sumaryBanker = _bankerLocks.Sum(x => x.betAmount);
            long requireNextLock = amount * GetWinnerMulti(gate);
            long oddLock = _bets[1].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.Odd)
                + _bets[2].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.ThreeUp)
                + _bets[3].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.ThreeDown);
            long evenLock = _bets[4].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.Even)
                + _bets[5].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.FourUp)
                + _bets[6].Logs.Sum(x => x.betAmount) * GetWinnerMulti(Gate.FourDown);
            if (gate >= Gate.Odd && gate <= Gate.ThreeDown)
                oddLock += requireNextLock;
            else evenLock += requireNextLock;
            long lockAmount = oddLock > evenLock ? oddLock - sumaryBanker : evenLock - sumaryBanker;
            long betResult = -99;
            string money = _moneyType == MoneyType.GOLD ? "Vàng" : "Xu";
            if (!skipDAO)
            {
                betResult = GameDAO.BetWithBanker(sessionId, accountId, accountName, $"Đặt cửa: {gate.ToString()}, {amount} {money}",
                    amount, (int)_moneyType, banker, lockAmount, bankerName, $"Tạm giữ bổ sung nhà cái: {gate.ToString()}, {amount} {money}");
            }
            else
            {
                betResult = 1;
            }

            if (betResult >= 0)
            {
                _bets[(int)gate].Logs.Add(new BetLog
                {
                    accountId = accountId,
                    accountName = accountName,
                    betAmount = amount,
                    betGate = gate
                });
                _bankerLocks.Add(new BetLog
                {
                    accountId = banker,
                    accountName = bankerName,
                    betAmount = lockAmount,
                    betGate = gate
                });
                gateSumary = _bets[(int)gate].Logs.Sum(x => x.betAmount);
            }
            return betResult;
        }

        private long BetNornal(long sessionId, long accountId, string accountName, long amount, Gate gate, out long gateSumary, bool skipDAO = false)
        {
            gateSumary = 0;
            string money = _moneyType == MoneyType.GOLD ? "Vàng" : "Xu";
            long betResult = -99;
            if (!skipDAO)
            {
                betResult = GameDAO.Bet(sessionId, accountId, accountName, $"Đặt cửa: {gate.ToString()}, {amount} {money}", amount, (int)_moneyType);
            }
            else
            {
                betResult = 1;
            }
            if (betResult >= 0)
            {
                _bets[(int)gate].Logs.Add(new BetLog
                {
                    accountId = accountId,
                    accountName = accountName,
                    betAmount = amount,
                    betGate = gate
                });
                gateSumary = _bets[(int)gate].Logs.Sum(x => x.betAmount);
            }
            return betResult;
        }

        public void CalculateResult(int result, long SessionId)
        {
            if (_calculated)
                return;

            if (_roomType == RoomType.FIFTY)
                NoBankerSumary(result);
            else BankerSumary(result);

            _query.AppendLine("begin transaction");
            _query.AppendLine("begin try");
            foreach(var data in _rewards)
            {
                long totalLose = data.Value.Sum(x => x.Lose);
                var totalRefund = data.Value.Sum(x => x.Refund);
                var totalReward = data.Value.Sum(x => x.Prize);
                _query.AppendLine($"exec SP_SedieSumary @_AccountId = {data.Key}, " +
                    $"@_AccountName = N'', " +
                    $"@_TotalLose = {totalLose}, " +
                    $"@_TotalRefund = {totalRefund}, " +
                    $"@_TotalAward = {totalReward}, " +
                    $"@_BetType = {(int)_moneyType}," +
                    $"@_SessionId = {SessionId}, " +
                    $"@_Description = N''");
            }
            _query.AppendLine($"update dbo.Session set Status = 1, Result = N'{result}' where SessionId = {SessionId}");
            _query.AppendLine("commit transaction");
            _query.AppendLine("end try");
            _query.AppendLine("begin catch");
            _query.AppendLine("if @@trancount > 0 begin rollback transaction end;");
            _query.AppendLine("throw 50000, 'sql exception', 1");
            _query.AppendLine("end catch");
            
        }

        public string GetQuery()
        {
            if (_calculated)
                return string.Empty ;
            _calculated = true;
            return _query.ToString();
        }

        private void BankerSumary(int result)
        {
            List<Gate> winGate = new List<Gate>();
            switch (result)
            {
                case 0:
                    winGate.Add(Gate.Even);
                    winGate.Add(Gate.FourDown);
                    break;
                case 1:
                    winGate.Add(Gate.Odd);
                    winGate.Add(Gate.ThreeDown);
                    break;
                case 2:
                    winGate.Add(Gate.Even);
                    break;
                case 3:
                    winGate.Add(Gate.Odd);
                    winGate.Add(Gate.ThreeUp);
                    break;
                case 4:
                    winGate.Add(Gate.Even);
                    winGate.Add(Gate.FourUp);
                    break;
            }

            winGate.ForEach(i => BankerResultOnWinGate(i));
            new List<Gate>()
            {
                Gate.Even,
                Gate.FourDown,
                Gate.FourUp,
                Gate.Odd,
                Gate.ThreeDown,
                Gate.ThreeUp
            }.Except(winGate).ToList().ForEach(i => BankerResultOnLoseGate(i));
        }

        private void BankerResultOnLoseGate(Gate loseGate)
        {
            var loseBetLogs = _bets[(int)loseGate];
            var groupByAccount = loseBetLogs.Logs.GroupBy(x => x.accountId);
            long bankerId = _banker != null ? _banker.AccountId : -1;

            foreach (var account in groupByAccount)
            {
                Reward reward = new Reward();
                Reward reward1 = new Reward();
                reward.Gate = (int)loseGate;
                reward1.Gate = (int)loseGate;

                if (loseBetLogs.GateState == -1 || loseBetLogs.GateState == 1)
                {
                    var prize = account.Sum(x => x.betAmount);
                    reward.Lose = prize;
                    _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));
                    
                    reward1.Prize = (long)(prize * _fee);
                    reward1.Refund = prize * GetWinnerMulti(loseGate);
                    _rewards.AddOrUpdate(bankerId, new List<Reward> { reward1 }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward1 })));
                }
                else
                {
                    var prize = account.Sum(x => x.betAmount);
                    reward.Refund = prize;
                    _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));
                }
            }
        }

        private void BankerResultOnWinGate(Gate winGate)
        {
            var winBetLogs = _bets[(int)winGate];
            var groupByAccount = winBetLogs.Logs.GroupBy(x => x.accountId);
            long bankerId = _banker != null ? _banker.AccountId : -1;
            foreach (var account in groupByAccount)
            {
                Reward reward = new Reward();
                Reward reward1 = new Reward();
                reward.Gate = (int)winGate;
                reward1.Gate = (int)winGate;

                if (winBetLogs.GateState == -1 || winBetLogs.GateState == 1)
                {
                    var prize = account.Sum(x => x.betAmount);
                    reward.Prize = prize + (long)(prize * GetWinnerMulti(winGate) * _fee);
                    _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));

                    var lose = prize * GetWinnerMulti(winGate);
                    reward1.Lose = lose;
                    _rewards.AddOrUpdate(bankerId, new List<Reward> { reward1 }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward1 })));
                }
                else
                {
                    var prize = account.Sum(x => x.betAmount);
                    reward.Refund = prize;
                    _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));
                }
            }
        }

        private void NoBankerSumary(int result)
        {
            List<Gate> winGate = new List<Gate>();
            switch (result)
            {
                case 0:
                    winGate.Add(Gate.Even);
                    winGate.Add(Gate.FourDown);
                    break;
                case 1:
                    winGate.Add(Gate.Odd);
                    winGate.Add(Gate.ThreeDown);
                    break;
                case 2:
                    winGate.Add(Gate.Even);
                    break;
                case 3:
                    winGate.Add(Gate.Odd);
                    winGate.Add(Gate.ThreeUp);
                    break;
                case 4:
                    winGate.Add(Gate.Even);
                    winGate.Add(Gate.FourUp);
                    break;
            }

            winGate.ForEach(i => ResultOnGate(i));
            new List<Gate>()
            {
                Gate.Even,
                Gate.FourDown,
                Gate.FourUp,
                Gate.Odd,
                Gate.ThreeDown,
                Gate.ThreeUp
            }.Except(winGate).ToList().ForEach(i => ResultOnLoseGate(i));
        }

        private void ResultOnLoseGate(Gate loseGate)
        {
            var loseBetLogs = _bets[(int)loseGate];
            var groupByAccount = loseBetLogs.Logs.GroupBy(x => x.accountId);
            foreach (var account in groupByAccount)
            {
                Reward reward = new Reward();
                reward.Gate = (int)loseGate;
                var prize = account.Sum(x => x.betAmount);
                reward.Lose = prize;
                _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));
            }
        }

        private void ResultOnGate(Gate winGate)
        {
            var winBetLogs = _bets[(int)winGate];
            var groupByAccount = winBetLogs.Logs.GroupBy(x => x.accountId);
            foreach (var account in groupByAccount)
            {
                Reward reward = new Reward();
                reward.Gate = (int)winGate;
                var prize = account.Sum(x => x.betAmount);
                reward.Prize = prize + (long)(prize * GetWinnerMulti(winGate) * _fee);
                _rewards.AddOrUpdate(account.Key, new List<Reward> { reward }, (k, v) => new List<Reward>(v.Union(new List<Reward>() { reward })));
            }
        }

        public Dictionary<int, BetInfo> GetSumaryBet(long accountId)
        {
            Dictionary<int, BetInfo> sum = new Dictionary<int, BetInfo>();
            foreach (var b in _bets)
                sum.Add(b.Key, new BetInfo {
                    TotalBet = b.Value.Logs.Sum(x => x.betAmount),
                    OwnBet = b.Value.Logs.Where(x => x.accountId == accountId).Sum(x => x.betAmount),
                    State = b.Value.GateState
                });
            return sum;
        }

        public int GetWinnerMulti(Gate gate)
        {
            switch (gate)
            {
                case Gate.Even:
                case Gate.Odd:
                    return 1;
                case Gate.ThreeDown:
                case Gate.ThreeUp:
                    return 2;
                case Gate.FourUp:
                case Gate.FourDown:
                    return 10;
                default:
                    return 0;
            }
        }

        public void Refresh(Player banker = null)
        {
            _calculated = false;
            _banker = banker;
            long bankerId = banker != null ? banker.AccountId : -1;
            _rewards.Clear();
            _bets.Clear();
            _query.Clear();
            _bankerLocks.Clear();
            _bets.AddOrUpdate(1, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
            _bets.AddOrUpdate(2, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
            _bets.AddOrUpdate(3, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
            _bets.AddOrUpdate(4, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
            _bets.AddOrUpdate(5, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
            _bets.AddOrUpdate(6, new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE }, (k, v) => new GateBet { Owner = bankerId, Logs = new List<BetLog>(), GateState = (int)GateState.NON_TRADE });
        }
    }
}