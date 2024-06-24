//  Authors:    Paul Schrum, Robert M. Scheller

using System.Collections.Generic;

namespace Landis.Extension.Hurricane
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep
        {
            get; set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        string MapNamesTemplate
        {
            get; set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        string LogFileName
        {
            get; set;
        }

        List<double> StormOccurenceProbabilities { get; set; }

        List<IWindSpeedModificationTable> WindSpeedModificationTable {get; set;}

        double LowBoundLandfallWindSpeed { get; set; }
        double ModeLandfallWindSpeed { get; set; }
        double HighBoundLandfallWindspeed { get; set; }
        double CoastalCenterX { get; set; }
        double CoastalCenterY { get; set; }
        double CoastalSlope { get; set; }
        double LandFallSigma { get; set; }
        double StormDirectionMu { get; set; }
        double StormDirectionSigma { get; set; }
        Dictionary<string, Dictionary<double, Dictionary<double, double>>> WindSpeedMortalityProbabilities { get; set; }
		Dictionary<int, string> WindExposureMaps { get; set; }

		bool InputUnitsEnglish { get; set; }

        int HurricaneRandomNumberSeed { get; set; }



    }
}
