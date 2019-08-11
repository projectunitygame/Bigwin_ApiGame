using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PTCN.CrossPlatform.Minigame.LuckyDice.Controllers;
using PTCN.CrossPlatform.Minigame.LuckyDice.Database;
using PTCN.CrossPlatform.Minigame.LuckyDice.Handlers.BotHandler;
using PTCN.CrossPlatform.Minigame.LuckyDice.Models;
using Utilities;
using Utilities.Log;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Handlers
{
    public interface IBotManager
    {
        void Start(); // Run Bot

        void Stop(); // Stop Bot

        void Bet();

    }

    public class BotManager : IBotManager
    {
        private BotConfiguration _configuration;

        private List<Bot> _botList;

        private object _managerLock;

        private LuckyDiceGameLoop _gameLoop;

        private Timer _updateTimer;

        private int updateTime = 30;

        private IEnumerable<BotData> _richBots;

        private IEnumerable<BotData> _normalBots;

        private IEnumerable<BotData> _poorBots;

        public long Fund = 0;

        public IList<BotData> _playersCollection;

        private List<BetData> _betData;


        public BotManager()
        {
            Fund = Lddb.Instance.GetFund().Fund;
            _betData = Lddb.Instance.GetBetData();
            NLogManager.LogMessage($"Fund : {Fund}");
            _configuration = Lddb.Instance.GetBotConfigs();
            _playersCollection = Lddb.Instance.GetAllBotName(); // Lấy danh sách bot
            _richBots = _playersCollection.Where(x => x.Vip == 2);
            _normalBots = _playersCollection.Where(x => x.Vip == 1);
            _poorBots = _playersCollection.Where(x => x.Vip == 0);
            _managerLock = new object();
            _botList = new List<Bot>();
            Init();

        }

        #region private methods

        private void Init()
        {
            var richBot = _richBots.Shuffle().Take(_configuration.NumRichBot);
            var normalBot = _normalBots.Shuffle().Take(_configuration.NumNormalBot);
            var poorBot = _poorBots.Shuffle().Take(_configuration.NumPoorBot);


            var richBetData = _betData.Where(x => x.Vip == 2).Select(x => x.BetValue).ToArray();
            var normalBetData = _betData.Where(x => x.Vip == 1).Select(x => x.BetValue).ToArray();
            var poorBetData = _betData.Where(x => x.Vip == 0).Select(x => x.BetValue).ToArray();
            if (_configuration.NumRichBot > 0)
            {
                foreach (var botData in richBot)
                {
                    var bot = new RichBot { DisplayName = botData.BotName, Vip = botData.Vip, BetValues = richBetData };
                    bot.Init();
                    _botList.Add(bot);
                }
            }

            if (_configuration.NumNormalBot > 0)
            {
                foreach (var botData in normalBot)
                {
                    var bot = new NormalBot() { DisplayName = botData.BotName, Vip = botData.Vip, BetValues = normalBetData };
                    bot.Init();
                    _botList.Add(bot);
                }
            }

            if (_configuration.NumPoorBot > 0)
            {
                foreach (var botData in poorBot)
                {
                    var bot = new PoorBot() { DisplayName = botData.BotName, Vip = botData.Vip, BetValues = poorBetData };
                    bot.Init();
                    _botList.Add(bot);
                }
            }
            NLogManager.LogMessage($"init botlist:  {JsonConvert.SerializeObject(_configuration)}");
            NLogManager.LogMessage($"init botlist: {_botList.Count}");
        }

        #endregion

        public void Start()
        {
            _updateTimer = new Timer(UpdateBot, null, _configuration.MinTimeChange * 1000 * 60, Timeout.Infinite);
        }

        public void Stop()
        {
            _updateTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }


        public void Bet()
        {

            if (Monitor.TryEnter(_managerLock, 5000))
            {
                try
                {
                    LoadBetData();
                    LoadConfig();
                    if (!_configuration.Enable)
                        return;
                    // Kiểm tra config
                    var accountId = 10000;
                    _gameLoop = GameManager.GetGameLoop(1);
                    var _factory = new TaskFactory();
                    // Thay đổi số lượng bot bet

                    _botList.Shuffle();
                    var botChange = RandomUtil.NextInt(_configuration.MinBot, _configuration.MaxBot);
                    var betList = _botList.ToList().GetRange(0, botChange);
                    var idleTime = RandomUtil.NextInt(1, 48);
                    var idleTime2 = RandomUtil.NextInt(1, 48);
                    NLogManager.LogMessage($"Bot Bet | IdleTime :{idleTime} - {idleTime2}");
                    foreach (var bot in betList)
                    {
                        _factory.StartNew(async () =>
                        {
                            try
                            {
                                var time = RandomUtil.NextInt(1, 50);
                                bot.Check();
                                while (time >= idleTime && time <= idleTime + 2 && bot.BetSide == BetSide.Tai)
                                {
                                    time = RandomUtil.NextInt(1, 50);
                                }

                                while (time >= idleTime2 && time <= idleTime2 + 2 && bot.BetSide == BetSide.Xiu)
                                {
                                    time = RandomUtil.NextInt(1, 50);
                                }

                                await Task.Delay(time * 1000);


                                var betResponse = _gameLoop.Bet("", accountId++, bot.DisplayName, "", bot.BetSide,
                                    bot.BetAmount, out var summaryBet,
                                    out var balance, out var error, true);
                            }
                            catch (Exception e)
                            {
                                NLogManager.PublishException(e);
                                Console.WriteLine(e);
                            }

                        });
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.PublishException(ex);
                }
                finally
                {
                    Monitor.Exit(_managerLock);
                }
            }

        }

        public void UpdateBot(object obj)
        {
            if (Monitor.TryEnter(_managerLock, 5000))
            {
                try
                {
                    _updateTimer.Change(RandomUtil.NextInt(_configuration.MinTimeChange, _configuration.MaxTimeChange) * 60 * 1000, Timeout.Infinite); // Cap nhat lai bot
                    NLogManager.LogMessage("Update Bots By Timer");
                    var richBots = _botList.Where(x => x.Vip == 2).Shuffle().ToList();
                    var normalBots = _botList.Where(x => x.Vip == 1).Shuffle().ToList();
                    var poorBots = _botList.Where(x => x.Vip == 0).Shuffle().ToList();
                    var richNumChange = (int)Math.Ceiling(richBots.Count * _configuration.VipChangeRate / 100.0);
                    var normalNumChange = (int)Math.Ceiling(normalBots.Count * _configuration.NorChangeRate / 100.0);
                    var poorNumChange = (int)Math.Ceiling(poorBots.Count * _configuration.PoorChangeRate / 100.0);

                    richBots.RemoveRange(0, richNumChange);
                    normalBots.RemoveRange(0, normalNumChange);
                    poorBots.RemoveRange(0, poorNumChange);

                    var richBetData = _betData.Where(x => x.Vip == 2).Select(x => x.BetValue).ToArray();
                    var normalBetData = _betData.Where(x => x.Vip == 1).Select(x => x.BetValue).ToArray();
                    var poorBetData = _betData.Where(x => x.Vip == 0).Select(x => x.BetValue).ToArray();

                    if (richNumChange > 0)
                    {
                        for (var i = 0; i < richNumChange; i++)
                        {
                            var bot = _richBots.PickRandom();
                            while (richBots.Any(x => x.DisplayName == bot.BotName))
                            {
                                bot = _richBots.PickRandom();
                            }

                            richBots.Add(new RichBot()
                            {
                                DisplayName = bot.BotName,
                                Vip = bot.Vip,
                                BetValues = richBetData
                            });
                        }
                    }

                    if (normalNumChange > 0)
                    {
                        for (var i = 0; i < normalNumChange; i++)
                        {
                            var bot = _normalBots.PickRandom();
                            while (normalBots.Any(x => x.DisplayName == bot.BotName))
                            {
                                bot = _normalBots.PickRandom();
                            }

                            normalBots.Add(new NormalBot()
                            {
                                DisplayName = bot.BotName,
                                Vip = bot.Vip,
                                BetValues = normalBetData
                            });
                        }
                    }

                    if (poorNumChange > 0)
                    {
                        for (var i = 0; i < poorNumChange; i++)
                        {
                            var bot = _poorBots.PickRandom();
                            while (poorBots.Any(x => x.DisplayName == bot.BotName))
                            {
                                bot = _poorBots.PickRandom();
                            }

                            poorBots.Add(new PoorBot()
                            {
                                DisplayName = bot.BotName,
                                Vip = bot.Vip,
                                BetValues = poorBetData
                            });
                        }
                    }

                    NLogManager.LogMessage($"Before Change List:{_botList.Count}");

                    _botList.Clear();
                    _botList = new List<Bot>(richBots);
                    _botList.AddRange(normalBots);
                    _botList.AddRange(poorBots);

                    NLogManager.LogMessage($"After Change List:{_botList.Count}");
                }
                catch (Exception e)
                {
                    NLogManager.PublishException(e);
                }
                finally
                {
                    Monitor.Exit(_managerLock);
                }
            }
            
            //Init();
        }

        public void UpdateFund(long sessionId, long amount)
        {
            long response = Lddb.Instance.UpdateFund(sessionId, amount, out var fund);
            Fund = fund;
        }

        public bool EnableRunBot()
        {
            return _configuration != null && _configuration.Enable;
        }

        public IList<Bot> GetBotsList()
        {
            return _botList;
        }

        private void LoadBetData()
        {
            var betData = Lddb.Instance.GetBetData();
            if (betData == null)
            {
                NLogManager.LogMessage("Cant load bet data");
                return;
            }
            if (!betData.JSONEquals(_betData)) // Cap nhat neu thay doi trong CMS
            {
                _betData = betData;
                var richBetData = _betData.Where(x => x.Vip == 2).Select(x => x.BetValue).ToArray();
                var normalBetData = _betData.Where(x => x.Vip == 1).Select(x => x.BetValue).ToArray();
                var poorBetData = _betData.Where(x => x.Vip == 0).Select(x => x.BetValue).ToArray();
                NLogManager.LogMessage("Update BetData By CMS");
                foreach (var bot in _botList)
                {
                    if (bot.GetType() == typeof(RichBot))
                    {
                        bot.BetValues = richBetData;
                    }
                    else if (bot.GetType() == typeof(NormalBot))
                    {
                        bot.BetValues = normalBetData;
                    }
                    else
                    {
                        bot.BetValues = poorBetData;
                    }
                }
            }
        }

        private void LoadConfig()
        {

            var config = Lddb.Instance.GetBotConfigs();
            if (config == null)
            {
                NLogManager.LogMessage("Cant load bet config");
                return;
            }
            NLogManager.LogMessage($"Load Config : {JsonConvert.SerializeObject(config)}");
            if (config == null) return;

            if (config.Equals(_configuration))
            {
                _configuration = config; // cap nhat bot config
                return;
            }
            // Cập nhật thay đổi số lượng bot
            var richBots = _botList.Where(x => x.Vip == 2).Shuffle().ToList();
            var normalBots = _botList.Where(x => x.Vip == 1).Shuffle().ToList();
            var poorBots = _botList.Where(x => x.Vip == 0).Shuffle().ToList();

            var richNumChange = config.NumRichBot - _configuration.NumRichBot;
            var normalNumChange = config.NumNormalBot - _configuration.NumNormalBot;
            var poorNumChange = config.NumPoorBot - _configuration.NumPoorBot;

            var richBetData = _betData.Where(x => x.Vip == 2).Select(x => x.BetValue).ToArray();
            var normalBetData = _betData.Where(x => x.Vip == 1).Select(x => x.BetValue).ToArray();
            var poorBetData = _betData.Where(x => x.Vip == 0).Select(x => x.BetValue).ToArray();

            if (richNumChange < 0)
            {
                richBots.RemoveRange(0, Math.Abs(richNumChange));
            }
            else if (richNumChange > 0)
            {
                for (var i = 0; i < richNumChange; i++)
                {
                    var bot = _richBots.PickRandom();
                    while (richBots.Any(x => x.DisplayName == bot.BotName))
                    {
                        bot = _richBots.PickRandom();
                    }

                    richBots.Add(new RichBot()
                    {
                        DisplayName = bot.BotName,
                        Vip = bot.Vip,
                        BetValues = richBetData
                    });
                }
            }

            if (normalNumChange < 0)
            {
                normalBots.RemoveRange(0, Math.Abs(normalNumChange));
            }
            else if (normalNumChange > 0)
            {
                for (var i = 0; i < normalNumChange; i++)
                {
                    var bot = _normalBots.PickRandom();
                    while (normalBots.Any(x => x.DisplayName == bot.BotName))
                    {
                        bot = _normalBots.PickRandom();
                    }

                    normalBots.Add(new NormalBot()
                    {
                        DisplayName = bot.BotName,
                        Vip = bot.Vip,
                        BetValues = normalBetData
                    });
                }
            }

            if (poorNumChange < 0)
            {
                poorBots.RemoveRange(0, Math.Abs(poorNumChange));
            }
            else if (poorNumChange > 0)
            {

                for (var i = 0; i < poorNumChange; i++)
                {
                    var bot = _poorBots.PickRandom();
                    while (poorBots.Any(x => x.DisplayName == bot.BotName))
                    {
                        bot = _poorBots.PickRandom();
                    }

                    poorBots.Add(new PoorBot()
                    {
                        DisplayName = bot.BotName,
                        Vip = bot.Vip,
                        BetValues = poorBetData
                    });
                }
            }

            NLogManager.LogMessage($"Update Bot By CMS => Old:{JsonConvert.SerializeObject(_configuration)}|New:{JsonConvert.SerializeObject(config)}|RichChange:{richNumChange}|NormalChange:{normalNumChange}|PoorChange:{poorNumChange}");

            _botList.Clear();
            _botList = new List<Bot>(richBots);
            _botList.AddRange(normalBots);
            _botList.AddRange(poorBots);

            _configuration = config; // cap nhat bot config
        }
    }
}