//  Authors:  Robert M. Scheller

using Landis.Core;
using System.Collections.Generic;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
//using Landis.Library.Biomass;
using System;

namespace Landis.Extension.BiomassHurricane
{
    public static class SiteVars
    {
        private static ISiteVar<HurricaneEvent> eventVar;
        private static ISiteVar<int> timeOfLastEvent;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<ISiteCohorts> cohorts;
        public static ISiteVar<Dictionary<int, int>> WindExposure;
        public static ISiteVar<double> AgeRichness;
        public static ISiteVar<int> BiomassMortality;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            eventVar = PlugIn.ModelCore.Landscape.NewSiteVar<HurricaneEvent>(InactiveSiteMode.DistinctValues);
            timeOfLastEvent = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            WindSpeed = PlugIn.ModelCore.Landscape.NewSiteVar<double>(InactiveSiteMode.DistinctValues);
            WindExposure = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastEvent, "Hurricane.TimeOfLastEvent");

            //cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.AgeCohorts");
            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.BiomassCohorts");

            AgeRichness = PlugIn.ModelCore.GetSiteVar<double>("Output.AgeRichness");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                WindExposure[site] = new Dictionary<int, int>();
            }

        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<HurricaneEvent> Event
        {
            get
            {
                return eventVar;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLastEvent
        {
            get
            {
                return timeOfLastEvent;
            }
        }

        //---------------------------------------------------------------------

        //public static ISiteVar<byte> Severity
        //{
        //    get {
        //        return severity;
        //    }
        //}

        public static ISiteVar<double> WindSpeed
        {
            get; set;
        }
        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed
        {
            get
            {
                return disturbed;
            }
        }
    }       
    }
