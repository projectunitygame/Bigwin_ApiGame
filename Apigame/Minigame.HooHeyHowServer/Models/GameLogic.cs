using Minigame.HooHeyHowServer.Models.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using Utilities.Log;

namespace Minigame.HooHeyHowServer.Models
{
    public class GameLogic
    {
        private ConcurrentQueue<BetLog> _betLogs;
        private Dictionary<long, Dictionary<int, BetLog>> _awards;
        private long _fund;
        private MoneyType _moneyType;
        private decimal _fee;
        private StringBuilder _query;
        private bool computedResult;
        public ConcurrentDictionary<int, long> BetGates { get; private set; }
        public ConcurrentDictionary<int, int> BetGateCount { get; private set; }

        public GameLogic(MoneyType moneyType, long fund)
        {
            _query = new StringBuilder();
            _fee = 0.98M;
            _moneyType = moneyType;
            _fund = fund;
            _awards = new Dictionary<long, Dictionary<int, BetLog>>();
            Restart();
        }

        public List<PlayerBetGate> GetPlayerBettingInfo(long accountId)
        {
            List<PlayerBetGate> bets = new List<PlayerBetGate>();
            var info = _betLogs.Where(x => x.accountId == accountId).GroupBy(x => x.betGate);
            foreach (var data in info)
                bets.Add(new PlayerBetGate
                {
                    Gate = data.FirstOrDefault().betGate,
                    Amount = data.Sum(x => x.amount)
                });
            return bets;
        }

        public long Bet(long sessionId, long accountId, string accountName, string betData)
        {
            try
            {
                long betAmount = 0;
                betData = betData.TrimEnd('|');
                string[] data = betData.Split('|');

                foreach (var d in data)
                {
                    string[] parse = d.Split(';');
                    if (long.Parse(parse[1]) < 1000)
                        return -99;
                    betAmount += long.Parse(parse[1]);

                    int gate = int.Parse(parse[0]);
                    if (gate < 1 || gate > 6)
                        return -99;
                }

                long response = GameDAO.Bet(sessionId, accountId, accountName, betData, betAmount, (int)_moneyType);

                if (response > 0)
                {
                    _fund += betAmount;
                    foreach (var d in data)
                    {
                        string[] parse = d.Split(';');
                        int gate = int.Parse(parse[0]);
                        long amount = long.Parse(parse[1]);

                        BetGates.AddOrUpdate((int)gate, amount, (k, v) => v += amount);
                        if (!_betLogs.ToList().Exists(x => x.accountId == accountId))
                            BetGateCount.AddOrUpdate(gate, 1, (k, v) => v += 1);
                        _betLogs.Enqueue(new BetLog { accountId = accountId, amount = amount, betGate = (BetGate)gate, accountName = accountName });
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
            return -99;
        }

        public string BuildResult(GameResult result)
        {
            foreach (var bet in _betLogs)
            {
                bool addLog = false;
                ResultOnGate(result.Dice1, bet, ref addLog);
                ResultOnGate(result.Dice2, bet, ref addLog);
                ResultOnGate(result.Dice3, bet, ref addLog);
            }

            foreach (var award in _awards)
            {
                bool haveResult = false;
                string accountName = string.Empty;
                long accountId = 0;
                string gate = string.Empty;
                long awardValue = 0;
                long totalBet = 0;
                foreach (var gateAward in award.Value)
                {
                    accountName = gateAward.Value.accountName;
                    accountId = gateAward.Value.accountId;
                    gate += (int)gateAward.Value.betGate + ";" + gateAward.Value.award + "|";
                    awardValue += gateAward.Value.award;
                    totalBet += gateAward.Value.amount;
                    haveResult = true;
                }

                if (haveResult)
                {
                    _query.AppendLine($"exec SP_Reward @_SessionId = {GameSession.Session.SessionId}, " +
                        $"@_AccountName = N'{accountName}', " +
                        $"@_AccountId = {accountId}, " +
                        $"@_BetType = {(int)_moneyType}, " +
                        $"@_Gate = N'{gate}', " +
                        $"@_TotalBet = {totalBet}, " +
                        $"@_Award = {awardValue}");
                }
            }

            computedResult
                = true;
            return _query.ToString();
        }

        public Dictionary<long, Dictionary<int, BetLog>> GetAwards()
        {
            if (computedResult)
                return DeepClone<Dictionary<long, Dictionary<int, BetLog>>>(_awards);

            return null;
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

        private void ResultOnGate(BetGate result, BetLog bet, ref bool addLog)
        {
            if (bet.betGate != result)
                return;

            long prize = bet.amount + (long)(bet.amount * _fee);
            if (addLog)
                prize -= bet.amount;
            _fund -= prize;
            if (_awards.ContainsKey(bet.accountId))
            {
                if (_awards[bet.accountId].ContainsKey((int)result))
                {
                    _awards[bet.accountId][(int)result].award += prize;
                    if(!addLog)
                        _awards[bet.accountId][(int)result].amount += bet.amount;
                }
                else
                {
                    _awards[bet.accountId].Add((int)result, new BetLog
                    {
                        accountId = bet.accountId,
                        accountName = bet.accountName,
                        betGate = bet.betGate,
                        amount = bet.amount,
                        award = prize
                    });
                }
            }
            else
            {
                _awards.Add(bet.accountId, new Dictionary<int, BetLog> {
                    {
                        (int)bet.betGate, new BetLog
                        {
                            accountId = bet.accountId,
                            accountName = bet.accountName,
                            betGate = bet.betGate,
                            amount = bet.amount,
                            award = prize
                        }
                    }
                });
            }
            addLog = true;
        }

        public void ReloadRealFund()
        {
            _fund = GameDAO.GetFund(_moneyType);
        }

        public void Restart()
        {
            computedResult = false;
            _awards.Clear();
            _query.Clear();
            _betLogs = new ConcurrentQueue<BetLog>();
            BetGates = new ConcurrentDictionary<int, long>();
            BetGates.AddOrUpdate(1, 0, (k, v) => v = 0);
            BetGates.AddOrUpdate(2, 0, (k, v) => v = 0);
            BetGates.AddOrUpdate(3, 0, (k, v) => v = 0);
            BetGates.AddOrUpdate(4, 0, (k, v) => v = 0);
            BetGates.AddOrUpdate(5, 0, (k, v) => v = 0);
            BetGates.AddOrUpdate(6, 0, (k, v) => v = 0);
            BetGateCount = new ConcurrentDictionary<int, int>();
            BetGateCount.AddOrUpdate(1, 0, (k, v) => v = 0);
            BetGateCount.AddOrUpdate(2, 0, (k, v) => v = 0);
            BetGateCount.AddOrUpdate(3, 0, (k, v) => v = 0);
            BetGateCount.AddOrUpdate(4, 0, (k, v) => v = 0);
            BetGateCount.AddOrUpdate(5, 0, (k, v) => v = 0);
            BetGateCount.AddOrUpdate(6, 0, (k, v) => v = 0);
        }
    }
}