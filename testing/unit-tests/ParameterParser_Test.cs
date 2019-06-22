using Edu.Wisc.Forest.Flel.Util;
using Landis.Wind;
using NUnit.Framework;
using System.Collections.Generic;

namespace Landis.Test.Wind
{
	[TestFixture]
	public class ParameterParser_Test
	{
		private ParameterParser parser;
		private LineReader reader;

		//---------------------------------------------------------------------

		[TestFixtureSetUp]
		public void Init()
		{
			parser = new ParameterParser();

			Ecoregions.DatasetParser ecoregionsParser = new Ecoregions.DatasetParser();
			reader = OpenFile("Ecoregions.txt");
			try {
				ParameterParser.EcoregionsDataset = ecoregionsParser.Parse(reader);
			}
			finally {
				reader.Close();
			}
		}

		//---------------------------------------------------------------------

		private FileLineReader OpenFile(string filename)
		{
			string path = System.IO.Path.Combine(Data.Directory, filename);
			return Landis.Data.OpenTextFile(path);
		}

		//---------------------------------------------------------------------

		private void TryParse(string filename,
		                      int    errorLineNum)
		{
			try {
				reader = OpenFile(filename);
				IParameters parameters = parser.Parse(reader);
			}
			catch (System.Exception e) {
				Data.Output.WriteLine();
				Data.Output.WriteLine(e.Message.Replace(Data.Directory, Data.DirPlaceholder));
				LineReaderException lrExc = e as LineReaderException;
				if (lrExc != null)
					Assert.AreEqual(errorLineNum, lrExc.LineNumber);
				throw;
			}
			finally {
				reader.Close();
			}
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Empty()
		{
			TryParse("empty.txt", LineReader.EndOfInput);
		}

		//---------------------------------------------------------------------

		[Test]
		public void WindParameters()
		{
			TryParse("WindParameters.txt", -100);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void LandisData_WrongValue()
		{
			TryParse("LandisData-WrongValue.txt", 3);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Timestep_Missing()
		{
			TryParse("Timestep-Missing.txt", LineReader.EndOfInput);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Timestep_Negative()
		{
			TryParse("Timestep-Negative.txt", 6);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Ecoregion_Unknown()
		{
			TryParse("Ecoregion-Unknown.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Ecoregion_Repeated()
		{
			TryParse("Ecoregion-Repeated.txt", 14);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxSize_Negative()
		{
			TryParse("MaxSize-Negative.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MeanSize_Negative()
		{
			TryParse("MeanSize-Negative.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MeanSize_MoreThanMax()
		{
			TryParse("MeanSize-MoreThanMax.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinSize_Negative()
		{
			TryParse("MinSize-Negative.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinSize_MoreThanMean()
		{
			TryParse("MinSize-MoreThanMean.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void RotationPeriod_Negative()
		{
			TryParse("RotationPeriod-Negative.txt", 12);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_NoKeyword()
		{
			TryParse("Severity-NoKeyword.txt", LineReader.EndOfInput);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_None()
		{
			TryParse("Severity-None.txt", 24);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_1stNumberNot5()
		{
			TryParse("Severity-1stNumberNot5.txt", 22);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_255()
		{
			TryParse("Severity-255.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_WrongNumber()
		{
			TryParse("Severity-WrongNumber.txt", 24);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void Severity_Missing1And2()
		{
			TryParse("Severity-Missing1And2.txt", 27);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinAge_NoPercent()
		{
			TryParse("MinAge-NoPercent.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinAge_Negative()
		{
			TryParse("MinAge-Negative.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void AgeRange_MissingTo()
		{
			TryParse("AgeRange-MissingTo.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void AgeRange_NotTo()
		{
			TryParse("AgeRange-NotTo.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinAge_TooBig()
		{
			TryParse("MinAge-TooBig.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinAge_Not0ForNum5()
		{
			TryParse("MinAge-Not0ForNum5.txt", 22);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MinAge_NotPrevMax()
		{
			TryParse("MinAge-NotPrevMax.txt", 24);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxAge_NoPercent()
		{
			TryParse("MaxAge-NoPercent.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxAge_Negative()
		{
			TryParse("MaxAge-Negative.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxAge_TooBig()
		{
			TryParse("MaxAge-TooBig.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxAge_LessThanMin()
		{
			TryParse("MaxAge-LessThanMin.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MaxAge_Not100ForNum1()
		{
			TryParse("MaxAge-Not100ForNum1.txt", 26);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MortalityProbability_Negative()
		{
			TryParse("MortalityProbability-Negative.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MortalityProbability_TooBig()
		{
			TryParse("MortalityProbability-TooBig.txt", 23);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MapNames_NoTimestep()
		{
			TryParse("MapNames-NoTimestep.txt", 28);
		}

		//---------------------------------------------------------------------

		[Test]
		[ExpectedException(typeof(LineReaderException))]
		public void MapNames_UnknownVar()
		{
			TryParse("MapNames-UnknownVar.txt", 28);
		}

	}
}
