using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.BiomassHurricane
{
    public class HurricaneWindMortalityTable
    {
        private Dictionary<string, Dictionary<double, Dictionary<double, double>>>
            theTable { get; set; }

        public HurricaneWindMortalityTable
            (Dictionary<string, Dictionary<double, Dictionary<double, double>>> windSpeedVulnverabilities)
        {
            this.theTable = windSpeedVulnverabilities;
        }

        public double GetMortalityProbability(string species, double cohort_age, double windspeed)
        {
            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Mortality:  {0}:{1}, Wind={2}", species, age, windspeed);

            var speciesTable = this.theTable[species];
            
            // Note: The next two lines, running once per site, is very inefficent.
            // To optimize it (later) will require creating a new class so the keys
            // may be stored in sorted order.    All this is necessary because the order
            // of keys is not guaranteed in Dictionaries.
            List<double> species_age_categories = new List<double>(speciesTable.Keys);
            species_age_categories.Sort();
            double final_probability = 0.0;
            species_age_categories.Reverse();

            double matching_age = 0;
            foreach(var tblAge in species_age_categories)
            {
                if (cohort_age > tblAge)
                {
                    matching_age = tblAge;
                    break;
                }
            }

            if (matching_age == 0)  // in this case, there are no table entries for such young cohorts, assume no mortality.
                return 0.0;

            var speed_probabilityTable = speciesTable[matching_age];
            var speeds = new List<double>(speed_probabilityTable.Keys);
            speeds.Sort();
            speeds.Reverse();
            double matching_speed = 0.0;
            foreach(var tblSpeed in speeds)
            {
                if(windspeed > tblSpeed)
                {
                    matching_speed = tblSpeed;
                    break;
                }
            }

            if (matching_speed <= 0.0) // in this case, there are no table entries for such low wind speeds, assume no mortality.
                return 0.0;

            final_probability = speed_probabilityTable[matching_speed];

            return final_probability;
        }

        //internal void ChangeSpeedsFromEnglishToMetric()
        //{
        //    foreach(var species in this.theTable.Values)
        //    {
        //        foreach(var age in species.Values)
        //        {
        //            var ageKeys = new List<double>(age.Keys);
        //            foreach(var mph in ageKeys)
        //            {
        //                var prob = age[mph];
        //                var kph = 1.60934 * mph;
        //                age.Remove(mph);
        //                age[kph] = prob;
        //            }
        //        }
        //    }
        //}

        //internal double MinimumWindSpeed
        //{
        //    get
        //    {
        //        double returnValue = 100000.0;
        //        foreach(var species in this.theTable.Values)
        //        {
        //            foreach(var age in species.Values)
        //            {
        //                var thisMin = age.Keys.Min();
        //                returnValue = Math.Min(thisMin, returnValue);
        //            }
        //        }

        //        if(returnValue >= 999.0)
        //            returnValue = 0.0;

        //        return returnValue;
        //    }
        //}
    }
}
