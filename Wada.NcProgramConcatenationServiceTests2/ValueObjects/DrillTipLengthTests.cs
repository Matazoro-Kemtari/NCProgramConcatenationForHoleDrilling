using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wada.NcProgramConcatenationService.ValueObjects.Tests
{
    [TestClass()]
    public class DrillTipLengthTests
    {
        [DataTestMethod()]
        [DataRow(2, 2)]
        [DataRow(2.5, 2)]
        [DataRow(3, 2.5)]
        [DataRow(3.5, 2.5)]
        [DataRow(4, 2.5)]
        [DataRow(4.5, 3)]
        [DataRow(5, 3)]
        public void 正常系_先端長さが計算できること(double diameter, double expected)
        {
            // given
            // when
            DrillTipLength drillTipLength = new((decimal)diameter);

            // then
            Assert.AreEqual((decimal)expected, drillTipLength.Value);
        }
    }
}