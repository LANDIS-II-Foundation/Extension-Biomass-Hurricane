//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System.IO;
using System;

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
        private static ICore modelCore;
        private int summaryTotalSites;
        private int summaryEventCount;
        //private int[] summaryEcoregionEventCount;


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
            Console.WriteLine("vvvvvvvvvvvv   ^^^^^^^^^^^^^^^^   vvvvvvvvvvvvvvvvvvvvvvvvvvvvv Hit Enter.");
            Console.ReadKey();
            InputParameterParser parser = new InputParameterParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
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
            foreach (IEcoregion ecoregion in modelCore.Ecoregions)
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

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            ModelCore.UI.WriteLine("Processing landscape for hurricane events ...");

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;

            int eventCount = 0;
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) {
                Event hurricaneEvent = Event.Initiate(site, Timestep);
                if (hurricaneEvent != null) {
                    LogEvent(PlugIn.ModelCore.CurrentTime, hurricaneEvent);
                    eventCount++;
                    summaryEventCount++;

                }
            }


            //ModelCore.UI.WriteLine("  Hurricane events: {0}", eventCount);

            //  Write hurricane wind severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, PlugIn.modelCore.CurrentTime);
            Dimensions dimensions = new Dimensions(modelCore.Landscape.Rows, modelCore.Landscape.Columns);
            using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(path, dimensions))
            {
                BytePixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte) (SiteVars.Severity[site] + 1);
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

        private void LogEvent(int   currentTime,
                              Event hurricaneEvent)
        {

            eventLog.Clear();
            EventsLog el = new EventsLog();
            el.Time = currentTime;
            el.InitRow = hurricaneEvent.StartLocation.Row;
            el.InitColumn = hurricaneEvent.StartLocation.Column;
            el.TotalSites = hurricaneEvent.Size;
            el.DamagedSites = hurricaneEvent.SitesDamaged;
            el.CohortsKilled = hurricaneEvent.CohortsKilled;
            el.MeanSeverity = hurricaneEvent.Severity;

            summaryTotalSites += hurricaneEvent.SitesDamaged;
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
}
