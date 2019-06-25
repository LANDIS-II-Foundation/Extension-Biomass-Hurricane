using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    class HurricaneEvent : ICohortDisturbance
    {
        private ActiveSite currentSite;

        private static WindSpeedGenerator windSpeedGenerator = null;

        public int hurricaneNumber { get; set; }
        public double landfallMaxWindSpeed { get; set; }
        public double distanceInlandToCenterPoint { get; set; }
        public double stormTrackEffectiveDistanceToCenterPoint { get; set; }
        public double stormTrackHeading { get; set; }

        public HurricaneEvent(int hurricaneNumber, WindSpeedGenerator windSpeedGenerator, double distanceInland)
        {
            this.hurricaneNumber = hurricaneNumber;
            this.landfallMaxWindSpeed = windSpeedGenerator.getWindSpeed();
            this.distanceInlandToCenterPoint = distanceInland;
            this.stormTrackHeading = 80.0 * PlugIn.ModelCore.GenerateUniform() + 280.0;
            var modHeading = (this.stormTrackHeading - 315) * Math.PI / 180.0;
            this.stormTrackEffectiveDistanceToCenterPoint = 
                this.distanceInlandToCenterPoint / Math.Cos(modHeading);
        }

        ExtensionType IDisturbance.Type => PlugIn.ExtType;
        ActiveSite IDisturbance.CurrentSite => this.currentSite;
        public bool MarkCohortForDeath(ICohort cohort)
        {
            return false;
        }

    }

    /*      *   /
    private void testWindGenerationDistribution()
    {
        var testThing = new List<double>();
        foreach(var i in Enumerable.Range(0, 10000))
            testThing.Add(this.windSpeedGenerator.getWindSpeed());
        double av = testThing.Average();
        Dictionary<double, int> bins = new Dictionary<double, int>();
        foreach(var val in testThing)
        {
            if(bins.ContainsKey(val))
                bins[val]++;
            else
                bins[val] = 1;
        }
        var keys = bins.Keys.ToList();
        keys.Sort();
        var sortedBins = new Dictionary<double, int>();
        foreach(var val in keys)
            sortedBins[val] = bins[val];
        bins = null;
    } /* */

    // testWindGenerationDistribution();

    /// <summary>
    /// Generate random wind speeds on a log-normal distribution, mu=0, sigma=0.4.
    /// The consuming code instantiates with minimum value, which is projected to 0,
    /// the requested mode value, and the maximum value.
    /// Maximum value is enforced by getting another number if the generated one
    /// is too high.
    /// </summary>
    class WindSpeedGenerator
    {
        private double minSpeed { get; set; }
        private double modeSpeed { get; set; }
        private double maxSpeed { get; set; }
        private double sigma { get; set; }
        private double mu { get; set; }
        private double adjustFactor { get; set; }

        private double mode
        {
            get { return Math.Exp(this.mu - this.sigma * this.sigma); }
        }

        internal WindSpeedGenerator(double minSpeed, double modeSpeed, double maxSpeed)
        {
            this.minSpeed = minSpeed;
            this.modeSpeed = modeSpeed;
            this.maxSpeed = maxSpeed;
            this.sigma = 0.4;  // Hard-coded for now.
            this.mu = 0.0;     // Hard-coded for now.
            this.adjustFactor = (this.modeSpeed - minSpeed) / this.mode;
        }

        public double getWindSpeed()
        {
            PlugIn.ModelCore.LognormalDistribution.Mu = this.mu;
            PlugIn.ModelCore.LognormalDistribution.Sigma = this.sigma;
            bool keepComputing = true;
            while(keepComputing)
            {
                double trialValue = PlugIn.ModelCore.LognormalDistribution.NextDouble();
                trialValue = Math.Round((this.adjustFactor * trialValue)) + this.minSpeed;
                if(trialValue <= this.maxSpeed)
                    return trialValue;
            }
            return -1.0;
        }
    }
}
