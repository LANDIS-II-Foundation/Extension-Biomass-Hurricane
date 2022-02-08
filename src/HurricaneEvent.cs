using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BaseHurricane
{
    public class HurricaneEvent : ICohortDisturbance
    {
        private ActiveSite currentSite;

        internal static WindSpeedGenerator WindSpeedGenerator { get; set; } = null;
        public static HurricaneWindMortalityTable WindMortalityTable { get; set; } = null;
        private static double BaseWindSpeed = 48.0; // Asymptotic minimum max wind speed of a storm.
        public static double MinimumWSforDamage { get; set; }

        public static bool HurricaneRandomNumber { get; set; } = false;

        //public int hurricaneYear { get; set; }
        public int hurricaneNumber { get; set; }
        public double StudyAreaMaxWindspeed { get; set; }
        public double StudyAreaMinWindspeed { get; set; }
        public bool studyAreaImpacts { get; set; }
        public double landfallMaxWindSpeed { get; private set; }
        public double landfallLatitude { get; private set; }

        //private double stormTrackEffectiveDistanceToCenterPoint { get; set; }
        public double stormTrackHeading { get; private set; }
        private double stormTrackSlope { get; set; }
        private double stormTrackPerpandicularSlope { get; set; }
        public Point LandfallPoint { get; private set; }
        public Line StormTrackLine { get; private set; }
        private double stormTrackLengthTo_b { get; set; }
        //internal ContinentalGrid ContinentalGrid { get; private set; }
        public int ClosestExposureKey;

        public double landfallDistanceFromCoastalCenterY = 0.0;

        //---------------------------------------------------------------------

        static HurricaneEvent()
        {
        }
        //---------------------------------------------------------------------
        ExtensionType IDisturbance.Type
        {
            get
            {
                return PlugIn.ExtType;
            }
        }
        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get
            {
                return currentSite;
            }
        }


        public static HurricaneEvent Initiate()//ContinentalGrid continentalGrid)
        {

            HurricaneEvent hurrEvent = new HurricaneEvent(); // continentalGrid);
            return hurrEvent;
        }

        private HurricaneEvent()//ContinentalGrid continentalGrid)
        {
            //this.ContinentalGrid = continentalGrid;
            this.landfallMaxWindSpeed = HurricaneEvent.WindSpeedGenerator.GetWindSpeed();
            if (HurricaneRandomNumber)
            {
                PlugIn.HurricaneGeneratorNormal.Mu = 0.0;
                PlugIn.HurricaneGeneratorNormal.Sigma = 0.0;
                landfallDistanceFromCoastalCenterY = PlugIn.ModelCore.NormalDistribution.NextDouble();
                PlugIn.HurricaneGeneratorNormal.Mu = 0.0;
                PlugIn.HurricaneGeneratorNormal.Sigma = 0.0;
                this.stormTrackHeading = PlugIn.ModelCore.NormalDistribution.NextDouble();

                //this.landfallLatitude = 7.75 * PlugIn.HurricaneGeneratorStandard.NextDouble() + 30.7;  // VERSION2
                //this.stormTrackHeading = 80.0 * PlugIn.HurricaneGeneratorStandard.NextDouble() + 280.0;  // VERSION2
            }
            else
            {
                PlugIn.ModelCore.NormalDistribution.Mu = 0.0;
                PlugIn.ModelCore.NormalDistribution.Sigma = 0.0;
                landfallDistanceFromCoastalCenterY = PlugIn.ModelCore.NormalDistribution.NextDouble();
                PlugIn.ModelCore.NormalDistribution.Mu = 0.0;
                PlugIn.ModelCore.NormalDistribution.Sigma = 0.0;
                this.stormTrackHeading = PlugIn.ModelCore.NormalDistribution.NextDouble();
                
                //this.landfallLatitude = 7.75 * PlugIn.ModelCore.GenerateUniform() + 30.7;  // VERSION2
                //this.stormTrackHeading = 80.0 * PlugIn.ModelCore.GenerateUniform() + 280.0; // VERSION2
            }

            // Find the closest exposure map key VERSION2
            foreach (int ExposureKey in PlugIn.WindExposures)
            {
                int minimumDifference = 100;
                int tempD = Math.Abs(ExposureKey - (int) this.stormTrackHeading);
                if (tempD < minimumDifference)
                        this.ClosestExposureKey = ExposureKey;
            }
            PlugIn.ModelCore.UI.WriteLine("StormHeading={0}, ClosestExposureKey={1}", this.stormTrackHeading, this.ClosestExposureKey);

            //var modHeading = (this.stormTrackHeading - 315) * Math.PI / 180.0;
            this.stormTrackSlope = 1 / Math.Tan(this.stormTrackHeading * Math.PI / 180.0);
            this.stormTrackPerpandicularSlope = -1.0 / this.stormTrackSlope;

            double metersPerDegreeLat = 111000.0; 
            double studyAreaHeightMeters = PlugIn.ModelCore.Landscape.Dimensions.Rows * PlugIn.ModelCore.CellLength;
            double studyAreaHeightDegreesLatitude = studyAreaHeightMeters / metersPerDegreeLat;

            double landfallY = PlugIn.CoastalCenterY + landfallDistanceFromCoastalCenterY;
            this.LandfallPoint = PlugIn.CoastLine.GivenYGetPoint(landfallY);
            this.StormTrackLine = new Line(this.LandfallPoint, this.stormTrackSlope);

            //double landfallY = this.ContinentalGrid.ConvertLatitudeToGridUnits(this.landfallLatitude);
            //var stormTrackInterceptPt = this.StormTrackLine.GivenXGetPoint(0.0);
            //this.stormTrackLengthTo_b = (this.LandfallPoint - stormTrackInterceptPt).Length;
        }

        public (double, double) GetDistanceAndOffset(Point pt)
        {
            double landfallPtDx = this.LandfallPoint.X;
            double landfallPtDy = this.LandfallPoint.Y - this.StormTrackLine.yIntercept;
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
            double y = b / ( (a2 + x2) * Math.Sqrt(1.0 + (x2 / a2)) );
            
            return y;
        }

        /// <summary>
        /// Field equation of maximum wind speed by returning a speed when given a distance
        /// down the track (x) and the distance lateral to the track (offset).
        /// The model is implemented as 
        /// </summary>
        /// <param name="x">Distance down the storm track from landfall (meters).</param>
        /// <param name="offset">Signed perpandicular distance from the track to the point
        /// of interest. (meters)</param>
        /// <param name="PeakSpeed">Wind speed at landfall</param>
        /// <param name="a">From hyperbola a; 2 * the inflection point distance. 
        /// Unit is kilometers, then the value is adjusted to be in meters. </param>
        /// <returns>Maximum wind speed at the given point.</returns>
        public double ComputeMaxWindSpeed(double x, double offset, double a=360.0)
        {
            double PeakSpeed = this.landfallMaxWindSpeed;
            double baseSpeed = HurricaneEvent.BaseWindSpeed;
            double Pb = PeakSpeed - baseSpeed;
            a *= 1000.0;
            double b = Pb * a * a;

            double speedAtOffset0 = this.secondDerivHyperobla(x: x, a: a, b: b) + baseSpeed;

            /* Bookmark: Adjust 'a' for side winds */
            if(offset > 0.0)
                a *= 0.667;
            else
                a *= 0.45;

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

        internal bool GenerateWindFieldRaster(
            string mapNameTemplate, ICore modelCore)//, ContinentalGrid continentalGrid)
        {
            //this.ContinentalGrid = continentalGrid;
            Dimensions dimensions = new Dimensions(modelCore.Landscape.Rows, modelCore.Landscape.Columns);
            int columns = modelCore.Landscape.Columns;
            int rows = modelCore.Landscape.Rows;
            double lowerLeftWindspeed = this.GetWindSpeed(0, 0);
            double lowerRightWindSpeed = this.GetWindSpeed(columns, 0);
            double upperRightWindSpeed = this.GetWindSpeed(columns, rows);
            double upperLeftWindSpeed = this.GetWindSpeed(0, rows);
            double maxWS = (new[] { lowerLeftWindspeed, lowerRightWindSpeed, upperRightWindSpeed, upperLeftWindSpeed }).Max();
            double minWS = (new[] { lowerLeftWindspeed, lowerRightWindSpeed, upperRightWindSpeed, upperLeftWindSpeed }).Min();
            this.studyAreaImpacts = true;
            if (maxWS < HurricaneEvent.MinimumWSforDamage)
            {
                //PlugIn.ModelCore.UI.WriteLine("   Hurricane Not Sufficient to Cause Damage:  MaxWS={0}, HurricaneMinWS={1}", maxWS, MinimumWSforDamage);
                this.studyAreaImpacts = false;
                return false;
            }


            SiteVars.WindSpeed.ActiveSiteValues = 0.0;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape.ActiveSites)
            {
                currentSite = site;
                SiteVars.WindSpeed[currentSite] = this.GetModifiedWindSpeed(site.Location.Column, site.Location.Row);
                KillSiteCohorts(currentSite);
            }

            double activeAreaMinWS = 999.0;
            double activeAreaMaxWS = 0.0;

            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, modelCore.CurrentTime);
            IOutputRaster<BytePixel> outputRaster = null;
            using (outputRaster = modelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach(Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if(site.IsActive)
                    {
                        double windspeed = SiteVars.WindSpeed[site];
                        pixel.MapCode.Value = (byte)windspeed;
                        if(windspeed < activeAreaMinWS) activeAreaMinWS = windspeed;
                        if(windspeed > activeAreaMaxWS) activeAreaMaxWS = windspeed;
                    }
                    else
                    {
                        //  Inactive site
                        pixel.MapCode.Value = (byte)0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }
            this.StudyAreaMinWindspeed = activeAreaMinWS;
            this.StudyAreaMaxWindspeed = activeAreaMaxWS;
            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Caused Damage:  AreaMaxWS={0}, AreaMinWS={1}", activeAreaMaxWS, activeAreaMinWS);
            return true;

        }

        public double GetWindSpeed(int column, int row)
        {
            Site site = PlugIn.ModelCore.Landscape.GetSite(new Location(row, column));
            Point pt = Point.siteIndexToCoordinates(column, row); //this.ContinentalGrid.siteIndexToCoordinates(column, row);
            double max_speed = this.GetMaxWindSpeedAtPoint(pt);

            return max_speed;
        }
        public double GetModifiedWindSpeed(int column, int row)  // VERSION2
        {
            Site site = PlugIn.ModelCore.Landscape.GetSite(new Location(row, column));
            Point pt = Point.siteIndexToCoordinates(column, row);
            double max_speed = this.GetMaxWindSpeedAtPoint(pt);

            max_speed = max_speed * CalculateWindSpeedReduction(SiteVars.WindExposure[site][this.ClosestExposureKey]); //VERISION 2

            return max_speed;
        }
        //---------------------------------------------------------------------

        private void KillSiteCohorts(ActiveSite site)
        {
            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
        }
        //---------------------------------------------------------------------

        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            var windSpeed = SiteVars.WindSpeed[this.currentSite];
            var name = cohort.Species.Name;
            var age = cohort.Age;

            var deathLiklihood = HurricaneEvent.WindMortalityTable.GetMortalityProbability(cohort.Species.Name, cohort.Age, SiteVars.WindSpeed[this.currentSite]);

            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Mortality:  {0}:{1}, Wind={2}, Pmort={3}", name, age, windSpeed, deathLiklihood);

            var randomVar = PlugIn.ModelCore.GenerateUniform();

            return (randomVar <= deathLiklihood);
        }
        private double CalculateWindSpeedReduction(int exposureIndex)
        {
            switch (exposureIndex)
            {
                case 1:
                    return 0.1;
                case 2:
                    return 0.2;
                case 3:
                    return 0.3;
                case 4:
                    return 0.4;
                case 5:
                    return 0.5;
                case 6:
                    return 0.6;
                case 7:
                    return 0.7;
                case 8:
                    return 0.8;
                case 9:
                    return 0.9;

                default:
                    break;
            }
            return 1.0;
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

        public double GetWindSpeed()
        {
            if(PlugIn.ModelCore == null)
                return this.modeSpeed;

            PlugIn.ModelCore.LognormalDistribution.Mu = this.mu;
            PlugIn.ModelCore.LognormalDistribution.Sigma = this.sigma;
            PlugIn.HurricaneGeneratorLogNormal.Mu = this.mu;
            PlugIn.HurricaneGeneratorLogNormal.Sigma = this.sigma;
            bool keepComputing = true;
            //for (int i = 0; i<10; i++)
            //{
            //    double testValue = PlugIn.HurricaneGeneratorLogNormal.NextDouble();
            //        PlugIn.ModelCore.UI.WriteLine("   LogNormal generator:  {0}", testValue);
            //}
            while(keepComputing)
            {
                double trialValue = PlugIn.ModelCore.LognormalDistribution.NextDouble();
                //int cnt = 0;
                if (HurricaneEvent.HurricaneRandomNumber)
                {
                    
                    trialValue = PlugIn.HurricaneGeneratorLogNormal.NextDouble();
                    //if(cnt < 10)
                    //    PlugIn.ModelCore.UI.WriteLine("   LogNormal generator:  {0}", trialValue);
                    //cnt++;

                }

                trialValue = Math.Round((this.adjustFactor * trialValue)) + this.minSpeed;
                if(trialValue <= this.maxSpeed)
                    return trialValue;
            }
            return -1.0;
        }
    }
}
