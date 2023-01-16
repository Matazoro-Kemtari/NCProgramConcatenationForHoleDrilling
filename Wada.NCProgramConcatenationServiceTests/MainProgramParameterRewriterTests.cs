using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.Tests
{
    [TestClass()]
    public class MainProgramParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MainProgramType.CenterDrilling, MaterialType.Aluminum, 2000)]
        [DataRow(MainProgramType.CenterDrilling, MaterialType.Iron, 1500)]
        public void 正常系_メインプログラムの回転数パラメータが書き換えられること(MainProgramType mainProgramType, MaterialType materialType, int spin)
        {
            // given
            // when
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create(
                ncBlocks: new List<NCBlock>
                {
                    TestNCBlockFactory.Create(
                    ncWords: new List<INCWord> {
                        TestNCWordFactory.Create(
                            address: TestAddressFactory.Create('M'),
                            valueData: TestNumericalValueFactory.Create("3")),
                        TestNCWordFactory.Create(
                            address: TestAddressFactory.Create('S'),
                            valueData: TestNumericalValueFactory.Create("*")),
                    })
                });

            IMainProgramParameterRewriter mainProgramParameterRewriter = new MainProgramParameterRewriter();
            var expected = mainProgramParameterRewriter.RewriteProgramParameter(rewritableCode, mainProgramType, materialType);

           // then
           var rewritedValue = expected.NCBlocks
                .Select(x=>
                    x.NCWords.Cast<NCWord>()
                    .Where(y => y.Address.Value == 'S')
                    .Select(y => y.ValueData.Number)
                    .First())
                .First();
            Assert.AreEqual(spin, rewritedValue);
        }
    }
}