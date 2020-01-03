using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.BaseHurricane
{
    public class EventsLog
    {
        //log.WriteLine("Time,Initiation Site,Total Sites,Damaged Sites,Cohorts Killed,Mean Severity");

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "...")]
        public int Time { get; set; }

        //[DataFieldAttribute(Desc = "Hurricane Year")]
        //public int Year { get; set; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Hurricane Number")]
        public int HurricaneNumber { get; set; }

        [DataFieldAttribute(Desc = "Latitude at Landfall")]
        public double LandfallLatitude { get; set; }

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


    }
}
