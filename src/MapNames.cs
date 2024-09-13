//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.Hurricane
{
	/// <summary>
	/// Methods for working with the template for map filenames.
	/// </summary>
	public static class MapNames
	{
		public const string TimestepVar = "timestep";
        public const string StormNumberVar = "stormNumber";

        private static IDictionary<string, bool> knownVars;
		private static IDictionary<string, string> varValues;

		//---------------------------------------------------------------------

		static MapNames()
		{
			knownVars = new Dictionary<string, bool>();
			knownVars[TimestepVar] = true;
            knownVars[StormNumberVar] = true;

			varValues = new Dictionary<string, string>();
		}

		//---------------------------------------------------------------------

		public static void CheckTemplateVars(string template)
		{
			OutputPath.CheckTemplateVars(template, knownVars);
		}

		//---------------------------------------------------------------------

		public static string ReplaceTemplateVars(string template,
		                                         int    timestep, int stormNum)
		{
			varValues[TimestepVar] = timestep.ToString();
            varValues[StormNumberVar] = stormNum.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
		}
	}
}
