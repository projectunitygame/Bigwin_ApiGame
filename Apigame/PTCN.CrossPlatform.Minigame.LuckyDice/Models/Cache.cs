using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models.Chat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Utilities.Log;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Models
{
    public class Cache
    {
        List<DiceResult> _results = new List<DiceResult>();
        object _lockResult = new object();
        bool _loadedResult = false;
        List<SessionInfo> _sessionInfos = new List<SessionInfo>();
        object _lockSessionInfo = new object();
        List<Rank> _ranks = new List<Rank>();
        object _lockRanks = new object();
        bool _loadedRank = false;
        int _lastUpdateRank = DateTime.Now.Day % 2;
        List<ChatMessage> _msgs = new List<ChatMessage>();
        object _lockmsg = new object();
        static ConcurrentDictionary<long, ChatUser> _users = new ConcurrentDictionary<long, ChatUser>();

        Timer _cleanTime = new Timer(new TimerCallback(Clean), null, 60 * 60000, 60 * 60000);
        //private static int _chatGoldLimit = 50000;
        private static int _chatGoldLimit = 20000;
        public Cache(bool enableTimer)
        {
            if (!enableTimer)
            {
                _cleanTime.Change(-1, -1);
                _cleanTime.Dispose();
            }
        }

        static void Clean(object o)
        {
            var lst = _users.Values.Where(x => TimeSpan.FromTicks(DateTime.Now.Ticks - x.LastActiveTime).TotalMinutes >= 60).ToList();
            lst.ForEach(u => { _users.TryRemove(u.AccountId, out u); });
        }

        public List<ChatMessage> GetMessage()
        {
            return _msgs;
        }

        public void UpdateRecentBetting(long accountId, string accountName, long total, bool updateValue = false)
        {
            try
            {
                ChatUser user;
                if (_users.TryGetValue(accountId, out user))
                {
                    if (!updateValue)
                        user.TotalRecentBetting += total;
                    else user.TotalRecentBetting = total;
                }
                else
                {
                    user = new ChatUser
                    {
                        AccountId = accountId,
                        AccountName = accountName,
                        TotalRecentBetting = total,
                        LastActiveTime = DateTime.Now.Ticks,
                    };
                    _users.AddOrUpdate(accountId, user, (k, v) => v = user);
                }
            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        public int CheckEnableChat(long accountId, string accountName, string message, int userType, out ChatMessage msg)
        {
            ChatUser user;
            msg = null;

            if (_users.TryGetValue(accountId, out user))
            {
                //if (user.TotalRecentBetting < _chatGoldLimit)
                //    return -1;

                if (ChatFilter.CheckBanned(accountName))
                    return -2;

                if (TimeSpan.FromTicks(DateTime.Now.Ticks - user.LastSpamTime).TotalMinutes <= 5)
                    return -3;

                if (user.DetectSpam(message))
                    return -4;
            }
            else
            {
                //long recentBetting = Lddb.Instance.GetRecentBetting(accountId);

                //if (recentBetting < 0)
                //    return -1;

                user = new ChatUser
                {
                    AccountId = accountId,
                    AccountName = accountName,
                    //TotalRecentBetting = recentBetting,
                    LastActiveTime = DateTime.Now.Ticks,
                };

                _users.AddOrUpdate(accountId, user, (k, v) => v = user);

                //if (recentBetting < _chatGoldLimit)
                //    return -1;

                if (ChatFilter.CheckBanned(accountName))
                    return -2;

                if (TimeSpan.FromTicks(DateTime.Now.Ticks - user.LastSpamTime).TotalMinutes <= 5)
                    return -3;

                if (user.DetectSpam(message))
                    return -4;
            }

            int uType = 4;

            if (userType >= 3)
                uType = 3;

            if (Monitor.TryEnter(_lockmsg, 5000))
            {
                try
                {
                    msg = new ChatMessage { T = uType, M = message, U = accountName };
                    _msgs.Add(msg);
                    if (_msgs.Count > 100)
                        _msgs.RemoveAt(0);
                }
                finally
                {
                    Monitor.Exit(_lockmsg);
                }
            }

            return 0;
        }

        public List<Rank> GetRanks(int moneyType)
        {
            if (Monitor.TryEnter(_lockRanks, 5000))
            {
                try
                {
                    _ranks = Lddb.Instance.GetRank(moneyType);
                    //if (_lastUpdateRank != DateTime.Now.Day % 2)
                    //{
                    //    _lastUpdateRank = DateTime.Now.Day % 2;
                    //    _loadedRank = false;
                    //}

                    //if (!_loadedRank)
                    //{
                    //    _ranks = Lddb.Instance.GetRank(moneyType);
                    //    _loadedRank = true;
                    //}
                }
                finally
                {
                    Monitor.Exit(_lockRanks);
                }
            }

            return _ranks;
        }

        public void UpdateCachedRank(Rank rank, int moneyType, int dateStatus)
        {
            if (Monitor.TryEnter(_lockRanks, 5000))
            {
                try
                {
                    if (_lastUpdateRank != dateStatus)
                    {
                        _lastUpdateRank = dateStatus;
                        _loadedRank = false;
                    }

                    if (!_loadedRank)
                    {
                        _ranks = Lddb.Instance.GetRank(moneyType);
                        _loadedRank = true;
                    }

                    if (_ranks.Count >= 20)
                    {
                        long val = _ranks.Select(x => x.Award).Min();
                        if (val < rank.Award)
                        {
                            _ranks.Remove(_ranks.LastOrDefault(x => x.Award == val));
                            _ranks.Add(rank);
                        }
                    }
                    else
                    {
                        _ranks.Add(rank);
                    }

                    _ranks = _ranks.OrderByDescending(x => x.Award).ToList();
                }
                finally
                {
                    Monitor.Exit(_lockRanks);
                }
            }
        }

        public SessionInfo GetRecentSessionInfo(long sessionId, int moneyType)
        {
            if (Monitor.TryEnter(_lockSessionInfo, 5000))
            {
                try
                {
                    var lstSession = GetRecentResult(moneyType);

                    if (lstSession.Exists(x => x.SessionId == sessionId))
                    {
                        var session = _sessionInfos.FirstOrDefault(x => x.Result.SessionId == sessionId);

                        if (session == null)
                        {
                            session = new SessionInfo
                            {
                                Result = Lddb.Instance.GetSessionResultInfo(sessionId),
                                BetList = Lddb.Instance.GetSessionBetInfo(sessionId)
                            };

                            _sessionInfos.Add(session);

                            session.Result.SessionId = sessionId;

                            if (_sessionInfos.Count > 20)
                            {
                                var minSessionId = _sessionInfos.Select(x => x.Result.SessionId).Min();
                                _sessionInfos.Remove(_sessionInfos.FirstOrDefault(x => x.Result.SessionId == minSessionId));
                            }

                            return session;
                        }

                        return session;
                    }
                    else
                    {
                        var session = _sessionInfos.FirstOrDefault(x => x.Result.SessionId == sessionId);
                        return session;
                    }
                }
                finally
                {
                    Monitor.Exit(_lockSessionInfo);
                }
            }

            return null;
        }

        public void AddSession(SessionInfo info)
        {
            if (Monitor.TryEnter(_lockSessionInfo, 5000))
            {
                try
                {
                    var included = _sessionInfos.FirstOrDefault(x => x.Result.SessionId == info.Result.SessionId);
                    if (included != null)
                    {
                        _sessionInfos.Remove(included);
                    }

                    _sessionInfos.Add(info);

                    if (_sessionInfos.Count > 20)
                    {
                        var minSessionId = _sessionInfos.Select(x => x.Result.SessionId).Min();
                        _sessionInfos.Remove(_sessionInfos.FirstOrDefault(x => x.Result.SessionId == minSessionId));
                    }
                }
                finally
                {
                    Monitor.Exit(_lockSessionInfo);
                }
            }
        }

        public List<DiceResult> GetRecentResult(int moneyType)
        {
            if (Monitor.TryEnter(_lockResult, 5000))
            {
                try
                {
                    if (!_loadedResult)
                    {
                        _results = Lddb.Instance.GetHistory(moneyType);
                        _results = _results.OrderBy(x => x.SessionId).ToList();
                        _loadedResult = true;
                    }
                }
                finally
                {
                    Monitor.Exit(_lockResult);
                }
            }
            return _results;
        }

        public void PushResult(DiceResult result)
        {
            if (Monitor.TryEnter(_lockResult, 5000))
            {
                try
                {
                    _results.Add(result);
                    if (_results.Count > 100)
                        _results.RemoveAt(0);
                }
                finally
                {
                    Monitor.Exit(_lockResult);
                }
            }
        }
    }
}