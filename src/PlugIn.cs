//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Landis.Extension.BaseHurricane
{
    ///<summary>
    /// A disturbance plug-in that simulates hurricane wind disturbance.
    /// </summary>

    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("disturbance:hurricane");
        public static MetadataTable<EventsLog> eventLog;
        public static MetadataTable<SummaryLog> summaryLog;
        public static readonly string ExtensionName = "Base Hurricane";

        private string mapNameTemplate;
        private IInputParameters parameters;
        private WindSpeedGenerator windSpeedGenerator = null;
        private static ICore modelCore;
        private int summaryTotalSites;
        private int summaryEventCount;
        //private int[] summaryEcoregionEventCount;
        private int actualYear { get; set; } = 2019;


        //---------------------------------------------------------------------

        public PlugIn()
            : base("Base Hurricane", ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            // Console.WriteLine("vvvvvvvvvvvv   ^^^^^^^^^^^^^^^^   vvvvvvvvvvvvvvvvvvvvvvvvvvvvv Hit Enter.");
            // Console.ReadKey();
            InputParameterParser parser = new InputParameterParser();
            this.parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
            this.windSpeedGenerator = new WindSpeedGenerator(this.parameters.LowBoundLandfallWindSpeed,
                this.parameters.ModeLandfallWindSpeed, this.parameters.HighBoundLandfallWindspeed);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the plug-in with a data file.
        /// </summary>
        /// <param name="dataFile">
        /// Path to the file with initialization data.
        /// </param>
        /// <param name="startTime">
        /// Initial timestep (year): the timestep that will be passed to the
        /// first call to the component's Run method.
        /// </param>
        public override void Initialize()
        {
            Console.Write("Hello Debug Hurricane");
            List<string> colnames = new List<string>();
            foreach(IEcoregion ecoregion in modelCore.Ecoregions)
            {
                colnames.Add(ecoregion.Name);
            }
            ExtensionMetadata.ColumnNames = colnames;

            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate);

            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;

            SiteVars.Initialize();
            Event.Initialize(parameters.EventParameters, parameters.WindSeverities);

        }

        //---------------------------------------------------------------------

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

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            ModelCore.UI.WriteLine("Processing landscape for hurricane events ...");

            foreach(var year in Enumerable.Range(0, this.parameters.Timestep))
            {
                int stormsThisYear = -1;
                var randomNum = PlugIn.ModelCore.GenerateUniform();
                var cummProb = 0.0;
                foreach(var probability in this.parameters.StormOccurenceProbabilities)
                {
                    cummProb += probability;
                    stormsThisYear++;
                    if(randomNum < cummProb)
                        break;
                }
                string message = stormsThisYear + " storms.";
                if(stormsThisYear == 1) message = "1 storm.";
                LogEvent(PlugIn.ModelCore.CurrentTime, null, message);
                this.actualYear++;
                var dbg = true;
            }

            // testWindGenerationDistribution();

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;

            int eventCount = 0;
            foreach(ActiveSite site in PlugIn.ModelCore.Landscape) {
                Event hurricaneEvent = Event.Initiate(site, Timestep);
                if(hurricaneEvent != null) {
                    // LogEvent(PlugIn.ModelCore.CurrentTime, hurricaneEvent);
                    eventCount++;
                    summaryEventCount++;

                }
            }


            //ModelCore.UI.WriteLine("  Hurricane events: {0}", eventCount);

            //  Write hurricane wind severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.modelCore.CurrentTime);
            Dimensions dimensions = new Dimensions(modelCore.Landscape.Rows, modelCore.Landscape.Columns);
            using(IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach(Site site in PlugIn.ModelCore.Landscape.AllSites) {
                    if(site.IsActive) {
                        if(SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte)(SiteVars.Severity[site] + 1);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            WriteSummaryLog(PlugIn.modelCore.CurrentTime);
            summaryTotalSites = 0;
            summaryEventCount = 0;
        }

        //---------------------------------------------------------------------

        private void LogHurricaneEvent(int currentTime, string message)
        {
            eventLog.Clear();

        }

        private void LogEvent(int currentTime, Event hurricaneEvent = null, string msg = "")
        {

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.message = msg;
            //el.InitRow = hurricaneEvent.StartLocation.Row;
            //el.InitColumn = hurricaneEvent.StartLocation.Column;
            //el.TotalSites = hurricaneEvent.Size;
            //el.DamagedSites = hurricaneEvent.SitesDamaged;
            //el.CohortsKilled = hurricaneEvent.CohortsKilled;
            //el.MeanSeverity = hurricaneEvent.Severity;

            //summaryTotalSites += hurricaneEvent.SitesDamaged;
            eventLog.AddObject(el);
            eventLog.WriteToFile();


        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            summaryLog.Clear();
            SummaryLog sl = new SummaryLog();
            sl.Time = currentTime;
            sl.TotalSitesDisturbed = summaryTotalSites;
            sl.NumberEvents = summaryEventCount;

            summaryLog.AddObject(sl);
            summaryLog.WriteToFile();
        }
    }

    /// <summary>
    /// Generate random wind speeds on a log-normal distribution, mu=0, sigma=0.4.
    /// The consuming code instantiates with minimum value, which is projected to 0,
    /// the requested mode value, and the maximum value.
    /// Maximum value is enforced by getting another number if the generated one
    /// is too high.
    /// </summary>
    class WindSpeedGenerator
    {
        private double minSpeed {get; set;}
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
                trialValue = Math.Round((this.adjustFactor * trialValue)) +  this.minSpeed;
                if(trialValue <= this.maxSpeed)
                    return trialValue;
            }
            return -1.0;
        }
    }
}
