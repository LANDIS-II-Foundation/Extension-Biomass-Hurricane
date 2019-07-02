using Landis.Extension.BaseHurricane;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
    }
}
