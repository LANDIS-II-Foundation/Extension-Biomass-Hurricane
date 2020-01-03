using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.BaseHurricane
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

        public double GetMortalityProbability(string species, double age, double windspeed)
        {
            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Mortality:  {0}:{1}, Wind={2}", species, age, windspeed);

            var speciesTable = this.theTable[species];
            
            // Note: The next two lines, running once per site, is very inefficent.
            // To optimize it (later) will require creating a new class so the keys
            // may be stored in sorted order.    All this is necessary because the order
            // of keys is not guaranteed in Dictionaries.
            var ages = new List<double>(speciesTable.Keys);
            ages.Sort();
            ages.Reverse();

            var ageToPick = 0.0;
            foreach(var tblAge in ages)
            {
                ageToPick = tblAge;
                if(age > tblAge)
                    break;
            }

            var speed_probabilityTable = speciesTable[ageToPick];
            var speeds = new List<double>(speed_probabilityTable.Keys);
            speeds.Sort();
            speeds.Reverse();
            var speedToUse = 0.0;
            foreach(var tblSpeed in speeds)
            {
                speedToUse = tblSpeed;
                if(windspeed > tblSpeed)
                    break;
            }

            return speed_probabilityTable[speedToUse];
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

        internal double MinimumWindSpeed
        {
            get
            {
                double returnValue = 100000.0;
                foreach(var species in this.theTable.Values)
                {
                    foreach(var age in species.Values)
                    {
                        var thisMin = age.Keys.Min();
                        returnValue = Math.Min(thisMin, returnValue);
                    }
                }

                if(returnValue >= 999.0)
                    returnValue = 0.0;

                return returnValue;
            }
        }
    }
}
