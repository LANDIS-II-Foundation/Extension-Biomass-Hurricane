using System;
using System.Collections.Generic;
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
            return 0.0;
        }

        internal void ChangeSpeedsFromEnglishToMetric()
        {
            foreach(var species in this.theTable.Values)
            {
                foreach(var age in species.Values)
                {
                    var ageKeys = new List<double>(age.Keys);
                    foreach(var mph in ageKeys)
                    {
                        var prob = age[mph];
                        var kph = 1.60934 * mph;
                        age.Remove(mph);
                        age[kph] = prob;
                    }
                }
            }
        }
    }
}
