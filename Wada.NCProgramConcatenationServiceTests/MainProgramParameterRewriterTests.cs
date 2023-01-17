using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.Tests
{
    [TestClass()]
    public class MainProgramParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000)]
        [DataRow(MaterialType.Iron, 1500)]
        public void 正常系_リーマメインプログラムの回転数パラメータが書き換えられること(MaterialType materialType, int spin)
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
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Reaming, rewritableCode },
            };
            double diameter = 15d;
            double fastDrill = 10d;
            double secondDrill = 11.8;
            double centerDrillDepth = -1.5;
            double? chamferingDepth = -6.1;
            MainProgramParametersRecord parametersRecord = new(
                new()
                {
                    {
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter(diameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth)
                        }
                    },
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter(fastDrill.ToString())
                        }
                    }
                });

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteProgramParameter(
                rewritableCodeDic,
                materialType,
                diameter,
                parametersRecord
                );

            // then
            var rewritedValue = expected
                 .Select(x => x.NCBlocks.Where(y => y != null)
                    .Select(y => y!.NCWords.Cast<NCWord>()
                        .Where(z => z.Address.Value == 'S')
                        .Select(z => z.ValueData.Number)
                        .First())
                    .First())
                 .First();
            Assert.AreEqual(spin, rewritedValue);
        }
    }
}