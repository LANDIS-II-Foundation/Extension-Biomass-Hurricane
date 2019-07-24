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

        [DataFieldAttribute(Desc = "Hurricane Year")]
        public int Year { get; set; }

        [DataFieldAttribute(Desc = "Hurricane Number")]
        public int Hnumber { get; set; }

        [DataFieldAttribute(Desc = "Study Area Max Windspeed")]
        public double StudyAreaMaxWS { get; set; }

        [DataFieldAttribute(Desc = "Study Area Min Windspeed")]
        public double StudyAreaMinWS { get; set; }

        //[DataFieldAttribute(Desc = "Max Windspeed at Landfall")]
        //public string landfallMaxWindspeed { get; set; }

        [DataFieldAttribute(Desc = "Impacts the Study Area?")]
        public string ImpactsStudyArea { get; set; }

        //[DataFieldAttribute(Desc = "Latitude Offset")]
        //public double latitudeOffset { get; set; }

        //[DataFieldAttribute(Desc = "Initiation Row")]
        //public int InitRow { set; get; }

        //[DataFieldAttribute(Desc = "Initiation Column")]
        //public int InitColumn { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Total Number of Sites in Event")]
        //public int TotalSites { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Damaged Sites in Event")]
        //public int DamagedSites { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Number of Cohorts Killed")]
        //public int CohortsKilled { set; get; }

        //[DataFieldAttribute(Unit = FieldUnits.Severity_Rank, Desc = "Mean Severity (1-5)", Format="0.00")]
        //public double MeanSeverity { set; get; }

    }
}
