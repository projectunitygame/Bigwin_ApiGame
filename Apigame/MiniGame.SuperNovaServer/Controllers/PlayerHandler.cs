using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Intecom.Software.RDTech.SlotMachine.DataAccess.DTO;
using Studio.WebGame.SupperNova.Hubs;
using Studio.WebGame.SupperNova.Models;
using Utilities.Log;

namespace Studio.WebGame.SupperNova.Controllers
{
    public class PlayerHandler
    {
        private static readonly Lazy<PlayerHandler> _instance = new Lazy<PlayerHandler>(() => new PlayerHandler());
        private readonly InnerPlayerHandler<GamePlayer> _inner = new InnerPlayerHandler<GamePlayer>();

        public static InnerPlayerHandler<GamePlayer> Instance
        {
            get { return _instance.Value._inner; }
        }

        private PlayerHandler()
        {
        }

        public class InnerPlayerHandler<T> where T : AbstractGamePlayer
        {
            // Singleton instance       
            private readonly ConcurrentDictionary<long, T> _players;

            public InnerPlayerHandler()
            {
                _players = new ConcurrentDictionary<long, T>();

            }

            #region dictionary_methods

            private T CreatePlayer(params object[] args)
            {
                return (T)Activator.CreateInstance(typeof(T), args);
            }

            public bool Contains(long accountId)
            {
                return _players.ContainsKey(accountId);
            }

            public T GetPlayer(long accountId)
            {
                if (accountId < 1)
                {
                    return null;
                }

                T player = null;
                if (Contains(accountId))
                {
                    _players.TryGetValue(accountId, out player);
                }
                return player;
            }

            public T GetPlayer(string connectionId)
            {
                long accountId = ConnectionHandler.Instance.GetAccountIdByConnectionId(connectionId);
                if (accountId <= 0) return null;
                T player = null;
                if (Contains(accountId))
                    _players.TryGetValue(accountId, out player);
                return player;
            }

            public bool AddPlayer(T player, long ConnectionKey)
            {
                if (player == null)
                {
                    return false;
                }

                if (_players.ContainsKey(ConnectionKey))
                {
                    var oldAccount = player.Account;
                    //update account
                    if (!Monitor.TryEnter(oldAccount, 5000))
                        return true;
                    try
                    {
                        Copy(player.Account, oldAccount);
                    }
                    finally
                    {
                        Monitor.Exit(oldAccount);
                    }
                    return true;
                }
                return _players.TryAdd(ConnectionKey, player);
            }

            //public T AddPlayer(Account account, SpinData spinData, BonusGame bonusGame, int betValue, long currentJackPort, int gameStatus)
            public T AddPlayer(Account account, long ConnectionKey)
            {
                if (account == null)
                {
                    return null;
                }
                if (account.AccountID < 1)
                {
                    return null;
                }

                T player = null;
                if (_players.ContainsKey(ConnectionKey))
                {
                    player = GetPlayer(ConnectionKey);
                    var oldAccount = player.Account;
                    //update account
                    if (!Monitor.TryEnter(oldAccount, 5000))

                        return player;
                    try
                    {
                        Copy(player.Account, oldAccount);
                    }
                    finally
                    {
                        Monitor.Exit(oldAccount);
                    }
                    return player;
                }

                player = CreatePlayer(account);
                if (player == null)
                    return null;

                player.AccountID = player.Account.AccountID;
                AddPlayer(player, ConnectionKey);
                return player;
            }

            public T AddPlayer(long accountId, string username, long ConnectionKey)
            {
                if (accountId < 1 || String.IsNullOrEmpty(username))
                {
                    return null;
                }

                if (_players.ContainsKey(ConnectionKey))
                {
                    return GetPlayer(ConnectionKey);
                }
                try
                {
                    var account = new Account()
                    {
                        AccountID = accountId,
                        UserName = username
                    };

                    return AddPlayer(account, ConnectionKey);
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
                return null;
            }

            public bool RemovePlayer(long accountId)
            {
                T player = null;
                return _players.TryRemove(accountId, out player);
            }

            public void CountPlayer(out int vip, out int normal)
            {
                vip = 0;
                normal = 0;
                foreach (var u in _players)
                {
                    var pl = GetPlayer(u.Key) as GamePlayer;
                    switch (pl.BetType)
                    {
                        case (int)Enums.BetType.COIN:
                            normal += 1;
                            break;
                        case (int)Enums.BetType.STAR:
                            vip += 1;
                            break;
                    }
                }


            }

            public int Count()
            {
                return _players.Count;
            }

            #endregion dictionary_methods

            #region util_methods

            public bool UpdateMoney(long accountId, long change,
                int betType)
            {
                if (accountId < 1 || change == 0)
                {
                    return false;
                }
                T player = GetPlayer(accountId);
                if (player == null)
                {
                    return false;
                }

                var account = player.Account;
                if (!Monitor.TryEnter(account, 5000)) return false;
                try
                {
                    long currentMoney = (betType ==
                                         (int)Enums.BetType.COIN)
                        ? account.Coin
                        : account.TotalStar;
                    if (change < 0 && Math.Abs(change) > currentMoney)
                    {
                        return false;
                    }
                    currentMoney += change;

                    switch (betType)
                    {
                        case (int)Enums.BetType.COIN:
                            account.Coin = currentMoney;
                            break;
                        case (int)Enums.BetType.STAR:
                            account.TotalStar = currentMoney;
                            break;
                        default:
                            return false;
                    }

                    return true;
                }
                finally
                {
                    Monitor.Exit(account);
                }
            }

            /// <summary>
            /// Copy from first to second.
            /// </summary>
            /// <param name="first"></param>
            /// <param name="second"></param>
            public void Copy(Account first, Account second)
            {
                second.UserName = first.UserName;
                second.Coin = first.Coin;
                second.TotalStar = first.TotalStar;
                second.Star = first.Star;
                second.Vcoin = first.Vcoin;
                second.EventCoin = first.EventCoin;
                second.IsOtp = first.IsOtp;
                second.MerchantID = first.MerchantID;
                if (first.SourceID != 0)
                    second.SourceID = first.SourceID;
            }


            public Account GetAccount(long accountId)
            {
                if (accountId < 1)
                {
                    return null;
                }

                T player = null;
                return _players.TryGetValue(accountId, out player) ? player.Account : null;
            }

            #endregion util_methods
        }
    }
}