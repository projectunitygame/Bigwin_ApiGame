using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PTCN.CrossPlatform.Minigame.LuckyDice.Handlers.BotHandler
{
    public class BotConfiguration
    {
        public int MinBot { get; set; }
        public int MaxBot { get; set; }

        public int NumRichBot { get; set; }

        public int NumNormalBot { get; set; }

        public int NumPoorBot { get; set; }
        
        public int VipChangeRate { get; set; }

        public int NorChangeRate { get; set; }

        public int PoorChangeRate { get; set; }

        public int MinTimeChange { get; set; }

        public int MaxTimeChange { get; set; }
        
        public bool Enable { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var config = (BotConfiguration) obj;
            return MinBot.Equals(config.MinBot) &&
                   MaxBot.Equals(config.MaxBot) &&
                   NumRichBot.Equals(config.NumRichBot) &&
                   NumNormalBot.Equals(config.NumNormalBot) &&
                   NumPoorBot.Equals(config.NumPoorBot);
        }
    }

    public static class JsonConvertExtension
    {
        public static bool JSONEquals(this object obj, object another)
        {
            if (ReferenceEquals(obj, another)) return true;
            if ((obj == null) || (another == null)) return false;
            if (obj.GetType() != another.GetType()) return false;

            var objJson = JsonConvert.SerializeObject(obj);
            var anotherJson = JsonConvert.SerializeObject(another);

            return objJson == anotherJson;
        }
    }
}