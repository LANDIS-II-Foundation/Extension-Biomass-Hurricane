//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;
using System.Text;
using System;

namespace Landis.Extension.BaseHurricane
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        public InputParameterParser()
        {
            // ToDo: fix this Hack to ensure that Percentage is registered with InputValues
            Landis.Utilities.Percentage p = new Landis.Utilities.Percentage();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            string[] parseLine(string line)
            {
                return line.Replace("\t", " ")
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            var stormOccurProb = "StormOccurrenceProbabilities";
            var inputUnitsEnglish = "InputUnitsEnglish";
            var lowBoundLandfallWindSpeed = "LowBoundLandfallWindSpeed";
            var modeLandfallWindSpeed = "ModeLandfallWindSpeed";
            var highBoundLandfallWindSpeed = "HighBoundLandfallWindSpeed";
            var centerPointLatitude = "CenterPointLatitude";
            var centerPointDistanceInland = "CenterPointDistanceInland";
            var windSpeedVuln = "WindSpeedVulnerabilities";
            var map_names = "MapNames";

            var sectionNames = new HashSet<System.String> {stormOccurProb, 
                windSpeedVuln, map_names };

            var singleLineNames = new HashSet<System.String> {lowBoundLandfallWindSpeed,
                modeLandfallWindSpeed, highBoundLandfallWindSpeed,
                centerPointLatitude, centerPointDistanceInland, inputUnitsEnglish,};
            var inputUnitsAreEnglish = false;

            ReadLandisDataVar();

            InputParameters parameters = new InputParameters(PlugIn.ModelCore.Ecoregions.Count);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            // Read the Storm Occurrence Probabilities table
            // The call to ReadVar advanced the cursor, so it contains the next line.
            string lastOperation = this.CurrentName;
            while(sectionNames.Contains(lastOperation) || singleLineNames.Contains(lastOperation))
            {
                string[] row;
                if(sectionNames.Contains(lastOperation))
                {
                    sectionNames.Remove(lastOperation);
                    GetNextLine();
                    if(lastOperation == stormOccurProb)
                    {
                        populateStormOccurenceProbabilities(
                            parameters, sectionNames, singleLineNames, parseLine);
                    }
                    else if (lastOperation == windSpeedVuln)
                    {
                        HurricaneEvent.windMortalityTable =
                            populateWindSpeedVulnverabilities(
                                sectionNames, singleLineNames, parseLine);
                    }
                    lastOperation = this.CurrentName;
                }

                if(singleLineNames.Contains(lastOperation))
                {
                    string value = "";
                    row = parseLine(this.CurrentLine);
                    if(row.Length >= 2)
                        value = row[1];
                    if(lastOperation == lowBoundLandfallWindSpeed)
                        parameters.LowBoundLandfallWindSpeed = Convert.ToDouble(value);
                    else if(lastOperation == modeLandfallWindSpeed)
                        parameters.ModeLandfallWindSpeed = Convert.ToDouble(value);
                    else if(lastOperation == highBoundLandfallWindSpeed)
                        parameters.HighBoundLandfallWindspeed = Convert.ToDouble(value);
                    else if(lastOperation == centerPointLatitude)
                        parameters.CenterPointLatitude = Convert.ToDouble(value);
                    else if(lastOperation == centerPointDistanceInland)
                        parameters.CenterPointDistanceInland = Convert.ToDouble(value);
                    else if(lastOperation == inputUnitsEnglish)
                        inputUnitsAreEnglish = true;
                    singleLineNames.Remove(lastOperation);
                    GetNextLine();
                    lastOperation = this.CurrentName;
                }
                if(lastOperation == map_names)
                    break;
            }
            if(inputUnitsAreEnglish)
                parameters.AdjustValuesFromEnglishToMetric();
            HurricaneEvent.minimumWSforDamage =
                HurricaneEvent.windMortalityTable.MinimumWindSpeed;

            const string MapNames = "MapNames";
            InputVar<string> mapNames = new InputVar<string>(MapNames);
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            CheckNoDataAfter(string.Format("the {0} parameter", logFile.Name));

            // testStuff(parameters);

            return parameters; //.GetComplete();
        }

        //private void testStuff(InputParameters parameters)
        //{
        //    var speciesName = "LobPine";
        //    var age = 8.0;
        //    var windSpeed = 115.2 * 1.60934;
        //    var expectedProbability = 0.60;
        //    double actualProbability = parameters.HurricaneMortalityTable
        //        .GetMortalityProbability(speciesName, age, windSpeed);
        //    var diff = expectedProbability - actualProbability;

        //    age = 15.0;
        //    actualProbability = parameters.HurricaneMortalityTable
        //        .GetMortalityProbability(speciesName, age, windSpeed);
        //    expectedProbability = 0.65;
        //    diff = expectedProbability - actualProbability;
        //}

        private HurricaneWindMortalityTable
            populateWindSpeedVulnverabilities(
            HashSet<string> sectionNames, HashSet<string> singleLineNames, 
            Func<string, string[]> parseLine)
        {
            string[] aRow;
            var windSpeedVulnverabilities =
                new Dictionary<string, Dictionary<double, Dictionary<double, double>>>();

            while(!(sectionNames.Contains(this.CurrentName) ||
                singleLineNames.Contains(this.CurrentName)))
            {
                aRow = parseLine(this.CurrentLine);
                string speciesName = aRow[0];
                if(!windSpeedVulnverabilities.ContainsKey(speciesName))
                    windSpeedVulnverabilities[speciesName] =
                        new Dictionary<double, Dictionary<double, double>>();

                double age = Convert.ToDouble(aRow[1]);
                var dataVals = aRow.SliceToEnd(2);
                Dictionary<double, double> probabilities = new Dictionary<double, double>();
                foreach(var entry in dataVals)
                {
                    var split = entry.Split(':');
                    var speed = Convert.ToDouble(split[0]);
                    var probability = Convert.ToDouble(split[1]);
                    probabilities[speed] = probability;
                }
                var cohortAges = new Dictionary<double, Dictionary<double, double>>();
                cohortAges[age] = probabilities;

                windSpeedVulnverabilities[speciesName][age] = probabilities;

                //parameters.windSpeedVulnverabilities;
                GetNextLine();
            }
            return new HurricaneWindMortalityTable(windSpeedVulnverabilities);
        }

        private void populateStormOccurenceProbabilities(
            InputParameters parameters, HashSet<string> sectionNames, 
            HashSet<string> singleLineNames, Func<string, string[]> parseLine)
        {
            string[] aRow;
            parameters.StormOccurenceProbabilities = new List<double>();
            while(!(sectionNames.Contains(this.CurrentName) ||
                singleLineNames.Contains(this.CurrentName)))
            {
                aRow = parseLine(this.CurrentLine);
                parameters.StormOccurenceProbabilities.Add(Convert.ToDouble(aRow[1]));
                GetNextLine();
            }
        }
    }

    public static class ExtensionMethods
    {
        public static List<string> SliceToEnd(this string[] strArray, int startIdx)
        {
            var len = strArray.Length;
            var retList = new List<string>(strArray);
            retList = retList.GetRange(startIdx, len - startIdx);
            return retList;

        }
    }
}
