//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.BaseHurricane
{
	/// <summary>
	/// Parameters for the plug-in.
	/// </summary>
	public class InputParameters
		: IInputParameters
	{
		private int timestep;
		private IEventParameters[] eventParameters;
		private List<ISeverity> severities;
		private string mapNamesTemplate;
		private string logFileName;

		//---------------------------------------------------------------------

		/// <summary>
		/// Timestep (years)
		/// </summary>
		public int Timestep
		{
			get {
				return timestep;
			}
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
		}

        public List<double> StormOccurenceProbabilities { get; set; }

        //               <species name      <max age,        <max wind kph, mort probability>>>
        public HurricaneWindMortalityTable HurricaneMortalityTable { get; set; }

        public double LowBoundLandfallWindSpeed { get; set; }
        public double ModeLandfallWindSpeed { get; set; }
        public double HighBoundLandfallWindspeed { get; set; }
        public double CenterPointLatitude { get; set; }
        public double CenterPointDistanceInland { get; set; }

        public void AdjustValuesFromEnglishToMetric()
        {
            this.LowBoundLandfallWindSpeed *= 1.60934;
            this.ModeLandfallWindSpeed *= 1.60934;
            this.HighBoundLandfallWindspeed *= 1.60934;
            this.CenterPointDistanceInland *= 1.60934;
            this.HurricaneMortalityTable.ChangeSpeedsFromEnglishToMetric();
        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Hurricane wind event parameters for each ecoregion.
        /// </summary>
        /// <remarks>
        /// Use Ecoregion.Index property to index this array.
        /// </remarks>
        public IEventParameters[] EventParameters
		{
			get {
				return eventParameters;
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Definitions of wind severities.
		/// </summary>
		public List<ISeverity> WindSeverities
		{
			get {
				return severities;
			}
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Template for the filenames for output maps.
		/// </summary>
		public string MapNamesTemplate
		{
			get {
				return mapNamesTemplate;
			}
            set {
                MapNames.CheckTemplateVars(value);
                mapNamesTemplate = value;
            }
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Name of log file.
		/// </summary>
		public string LogFileName
		{
			get {
				return logFileName;
			}
            set {
                if (value == null)
                    throw new InputValueException(value.ToString(), "Value must be a file path.");
                logFileName = value;
            }
		}

        //---------------------------------------------------------------------

        public InputParameters(int ecoregionCount)
        {
            eventParameters = new IEventParameters[ecoregionCount];
            severities = new List<ISeverity>();
        }
/*		//---------------------------------------------------------------------

		public Parameters(int                timestep,
		                  IEventParameters[] eventParameters,
		                  ISeverity[]        severities,
		                  string             mapNameTemplate,
		                  string             logFileName)
		{
			this.timestep = timestep;
			this.eventParameters = eventParameters;
			this.severities = severities;
			this.mapNamesTemplate = mapNameTemplate;
			this.logFileName = logFileName;
		}*/
	}
}
