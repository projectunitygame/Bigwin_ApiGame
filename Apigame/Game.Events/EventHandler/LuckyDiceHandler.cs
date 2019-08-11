using Game.Events.Controllers;
using Game.Events.Database.DAOImpl;
using Game.Events.Database.DTO;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities.Log;

namespace Game.Events.EventHandler
{
    public class LuckyDiceHandler : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                BetKingTime time = LuckyDiceEventDAO.GetTimeEvent();
                string t = time.Start.ToString();
                DateTime start = new DateTime(int.Parse(t.Substring(0, 4)), int.Parse(t.Substring(4, 2)), int.Parse(t.Substring(6, 2)));
                List<int> days = new List<int>();
                int timeInt = time.Start;
                days.Add(timeInt);
                start = start.AddDays(1);
                while (timeInt != time.End)
                {
                    timeInt = int.Parse($"{start.Year.ToString("D4")}{start.Month.ToString("D2")}{start.Day.ToString("D2")}");
                    start = start.AddDays(1);
                    days.Add(timeInt);
                }

                IEnumerable<BetKingTime> successRewardDays = LuckyDiceEventDAO.GetSuccessRewardDay();
                List<int> successes = successRewardDays.Select(x => x.Day).ToList();
                int now = int.Parse($"{DateTime.Now.Year.ToString("D4")}{DateTime.Now.Month.ToString("D2")}{DateTime.Now.Day.ToString("D2")}");
                days = days.Where(x => x < now).ToList();
                days = days.Where(x => !successes.Contains(x)).ToList();
                StringBuilder strQuery = new StringBuilder();
                strQuery.AppendLine("begin transaction");
                strQuery.AppendLine("begin try");
                foreach (var d in days)
                {
                    //trao giai ngay
                    IEnumerable<LuckydiceRank> topWins = new LuckydiceController().getTop(d.ToString(), 1);
                    IEnumerable<LuckydiceRank> topLoses = new LuckydiceController().getTop(d.ToString(), 2);
                    foreach (var i in topWins)
                    {
                        strQuery.AppendLine($"exec SP_RewardPrize @_AccountId = {i.AccountID}, @_AccountName = '{i.AccountName}', @_Day = {d}, @_Rank = {i.ID}, @_Type = 4, @_Prize = {getPrize(i.ID)}");
                    }
                    foreach (var i in topLoses)
                    {
                        strQuery.AppendLine($"exec SP_RewardPrize @_AccountId = {i.AccountID}, @_AccountName = '{i.AccountName}', @_Day = {d}, @_Rank = {i.ID}, @_Type = 5, @_Prize = {getPrize(i.ID)}");
                    }

                    strQuery.AppendLine($"insert into event.SuccessRewardDay values ({d}, getdate())");
                }

                strQuery.AppendLine("commit transaction");
                strQuery.AppendLine("end try");
                strQuery.AppendLine("begin catch");
                strQuery.AppendLine("if @@trancount > 0 begin rollback transaction end;");
                strQuery.AppendLine("throw 50000, 'sql exception', 1");
                strQuery.AppendLine("end catch");

                NLogManager.LogMessage(strQuery.ToString());

                await LuckyDiceEventDAO.ExecuteSql(strQuery.ToString());


            }
            catch (Exception ex)
            {
                NLogManager.PublishException(ex);
            }
        }

        private int getPrize(int rank)
        {
            if (rank == 1)
                return 1000000;
            else if (rank == 2)
                return 500000;
            else if (rank == 3)
                return 300000;
            else if (rank >= 4 && rank <= 10)
                return 100000;
            else if (rank >= 11 && rank <= 20)
                return 50000;
            else if (rank >= 21 && rank <= 50)
                return 20000;
            else return 10000;
        }

        public static async Task EventInit()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<LuckyDiceHandler>().WithIdentity("LuckyDiceEvent", "LuckyDice").Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("LuckyDiceTrigger", "LuckyDice")
                .StartNow().WithCronSchedule("0 30 0 * * ? *").Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}