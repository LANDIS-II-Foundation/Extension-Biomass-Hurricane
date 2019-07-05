using Landis.Extension.BaseHurricane;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HurricaneUnitTests
{
    [TestClass]
    public class UnitTest1
    {
        WindSpeedGenerator windGen { get; set; }
        ContinentalGrid grid { get; set; }
        HurricaneEvent storm1 { get; set; }

        [TestInitialize]
        public void intialize()
        {
            windGen = new WindSpeedGenerator(67.59, 119.09, 241.40);
            grid = new ContinentalGrid(35.11, 100, 436, 265, 160.934);
            storm1 = new HurricaneEvent(1, windGen, 160.934, grid);
        }

        [TestMethod]
        public void Test_AllVariables_NotNull()
        {
            Assert.IsNotNull(windGen);
            Assert.IsNotNull(grid);
            Assert.IsNotNull(storm1);
        }

        [TestMethod]
        public void Test_Coordiantes_CenterPoint()
        {
            bool result = grid.CenterPoint == new Point(21800, 13250);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Coordinates_CoastNearPoint()
        {
            bool result = grid.CoastNearPoint == new Point(135597.5227, -100547.5227);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Coordinates_Coastline_b()
        {
            bool result = Math.Abs(grid.CoastLine.b - -236145.04545) < 0.001;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Coordinates_Storm1Landfall()
        {
            bool result = storm1.LandfallPoint == new Point(159485.0454, -76660.0);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_Line_StormTrack()
        {
            Assert.IsNotNull(storm1.StormTrackLine);

            double expectedYintercept = 57163.8428;
            bool result = new Point(0.0, storm1.StormTrackLine.b) == new Point(0.0, expectedYintercept);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Given a point, is the distance and offset from the storm's landfall point
        /// to the given point correct?
        /// </summary>
        [TestMethod]
        public void Test_Line_GetDistanceAndOffset_IsCorrect()
        {
            Point testPt = new Point(43600.0, 26500.0);
            (double distance, double offset) = storm1.GetDistanceAndOffset(testPt);

            bool result = Math.Abs(offset - 4535.6734) < 0.001;
            Assert.IsTrue(result);

            result = Math.Abs(distance - 155083.064) < 0.001;
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void Test_Windfield_Equation()
        {
            var test119 = storm1.ComputeMaxWindSpeed(x: 0.0, offset: 0.0, a: 228.0);
            var test108 = storm1.ComputeMaxWindSpeed(x:76.0, offset: 0.0, a: 228.0);
            var test60 = storm1.ComputeMaxWindSpeed(x: 339.6, offset: 0.0, a: 228.0);

            bool result = Math.Abs(test119 - 119.1) < 0.15;
            Assert.IsTrue(result);

            result = Math.Abs(test108 - 108.7) < 0.15;
            Assert.IsTrue(result);

            result = Math.Abs(test60 - 60.3) < 0.15;
            Assert.IsTrue(result);

            var testRight = storm1.ComputeMaxWindSpeed(x: 76.0, offset: 60.0, a: 228.0);
            var testLeft = storm1.ComputeMaxWindSpeed(x: 76.0, offset: -60.0, a: 228.0);

            result = Math.Abs(testRight - 96.9) < 0.15;
            Assert.IsTrue(result);

            result = Math.Abs(testLeft - 87.0) < 0.15;
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_WindSpeed_Get()
        {
            Point testPt = new Point(43600.0, 26500.0);
            double maxSpeed = storm1.GetMaxWindSpeedAtPoint(testPt);

            bool result = Math.Abs(maxSpeed - 55.795) < 0.001;
            //Assert.IsTrue(result);
        }
    }
}
