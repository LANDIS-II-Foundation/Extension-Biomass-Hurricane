using Landis.Core;
using Landis.Library.BiomassCohorts;
//using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.BiomassHurricane
{
    public class HurricaneEvent : IDisturbance
    {
        private Site currentSite;

        internal static WindSpeedGenerator WindSpeedGenerator { get; set; } = null;
        public static HurricaneWindMortalityTable WindMortalityTable { get; set; } = null;
        private static double BaseWindSpeed = 48.0; // Asymptotic minimum max wind speed of a storm. // FIXME VERSION 2
        public static double MinimumWSforDamage { get; set; }

        public static bool HurricaneRandomNumber { get; set; } = false;

        //public int hurricaneYear { get; set; }
        public int HurricaneNumber { get; set; }
        public double StudyAreaMaxWindspeed { get; set; }
        public double StudyAreaMinWindspeed { get; set; }
        public bool StudyAreaMortality { get; set; }
        public double LandfallMaxWindSpeed { get; private set; }
        public int CohortsKilled { get; set; }
        public double StormTrackHeading { get; private set; }
        public Point LandfallPoint { get; private set; }
        public Line StormTrackLine { get; private set; }
        public int ClosestExposureKey { get; }
        public int BiomassMortality { get; set; }
        //public bool CausedMortality { get; set; }

        private double stormTrackSlope; // { get; set; }
        private double stormTrackPerpandicularSlope; // { get; set; }

        public double landfallDistanceFromCoastalCenterY = 0.0;
        public int AvailableCohorts;
        
        //internal ContinentalGrid ContinentalGrid { get; private set; }
        //private double stormTrackEffectiveDistanceToCenterPoint { get; set; }
        //private double stormTrackLengthTo_b { get; set; }

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
                return (ActiveSite) currentSite;
            }
        }


        //public static HurricaneEvent Initiate()
        //{

        //    HurricaneEvent hurrEvent = new HurricaneEvent(0); 
        //    return hurrEvent;
        //}

        public HurricaneEvent(int stormCnt)
        {
            
            this.HurricaneNumber = stormCnt;
            this.LandfallMaxWindSpeed = HurricaneEvent.WindSpeedGenerator.GetWindSpeed();
            if (HurricaneRandomNumber)
            {
                // Landfall
                PlugIn.HurricaneGeneratorNormal.Mu = 0.0;
                PlugIn.HurricaneGeneratorNormal.Sigma = PlugIn.LandFallSigma;
                landfallDistanceFromCoastalCenterY = PlugIn.HurricaneGeneratorNormal.NextDouble();

                // Storm track
                PlugIn.HurricaneGeneratorNormal.Mu = PlugIn.StormDirectionMu;
                PlugIn.HurricaneGeneratorNormal.Sigma = PlugIn.StormDirectionSigma;
                this.StormTrackHeading = PlugIn.HurricaneGeneratorNormal.NextDouble();
            }
            else
            {
                // Landfall
                PlugIn.ModelCore.NormalDistribution.Mu = 0.0;
                PlugIn.ModelCore.NormalDistribution.Sigma = PlugIn.LandFallSigma;
                landfallDistanceFromCoastalCenterY = PlugIn.ModelCore.NormalDistribution.NextDouble();

                // Storm track
                PlugIn.ModelCore.NormalDistribution.Mu = PlugIn.StormDirectionMu;
                PlugIn.ModelCore.NormalDistribution.Sigma = PlugIn.StormDirectionSigma;
                this.StormTrackHeading = PlugIn.ModelCore.NormalDistribution.NextDouble();
            }

            if (this.StormTrackHeading > 360)
                this.StormTrackHeading -= 360;

            // Find the closest exposure map key VERSION2
            int minimumDifference = 181;  // two degrees can't be absolutely more than 181 degree apart.
            foreach (int ExposureKey in PlugIn.WindExposures)
            {
                int degreeDifference = Math.Abs(Math.Abs(ExposureKey - (int) this.StormTrackHeading) - 360);
                if (degreeDifference > 180)
                    degreeDifference = Math.Abs(360 - degreeDifference);
                if (degreeDifference < minimumDifference)
                {
                    this.ClosestExposureKey = ExposureKey;
                    minimumDifference = degreeDifference;
                }
            }
            PlugIn.ModelCore.UI.WriteLine("Storm #{0}, MaxWindSpeed={1:0.0}, StormHeading={2:0.0}, ClosestExposureKey={3}", this.HurricaneNumber, this.LandfallMaxWindSpeed, this.StormTrackHeading, this.ClosestExposureKey);

            this.stormTrackSlope = 1 / Math.Tan(this.StormTrackHeading * Math.PI / 180.0);
            this.stormTrackPerpandicularSlope = -1.0 / this.stormTrackSlope;

            double studyAreaHeightMeters = PlugIn.ModelCore.Landscape.Dimensions.Rows * PlugIn.ModelCore.CellLength;

            double landfallY = PlugIn.CoastalCenterY + landfallDistanceFromCoastalCenterY;
            this.LandfallPoint = PlugIn.CoastLine.GivenYGetPoint(landfallY);  
            this.StormTrackLine = new Line(this.LandfallPoint, this.stormTrackSlope);




            //double landfallY = this.ContinentalGrid.ConvertLatitudeToGridUnits(this.landfallLatitude);
            //var stormTrackInterceptPt = this.StormTrackLine.GivenXGetPoint(0.0);
            //this.stormTrackLengthTo_b = (this.LandfallPoint - stormTrackInterceptPt).Length;
            //double studyAreaHeightDegreesLatitude = studyAreaHeightMeters / metersPerDegreeLat;
            //double metersPerDegreeLat = 111000.0; 
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
            double PeakSpeed = this.LandfallMaxWindSpeed;
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

        internal bool HurricaneDisturb()
        {
            //this.ContinentalGrid = continentalGrid;
            SiteVars.BiomassMortality.ActiveSiteValues = 0;

            Dimensions dimensions = new Dimensions(PlugIn.ModelCore.Landscape.Rows, PlugIn.ModelCore.Landscape.Columns);
            int columns = PlugIn.ModelCore.Landscape.Columns;
            int rows = PlugIn.ModelCore.Landscape.Rows;
            double lowerLeftWindspeed = this.GetModifiedWindSpeed(0, 0);
            double lowerRightWindSpeed = this.GetModifiedWindSpeed(columns, 0);
            double upperRightWindSpeed = this.GetModifiedWindSpeed(columns, rows);
            double upperLeftWindSpeed = this.GetModifiedWindSpeed(0, rows);
            double maxWS = (new[] { lowerLeftWindspeed, lowerRightWindSpeed, upperRightWindSpeed, upperLeftWindSpeed }).Max();
            double minWS = (new[] { lowerLeftWindspeed, lowerRightWindSpeed, upperRightWindSpeed, upperLeftWindSpeed }).Min();
            this.StudyAreaMortality = true;
            if (maxWS < HurricaneEvent.MinimumWSforDamage)
            {
                //PlugIn.ModelCore.UI.WriteLine("   Hurricane Not Sufficient to Cause Damage:  MaxWS={0}, HurricaneMinWS={1}", maxWS, MinimumWSforDamage);
                this.StudyAreaMortality = false;
                return false;
            }

            int siteCohortsKilled = 0;
            SiteVars.WindSpeed.SiteValues = 0.0;

            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
            {
                this.currentSite = site;
                double standConditionsWindReduction = CalculateWindReduction(site);
                SiteVars.WindSpeed[site] = this.GetModifiedWindSpeed(site.Location.Column, site.Location.Row);
                SiteVars.WindSpeed[site] *= standConditionsWindReduction;

                if (site.IsActive)
                {
                    siteCohortsKilled = Damage((ActiveSite)site);
                    //PlugIn.ModelCore.UI.WriteLine("   Hurricane Caused Damage:  WindSpeed={0}, StandConditions={1}.", SiteVars.WindSpeed[site], standConditionsWindReduction);
                }
                //KillSiteCohorts((ActiveSite) currentSite);
            }

            double activeAreaMinWS = 999.0;
            double activeAreaMaxWS = 0.0;

            string path = MapNames.ReplaceTemplateVars(@"Hurricane\wind-speeds-{timestep}-{stormNumber}.img", PlugIn.ModelCore.CurrentTime, HurricaneNumber);
            IOutputRaster<BytePixel> outputRaster = null;
            using (outputRaster = PlugIn.ModelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach(Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    double windspeed = SiteVars.WindSpeed[site];
                    pixel.MapCode.Value = (byte)windspeed;
                    if (site.IsActive)
                    {
                        if (windspeed < activeAreaMinWS) activeAreaMinWS = windspeed;
                        if (windspeed > activeAreaMaxWS) activeAreaMaxWS = windspeed;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }
            this.StudyAreaMinWindspeed = activeAreaMinWS;
            this.StudyAreaMaxWindspeed = activeAreaMaxWS;
            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Caused Damage:  AreaMaxWS={0}, AreaMinWS={1}", activeAreaMaxWS, activeAreaMinWS);
            return true;

        }

        // The purpose of this function is to allow other outputs to influence wind speed.
        // For example, AgeEvenness (0-100) is paired with a maximum that determines how much wind speed should be reduced.
        public double CalculateWindReduction(Site site)
        {
            if (PlugIn.WindSpeedReductions == null)
                return 1.0;
            double priorWindSpeed = SiteVars.WindSpeed[site];
            double rangeMax = 0.0;
            double reduction = 0.0;

            foreach (IWindSpeedModificationTable wsr in PlugIn.WindSpeedReductions)
            {
                if (wsr.RangeMaximum > rangeMax)
                    reduction = wsr.FractionWindReduction;
            }

            return reduction;
        }

        //public double GetWindSpeed(int column, int row)
        //{
        //    Site site = PlugIn.ModelCore.Landscape.GetSite(new Location(row, column));
        //    Point pt = Point.siteIndexToCoordinates(column, row); //this.ContinentalGrid.siteIndexToCoordinates(column, row);
        //    double max_speed = this.GetMaxWindSpeedAtPoint(pt);

        //    return max_speed;
        //}
        public double GetModifiedWindSpeed(int column, int row)  // VERSION2
        {
            Site site = PlugIn.ModelCore.Landscape.GetSite(new Location(row, column));
            Point pt = Point.siteIndexToCoordinates(column, row);
            double max_speed = this.GetMaxWindSpeedAtPoint(pt);

            if (site.IsActive)
                max_speed = max_speed * CalculateWindSpeedReduction(SiteVars.WindExposure[site][this.ClosestExposureKey]); //VERISION 2

            return max_speed;
        }
        //---------------------------------------------------------------------

        //private void KillSiteCohorts(ActiveSite site)
        //{
        //    SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
        //}
        //---------------------------------------------------------------------

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
        //bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {

            this.AvailableCohorts++;

            //bool killCohort = false;
            var windSpeed = SiteVars.WindSpeed[this.currentSite];
            var name = cohort.Species.Name;
            var age = cohort.Age;

            var deathLiklihood = HurricaneEvent.WindMortalityTable.GetMortalityProbability(cohort.Species.Name, cohort.Age, SiteVars.WindSpeed[this.currentSite]);

            //PlugIn.ModelCore.UI.WriteLine("   Hurricane Mortality:  {0}:{1}, Wind={2}, Pmort={3}", name, age, windSpeed, deathLiklihood);

            var randomVar = PlugIn.ModelCore.GenerateUniform();
            
            if (randomVar <= deathLiklihood)
            {
                //killCohort = true;
                this.CohortsKilled++;
                this.BiomassMortality += cohort.Biomass;
                SiteVars.BiomassMortality[this.currentSite] += cohort.Biomass;
                return cohort.Biomass;
            }

            //if (killCohort)
            //{
            //}

            return 0;

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

        private int Damage(ActiveSite site)
        {
            int previousCohortsKilled = this.CohortsKilled;
            SiteVars.Cohorts[site].ReduceOrKillBiomassCohorts(this);
            return this.CohortsKilled - previousCohortsKilled;
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

    //---------------------------------------------------------------------


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
                if (HurricaneEvent.HurricaneRandomNumber)
                {
                    trialValue = PlugIn.HurricaneGeneratorLogNormal.NextDouble();
                }

                trialValue = Math.Round((this.adjustFactor * trialValue)) + this.minSpeed;
                if(trialValue <= this.maxSpeed)
                    return trialValue;
            }
            return -1.0;
        }
    }
}
