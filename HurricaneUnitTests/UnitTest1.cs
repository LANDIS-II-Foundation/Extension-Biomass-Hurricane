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

    }
}
