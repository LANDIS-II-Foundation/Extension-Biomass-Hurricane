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

        private ISpeciesDataset speciesDataset;
        private InputVar<string> speciesName;

        //---------------------------------------------------------------------

        public InputParameterParser()
        {
            this.speciesDataset = PlugIn.ModelCore.Species;
            this.speciesName = new InputVar<string>("Species");
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            var inputUnitsEnglish = "InputUnitsEnglish";
            var lowBoundLandfallWindSpeed = "LowBoundLandfallWindSpeed";
            var windSpeedVuln = "WindSpeedVulnerabilities";
            var map_names = "MapNames";

            ReadLandisDataVar();

            InputParameters parameters = new InputParameters(PlugIn.ModelCore.Ecoregions.Count);

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<bool> iue = new InputVar<bool>("InputUnitsEnglish");
            if (ReadOptionalVar(iue))
                parameters.InputUnitsEnglish = iue.Value;

            InputVar<int> hrs = new InputVar<int>("HurricaneRandomNumberSeed");
            if (ReadOptionalVar(hrs))
                parameters.HurricaneRandomNumberSeed = hrs.Value;

            // Read the Storm Occurrence Probabilities table
            ReadName("StormOccurrenceProbabilities");
            parameters.StormOccurenceProbabilities = new List<double>();

            InputVar<int> stormYear = new InputVar<int>("Year Count");
            InputVar<double> stormProb = new InputVar<double>("Hurricane Probability");

            Dictionary<string, int> lineNumbers = new Dictionary<string, int>();


            while (!AtEndOfInput && CurrentName != inputUnitsEnglish && CurrentName != lowBoundLandfallWindSpeed)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(stormYear, currentLine);

                ReadValue(stormProb, currentLine);
                parameters.StormOccurenceProbabilities.Add(stormProb.Value);

                CheckNoDataAfter("the " + stormProb.Name + " column", currentLine);
                GetNextLine();
            }

            InputVar<int> lowboundLFWS = new InputVar<int>("LowBoundLandfallWindSpeed");
            ReadVar(lowboundLFWS);
            parameters.LowBoundLandfallWindSpeed = lowboundLFWS.Value;

            InputVar<int> modeLFWS = new InputVar<int>("ModeLandfallWindSpeed");
            ReadVar(modeLFWS);
            parameters.ModeLandfallWindSpeed = modeLFWS.Value;

            InputVar<int> hiboundLFWS = new InputVar<int>("HighBoundLandfallWindSpeed");
            ReadVar(hiboundLFWS);
            parameters.HighBoundLandfallWindspeed = hiboundLFWS.Value;

            InputVar<double> cpl = new InputVar<double>("CenterPointLatitude");  //VERSION2
            ReadVar(cpl);
            parameters.CenterPointLatitude = cpl.Value;

            InputVar<int> cpdi = new InputVar<int>("CenterPointDistanceInland"); //VERSION2
            ReadVar(cpdi);
            parameters.CenterPointDistanceInland = cpdi.Value;

            InputVar<int> msd = new InputVar<int>("MeanStormDirection"); //VERSION2
            ReadVar(msd);
            parameters.MeanStormDirection = msd.Value;

            InputVar<int> mso = new InputVar<int>("MeanStormOffset"); //VERSION2
            ReadVar(mso);
            parameters.MeanStormOffset = mso.Value;

            InputVar<int> mws = new InputVar<int>("MinimumWindSpeedforDamage");  //VERSION2
            ReadVar(mws);
            //double minimumWindSpeed = 96.5;  // kph = 60 mph:  This is a standard minimum, below this effects generally not expected.
            HurricaneEvent.MinimumWSforDamage = mws.Value;

            ReadName("ExposureMaps");

            parameters.WindExposureMaps = new Dictionary<int, string>();
            InputVar<int> maxDegree = new InputVar<int>("Maximum Degree");
            InputVar<string> mapName = new InputVar<string>("Map Name");
            while (!AtEndOfInput && CurrentName != windSpeedVuln)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(maxDegree, currentLine);
                int maxD = maxDegree.Value;

                ReadValue(mapName, currentLine);
                string mapN = mapName.Value;

                parameters.WindExposureMaps.Add(maxD, mapN);

                GetNextLine();
            }


            ReadName(windSpeedVuln);

            parameters.WindSpeedMortalityProbabilities =
                new Dictionary<string, Dictionary<double, Dictionary<double, double>>>();
            InputVar<int> maxAge = new InputVar<int>("Maximum Age");
            InputVar<string> age_prob_string = new InputVar<string>("Age Prob Combo");


            while (!AtEndOfInput && CurrentName != map_names)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ISpecies species = ReadSpecies(currentLine);

                ReadValue(maxAge, currentLine);
                double max_age = maxAge.Value;

                //  Read ages and probabilities
                List<string> ages_probs = new List<string>();
                TextReader.SkipWhitespace(currentLine);
                while (currentLine.Peek() != -1)
                {
                    ReadValue(age_prob_string, currentLine);
                    ages_probs.Add(age_prob_string.Value.Actual);
                    TextReader.SkipWhitespace(currentLine);
                }

                Dictionary<double, double> probabilities = new Dictionary<double, double>();
                foreach (var entry in ages_probs)
                {
                    var split = entry.Split(':');
                    var speed = Convert.ToDouble(split[0]);
                    if (parameters.InputUnitsEnglish)
                        speed *= 1.60934;
                    if (speed < HurricaneEvent.MinimumWSforDamage)
                        HurricaneEvent.MinimumWSforDamage = speed;
                    var probability = Convert.ToDouble(split[1]);
                    probabilities[speed] = probability;
                    PlugIn.ModelCore.UI.WriteLine("   Hurricane Mortality Table:  {0}:{1}, Wind={2}, Pmort={3}", species.Name, max_age, speed, probability);
                }

                var cohortAges = new Dictionary<double, Dictionary<double, double>>();
                cohortAges[max_age] = probabilities;

                if (!parameters.WindSpeedMortalityProbabilities.ContainsKey(species.Name))
                    parameters.WindSpeedMortalityProbabilities.Add(species.Name, cohortAges);
                else
                    parameters.WindSpeedMortalityProbabilities[species.Name][max_age] = probabilities;

                GetNextLine();
            }


            const string MapNames = "MapNames";
            InputVar<string> mapNames = new InputVar<string>(MapNames);
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            CheckNoDataAfter(string.Format("the {0} parameter", logFile.Name));

            // testStuff(parameters);

            return parameters; 
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

        //private HurricaneWindMortalityTable
        //    populateWindSpeedVulnverabilities(
        //    HashSet<string> sectionNames, HashSet<string> singleLineNames, 
        //    Func<string, string[]> parseLine)
        //{
        //    string[] aRow;
        //    var windSpeedVulnverabilities =
        //        new Dictionary<string, Dictionary<double, Dictionary<double, double>>>();

        //    while(!(sectionNames.Contains(this.CurrentName) ||
        //        singleLineNames.Contains(this.CurrentName)))
        //    {
        //        aRow = parseLine(this.CurrentLine);
        //        string speciesName = aRow[0];
        //        if(!windSpeedVulnverabilities.ContainsKey(speciesName))
        //            windSpeedVulnverabilities[speciesName] =
        //                new Dictionary<double, Dictionary<double, double>>();

        //        double age = Convert.ToDouble(aRow[1]);
        //        var dataVals = aRow.SliceToEnd(2);
        //        Dictionary<double, double> probabilities = new Dictionary<double, double>();
        //        foreach(var entry in dataVals)
        //        {
        //            var split = entry.Split(':');
        //            var speed = Convert.ToDouble(split[0]);
        //            var probability = Convert.ToDouble(split[1]);
        //            probabilities[speed] = probability;
        //        }
        //        var cohortAges = new Dictionary<double, Dictionary<double, double>>();
        //        cohortAges[age] = probabilities;

        //        windSpeedVulnverabilities[speciesName][age] = probabilities;

        //        GetNextLine();
        //    }
        //    return new HurricaneWindMortalityTable(windSpeedVulnverabilities);
        //}
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a species name from the current line, and verifies the name.
        /// </summary>
        private ISpecies ReadSpecies(StringReader currentLine)
        {
            ReadValue(speciesName, currentLine);
            ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
            if (species == null)
                throw new InputValueException(speciesName.Value.String,
                                              "{0} is not a species name.",
                                              speciesName.Value.String);
            return species;
        }
        public static List<string> SliceToEnd(StringReader currentLine)
        {
            string[] strArray = new System.String[] { currentLine.ReadToEnd() };
            int startIdx = 0; // currentLine.Index;
            var len = strArray.Length;
            List<string> retList = new List<string>(strArray);
            retList = retList.GetRange(startIdx, len - startIdx);
            return retList;

        }
    }

}
