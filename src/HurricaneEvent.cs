using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    public class HurricaneEvent : ICohortDisturbance
    {
        private ActiveSite currentSite;

        private static WindSpeedGenerator windSpeedGenerator = null;

        public int hurricaneNumber { get; set; }
        public double landfallMaxWindSpeed { get; set; }
        public double landfallLatitude { get; set; }
        public double distanceInlandToCenterPoint { get; set; }
        public double stormTrackEffectiveDistanceToCenterPoint { get; set; }
        public double stormTrackHeading { get; set; }
        private double stormTrackSlope { get; set; }
        private double stormTrackPerpandicularSlope { get; set; }
        public Point LandfallPoint { get; private set; }
        public Line StormTrackLine { get; private set; }
        internal ContinentalGrid ContinentalGrid { get; private set; }

        public HurricaneEvent(int hurricaneNumber, WindSpeedGenerator windSpeedGenerator, 
            double distanceInland, ContinentalGrid continentalGrid)
        {
            this.hurricaneNumber = hurricaneNumber;
            this.landfallMaxWindSpeed = windSpeedGenerator.getWindSpeed();
            this.landfallLatitude = 34.3;   /// For unit testing only.
            if(PlugIn.ModelCore != null)
                this.landfallLatitude = 7.75 * PlugIn.ModelCore.GenerateUniform() + 30.7;
            this.distanceInlandToCenterPoint = distanceInland;
            this.stormTrackHeading = 310.0;  /// For unit testing only.
            if(PlugIn.ModelCore != null)
                this.stormTrackHeading = 80.0 * PlugIn.ModelCore.GenerateUniform() + 280.0;
            var modHeading = (this.stormTrackHeading - 315) * Math.PI / 180.0;
            this.stormTrackEffectiveDistanceToCenterPoint = 
                this.distanceInlandToCenterPoint / Math.Cos(modHeading);
            this.stormTrackSlope = 1 / Math.Tan(this.stormTrackHeading * Math.PI / 180.0);
            this.stormTrackPerpandicularSlope = -1.0 / this.stormTrackSlope;
            double landfallY = continentalGrid.ConvertLatitudeToGridUnits(this.landfallLatitude);
            this.LandfallPoint = continentalGrid.CoastLine.GivenYGetPoint(landfallY);
            this.StormTrackLine = new Line(this.LandfallPoint, this.stormTrackSlope);
        }

        public (double, double) GetDistanceAndOffset(Point pt)
        {
            double landfallPtDx = this.LandfallPoint.X;
            double landfallPtDy = this.LandfallPoint.Y - this.StormTrackLine.b;
            double landfallDistanceToIntercept =
                Math.Sqrt(landfallPtDx * landfallPtDx + landfallPtDy * landfallPtDy);

            (double nearPtDistYintercept, double offset) = this.StormTrackLine.GetDistanceAndOffset(pt);
            double distNearPtToLandfall = landfallDistanceToIntercept - nearPtDistYintercept;

            return (distNearPtToLandfall, offset);
        }

        /// <summary>
        /// Equation for the second derivitive of the hyperbola.
        /// </summary>
        /// <returns></returns>
        private double secondDerivHyperobla(double x, double a, double b)
        {
            double a2 = Math.Pow(a, 2);
            double x2 = Math.Pow(x, 2);
            double y = b /
                (a2 + x2) * Math.Sqrt(1.0 + (x2 / a2));
            
            return y;
        }

        /// <summary>
        /// Field equation of maximum wind speed by returning a speed when given a distance
        /// down the track (x) and the distance lateral to the track (offset).
        /// The model is implemented as 
        /// </summary>
        /// <param name="x">Distance down the storm track from landfall.</param>
        /// <param name="offset">Signed perpandicular distance from the track to the point
        /// of interest.</param>
        /// <param name="PeakSpeed">Wind speed at landfall</param>
        /// <param name="a">From hyperbola a; 2 * the inflection point distance</param>
        /// <returns>Maximum wind speed at the given point.</returns>
        public double ComputeMaxWindSpeed(double x, double offset, double PeakSpeed=180.0, double a=360.0)
        {
            double baseSpeed = 48.0;
            double Pb = PeakSpeed - baseSpeed;
            double b = Pb * a * a;

            double speedAtOffset0 = this.secondDerivHyperobla(x: x, a: a, b: b) + baseSpeed;

            a = a / 2.0;
            Pb = speedAtOffset0 - baseSpeed;
            b = Pb * a * a;

            return this.secondDerivHyperobla(x: offset, a: a, b: b) + baseSpeed;
        }

        public double GetMaxWindSpeedAtPoint(Point point)
        {
            (var distance, var offset) = this.GetDistanceAndOffset(point);

            double speed = this.ComputeMaxWindSpeed(distance, offset);

            return speed;
        }

        ExtensionType IDisturbance.Type => PlugIn.ExtType;
        ActiveSite IDisturbance.CurrentSite => this.currentSite;
        public bool MarkCohortForDeath(ICohort cohort)
        {
            return false;
        }

        internal void GenerateWindFieldRaster(
            string mapNameTemplate, ICore modelCore, ContinentalGrid continentalGrid)
        {
            this.ContinentalGrid = continentalGrid;
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, modelCore.CurrentTime);
            Dimensions dimensions = new Dimensions(modelCore.Landscape.Rows, modelCore.Landscape.Columns);
            int columns = modelCore.Landscape.Columns;
            int rows = modelCore.Landscape.Rows;
            double lowerLeftWindspeed = this.GetWindSpeed(0, rows);
            double lowerRightWindSpeed = this.GetWindSpeed(columns, rows);
            double upperRightWindSpeed = this.GetWindSpeed(columns, 0);
            IOutputRaster<BytePixel> outputRaster = null;
            using(outputRaster = modelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach(Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if(site.IsActive)
                    {
                        if(SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte)(SiteVars.Severity[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

        }

        private double GetWindSpeed(int column, int row)
        {
            Point pt = this.ContinentalGrid.siteIndexToCoordinates(column, row);
            return 0.0;
            //this.
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
    public class WindSpeedGenerator
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

        public WindSpeedGenerator(double minSpeed, double modeSpeed, double maxSpeed)
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
            if(PlugIn.ModelCore == null)
                return this.modeSpeed;

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
