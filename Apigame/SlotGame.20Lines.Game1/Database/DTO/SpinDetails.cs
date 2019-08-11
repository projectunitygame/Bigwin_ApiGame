using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlotGame._20Lines.Game1.Database.DTO
{

    public class SpinDetail
    {
        public long SpinID { get; set; }
        public int LineID { get; set; }
        public int Multiplier { get; set; }
        public long PrizeValue { get; set; }
        public int PrizeID { get; set; }
    }

    public class SpinDetailList
    {
        public List<SpinDetail> DetailSpin { get; set; }
        public string LineData { get; set; }
    }
}
