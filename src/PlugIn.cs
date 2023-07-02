//  Authors:  Robert M. Scheller, Paul Schrum

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Metadata;
using Landis.Utilities;
using Troschuetz.Random.Distributions.Continuous;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Landis.Extension.BiomassHurricane
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
        public static readonly string ExtensionName = "Biomass Hurricane";
        public static LognormalDistribution HurricaneGeneratorLogNormal = new LognormalDistribution();
        public static Troschuetz.Random.Generators.MT19937Generator HurricaneGeneratorStandard = new Troschuetz.Random.Generators.MT19937Generator();
        public static NormalDistribution HurricaneGeneratorNormal = new NormalDistribution();

        public static List<int> WindExposures;
        public static List<IWindSpeedModificationTable> WindSpeedReductions;

        private string mapNameTemplate;
        private IInputParameters parameters;
        private static ICore modelCore;
        private int summaryEventCount = 0;

        public static double CoastalCenterX;  // VERSION2 
        public static double CoastalCenterY;  // VERSION2 
        public static double CoastalSlope;  //VERSION2
        public static Line CoastLine;
        public static Point CoastalCenter;
        public static double LandFallSigma; //VERSION2

        public static double StormDirectionMu;
        public static double StormDirectionSigma;


        //private ContinentalGrid ContinentalGrid = null;


        //---------------------------------------------------------------------

        public PlugIn()
            : base("Biomass Hurricane", ExtType)
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
            InputParameterParser parser = new InputParameterParser();
            this.parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
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

            CoastalCenterX = parameters.CoastalCenterX * 1000;  // convert km to meters, VERSION2 
            CoastalCenterY = parameters.CoastalCenterY * 1000;  // convert km to meters, VERSION2 
            CoastalSlope = parameters.CoastalSlope;  //VERSION2
            LandFallSigma = parameters.LandFallSigma; //VERSION2
            StormDirectionMu = parameters.StormDirectionMu;
            StormDirectionSigma = parameters.StormDirectionSigma;
            HurricaneEvent.WindMortalityTable = new HurricaneWindMortalityTable(parameters.WindSpeedMortalityProbabilities);
            if (parameters.InputUnitsEnglish)
            {
                parameters.LowBoundLandfallWindSpeed *= 1.60934;
                parameters.ModeLandfallWindSpeed *= 1.60934;
                parameters.HighBoundLandfallWindspeed *= 1.60934;

            }

            CoastalCenter = new Point(CoastalCenterX, CoastalCenterY);  // VERSION2
            CoastLine = new Line(CoastalCenter, CoastalSlope);  //VERSION2

            HurricaneEvent.WindSpeedGenerator = new WindSpeedGenerator(this.parameters.LowBoundLandfallWindSpeed,
                this.parameters.ModeLandfallWindSpeed, this.parameters.HighBoundLandfallWindspeed);
            //parameters.AdjustValuesFromEnglishToMetric();

            MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.MapNamesTemplate);

            Timestep = parameters.Timestep;
            mapNameTemplate = parameters.MapNamesTemplate;

            SiteVars.Initialize();

            //this.ContinentalGrid = new ContinentalGrid(
            //    //this.parameters.CenterPointLatitude, 
            //    PlugIn.ModelCore.CellLength,
            //    PlugIn.ModelCore.Landscape.Columns,
            //    PlugIn.ModelCore.Landscape.Rows
            //    //this.parameters.CenterPointDistanceInland
            //    );

            if (parameters.HurricaneRandomNumberSeed > 0)
            {
                HurricaneEvent.HurricaneRandomNumber = true;
                HurricaneGeneratorStandard = new Troschuetz.Random.Generators.MT19937Generator((uint)parameters.HurricaneRandomNumberSeed);
                HurricaneGeneratorLogNormal = new Troschuetz.Random.Distributions.Continuous.LognormalDistribution((uint)parameters.HurricaneRandomNumberSeed);
            }

            LoadWindExposureData();  // VERSION2

            WindSpeedReductions = parameters.WindSpeedModificationTable;

        }

        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            ModelCore.UI.WriteLine("Processing landscape for hurricane events ...");

            summaryEventCount = 0;

            foreach(var year in Enumerable.Range(0, this.parameters.Timestep))
            {
                int stormsThisYear = -1;
                var randomNum = PlugIn.ModelCore.GenerateUniform();
                if (HurricaneEvent.HurricaneRandomNumber)
                    randomNum = PlugIn.HurricaneGeneratorStandard.NextDouble();

                var cummProb = 0.0;
                foreach(var probability in this.parameters.StormOccurenceProbabilities)
                {
                    cummProb += probability;
                    stormsThisYear++;

                    // Here the number of hurricanes per time step is determined:
                    if(randomNum < cummProb)
                        break;
                }
                string message = stormsThisYear + " storms.";
                if(stormsThisYear == 1) message = "1 storm.";
                foreach(var stormCount in Enumerable.Range(0, stormsThisYear))
                {
                    HurricaneEvent storm = new HurricaneEvent(stormCount+1); 

                    bool impactsStudyArea =
                        storm.HurricaneDisturb(); 
                    
                    LogEvent(storm);

                    if (impactsStudyArea)
                    {

                        string path = MapNames.ReplaceTemplateVars(@"Hurricane\biomass-mortality-{timestep}-{stormNumber}.img", modelCore.CurrentTime, stormCount);
                        IOutputRaster<IntPixel> outputRaster = null;
                        using (outputRaster = modelCore.CreateRaster<IntPixel>(path, ModelCore.Landscape.Dimensions))
                        {
                            IntPixel pixel = outputRaster.BufferPixel;
                            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                            {
                                if (site.IsActive)
                                {
                                    pixel.MapCode.Value = SiteVars.BiomassMortality[site];
                                }
                                else
                                {
                                    pixel.MapCode.Value = 0;
                                }
                                outputRaster.WriteBufferPixel();
                            }
                        }

                    }

                }
            }

            WriteSummaryLog(PlugIn.modelCore.CurrentTime);
        }

        //---------------------------------------------------------------------

        enum unitType { Dist, Speed };
        private void LogEvent(HurricaneEvent hurricaneEvent = null)
        {

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = ModelCore.CurrentTime;
            el.HurricaneNumber = hurricaneEvent.HurricaneNumber;
            el.ImpactsStudyArea = hurricaneEvent.StudyAreaMortality;
            el.StudyAreaMaxWS = hurricaneEvent.StudyAreaMaxWindspeed;
            el.StudyAreaMinWS = hurricaneEvent.StudyAreaMinWindspeed;
            el.LandfallX = hurricaneEvent.LandfallPoint.X;
            el.LandfallY = hurricaneEvent.LandfallPoint.Y;
            el.LandfallMaxWindSpeed = hurricaneEvent.LandfallMaxWindSpeed;
            el.PathHeading = hurricaneEvent.StormTrackHeading;
            el.CohortKilled = hurricaneEvent.CohortsKilled;
            el.BiomassMortality = hurricaneEvent.BiomassMortality;
            eventLog.AddObject(el);
            eventLog.WriteToFile();

            if(hurricaneEvent.StudyAreaMortality)
                summaryEventCount++;
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int currentTime)
        {
            summaryLog.Clear();
            SummaryLog sl = new SummaryLog();
            sl.Year = currentTime;
            sl.NumberEvents = summaryEventCount;

            summaryLog.AddObject(sl);
            summaryLog.WriteToFile();
        }

        private void LoadWindExposureData()
        {
            WindExposures = new List<int>(parameters.WindExposureMaps.Keys);

            // Add degrees and map values to SiteVars.WindExpsosure
            // Convert Wind Exposure Maps into Site Dictionaries //VERSION2
            foreach (KeyValuePair<int, string> windmap in parameters.WindExposureMaps)
            {
                IInputRaster<IntPixel> map = MakeIntMap(windmap.Value);

                using (map)
                {
                    IntPixel pixel = map.BufferPixel;
                    foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                    {
                        map.ReadBufferPixel();
                        int mapValue = pixel.MapCode.Value;
                        if (site.IsActive)
                        {
                            if (mapValue < 0 || mapValue > 9)
                                throw new InputValueException(mapValue.ToString(),
                                                              "Wind Exposure Values: {0} is incorrect. They must range from 1-9. Site_Row={1:0}, Site_Column={2:0}",
                                                              mapValue, site.Location.Row, site.Location.Column);

                            // add data to SiteVars.WindExposure[site] dictionary
                            SiteVars.WindExposure[site].Add(windmap.Key, mapValue);
                        }
                    }

                }
            }

            return;
        }

        //---------------------------------------------------------------------
        private static IInputRaster<IntPixel> MakeIntMap(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("    Read in data from {0}", path);

            IInputRaster<IntPixel> map;

            try
            {
                map = PlugIn.ModelCore.OpenRaster<IntPixel>(path);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the scenario ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            return map;
        }
    }

}
