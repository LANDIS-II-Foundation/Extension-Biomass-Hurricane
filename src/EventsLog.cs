using Landis.Library.Metadata;

namespace Landis.Extension.Hurricane
{
    public class EventsLog
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "...")]
        public int Time { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Hurricane Number")]
        public int HurricaneNumber { get; set; }

        [DataFieldAttribute(Desc = "X Coordinate of Landfall")]
        public double LandfallX { get; set; }

        [DataFieldAttribute(Desc = "Y Coordinate of Landfall")]
        public double LandfallY { get; set; }

        [DataFieldAttribute(Desc = "Max Windspeed at Landfall")]
        public double LandfallMaxWindSpeed { get; set; }

        [DataFieldAttribute(Desc = "Storm Path Direction (Heading)")]
        public double PathHeading { get; set; }

        [DataFieldAttribute(Desc = "Study Area Max Windspeed")]
        public double StudyAreaMaxWS { get; set; }

        [DataFieldAttribute(Desc = "Study Area Min Windspeed")]
        public double StudyAreaMinWS { get; set; }

        [DataFieldAttribute(Desc = "Impacts the Study Area?")]
        public bool ImpactsStudyArea { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed")]
        public int CohortKilled { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.g_B_m2, Desc = "Biomass Mortality")]
        public int BiomassMortality { get; set; }


    }
}
