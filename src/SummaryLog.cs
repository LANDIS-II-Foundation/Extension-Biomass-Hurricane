using Landis.Library.Metadata;

namespace Landis.Extension.BaseHurricane
{
    public class SummaryLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Time")]
        public int Time { set; get; }

        //[DataFieldAttribute(Desc = "Total Sites")]
        //public int TotalSitesDisturbed { set; get; }

        [DataFieldAttribute(Desc = "Damaged Sites")]
        public int NumberEvents { set; get; }

    }
}
