//  Authors:  Robert M. Scheller

using System.Collections.Generic;
using Landis.SpatialModeling;
using Landis.Library.UniversalCohorts;
namespace Landis.Extension.Hurricane
{
    public static class SiteVars
    {
        public static ISiteVar<Dictionary<int, int>> WindExposure;
        public static ISiteVar<double> AgeRichness;
        public static ISiteVar<int> BiomassMortality;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            Event = PlugIn.ModelCore.Landscape.NewSiteVar<HurricaneEvent>(InactiveSiteMode.DistinctValues);
            TimeOfLastEvent = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            Disturbed = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            WindSpeed = PlugIn.ModelCore.Landscape.NewSiteVar<double>(InactiveSiteMode.DistinctValues);
            WindExposure = PlugIn.ModelCore.Landscape.NewSiteVar<Dictionary<int, int>>();
            BiomassMortality = PlugIn.ModelCore.Landscape.NewSiteVar<int>();

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.TimeOfLastEvent, "Hurricane.TimeOfLastEvent");

            Cohorts = PlugIn.ModelCore.GetSiteVar<SiteCohorts>("Succession.UniversalCohorts");

            AgeRichness = PlugIn.ModelCore.GetSiteVar<double>("Output.AgeRichness");

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                WindExposure[site] = new Dictionary<int, int>();
            }

        }

        //---------------------------------------------------------------------
        public static ISiteVar<SiteCohorts> Cohorts { get; private set; }
        public static ISiteVar<HurricaneEvent> Event { get; private set; }
        public static ISiteVar<int> TimeOfLastEvent { get; private set; }
        public static ISiteVar<double> WindSpeed {get; set;}
        public static ISiteVar<bool> Disturbed { get; private set; }
    }       
}
