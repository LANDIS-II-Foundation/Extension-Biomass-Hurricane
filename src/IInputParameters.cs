//  Authors:    Robert M. Scheller, James B. Domingo

using System.Collections.Generic;

namespace Landis.Extension.BaseHurricane
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
			get;set;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Wind event parameters for each ecoregion.
		/// </summary>
		/// <remarks>
		/// Use Ecoregion.Index property to index this array.
		/// </remarks>
		IEventParameters[] EventParameters
		{
			get;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Definitions of hurricane wind severities.
		/// </summary>
		List<ISeverity> WindSeverities
		{
			get;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Template for the filenames for output maps.
		/// </summary>
		string MapNamesTemplate
		{
			get;set;
		}

		//---------------------------------------------------------------------

		/// <summary>
		/// Name of log file.
		/// </summary>
		string LogFileName
		{
			get;set;
		}

        List<double> StormOccurenceProbabilities { get; set; }
        double LowBoundLandfallWindSpeed { get; set; }
        double AverageLandfallWindSpeed { get; set; }
        double StdDevLandfallWindSpeed { get; set; }
        double HighBoundLandfallWindspeed { get; set; }
        double CenterPointLatitude { get; set; }
        double CenterPointDistanceInland { get; set; }


    }
}
