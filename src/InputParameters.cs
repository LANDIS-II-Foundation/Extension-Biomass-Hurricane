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
        //private IEventParameters[] eventParameters;
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

        public double LowBoundLandfallWindSpeed { get; set; }
        public double ModeLandfallWindSpeed { get; set; }
        public double HighBoundLandfallWindspeed { get; set; }
        public double LandfallLatitudeMean { get; set; }
        public double LandfallLatitudeStdDev { get; set; }
        public double StormDirectionMean { get; set; }
        public double StormDirectionStdDev { get; set; }
        //public double CenterPointLatitude { get; set; }
        //public double CenterPointDistanceInland { get; set; }
        //public  double minimumWSforDamage { get; set; } = 96.5;

        public Dictionary<string, Dictionary<double, Dictionary<double, double>>> WindSpeedMortalityProbabilities { get; set; }
		public Dictionary<int, string> WindExposureMaps { get; set; }

		public bool InputUnitsEnglish { get; set; } = false;

        public int HurricaneRandomNumberSeed { get; set; } = -999;



        //---------------------------------------------------------------------

        /// <summary>
        /// Hurricane wind event parameters for each ecoregion.
        /// </summary>
        /// <remarks>
        /// Use Ecoregion.Index property to index this array.
        /// </remarks>
  //      public IEventParameters[] EventParameters
		//{
		//	get {
		//		return eventParameters;
		//	}
		//}

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

        public InputParameters() //int ecoregionCount)
        {
            //eventParameters = new IEventParameters; //[ecoregionCount];

        }
	}
}
