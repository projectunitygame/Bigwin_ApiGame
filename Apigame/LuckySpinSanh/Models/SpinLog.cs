using Newtonsoft.Json;
using System;

namespace LuckySpinSanh.Models
{
    public class SpinLog
    {
        public long SessionId { get; set; }
        [JsonIgnore]
        public long AccountId { get; set; }
        public int SmallResult { get; set; }
        public int BigResult { get; set; }
        public DateTime CreatedTime { get; set; }
        [JsonIgnore]
        public int CreatedDate { get; set; }
    }
}