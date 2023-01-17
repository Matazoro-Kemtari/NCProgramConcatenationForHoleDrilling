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
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルがリーマパラメータで書き換えられること(MaterialType materialType, int spin, int feed)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create(
                ncBlocks: new List<NCBlock>
                {
                    TestNCBlockFactory.Create(
                        ncWords: new List<INCWord>
                        {
                            TestNCCommentFactory.Create(),
                        }),
                    TestNCBlockFactory.Create(
                        ncWords: new List<INCWord> {
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('M'),
                                valueData: TestNumericalValueFactory.Create("3")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('S'),
                                valueData: TestNumericalValueFactory.Create("*")),
                        }),
                    TestNCBlockFactory.Create(
                        ncWords: new List<INCWord> {
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('G'),
                                valueData: TestNumericalValueFactory.Create("98")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('G'),
                                valueData: TestNumericalValueFactory.Create("82")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('R'),
                                valueData: TestCoordinateValueFactory.Create("3")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('Z'),
                                valueData: TestCoordinateValueFactory.Create("*")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('P'),
                                valueData: TestNumericalValueFactory.Create("1000")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('F'),
                                valueData: TestNumericalValueFactory.Create("*")),
                            TestNCWordFactory.Create(
                                address: TestAddressFactory.Create('L'),
                                valueData: TestNumericalValueFactory.Create("0")),
                        })
                });
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            decimal diameter = 15m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;
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
                            new DrillingProgramPrameter(fastDrill.ToString(), -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter(secondDrill.ToString(), -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteProgramParameter(
                rewritableCodeDic,
                materialType,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を抽出する(expected, 'S');
            Assert.AreEqual(spin, rewritedSpin);
            var rewritedDepth = NCWordから値を抽出する(expected, 'Z');
            Assert.AreEqual(centerDrillDepth, rewritedDepth);
            var rewritedFeed = NCWordから値を抽出する(expected, 'F');
            Assert.AreEqual(feed, rewritedFeed);
        }

        private static decimal NCWordから値を抽出する(IEnumerable<NCProgramCode> expected, char address)
        {
            return expected.Select(x => x.NCBlocks)
                .SelectMany(x => x)
                .Select(x => x.NCWords)
                .SelectMany(x => x)
                .Where(y => y!.GetType() == typeof(NCWord))
                .Cast<NCWord>()
                .Where(z => z.Address.Value == address)
                .Select(z => z.ValueData.Number)
                .First();
        }

        [TestMethod]
        public void 異常系_リーマのパラメータを渡さないとき例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            decimal diameter = 15m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            MainProgramParametersRecord parametersRecord = new(
                new()
                {
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter(fastDrill.ToString(), -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter(secondDrill.ToString(), -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteProgramParameter(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"パラメータが受け取れません ParameterType: {nameof(ParameterType.CrystalReamerParameter)}",
                ex.Message);
        }

        [TestMethod]
        public void 異常系_ドリルのパラメータを渡さないとき例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            decimal diameter = 15m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;
            MainProgramParametersRecord parametersRecord = new(
                new()
                {
                    {
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter(diameter.ToString(), fastDrill, secondDrill, centerDrillDepth, chamferingDepth)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteProgramParameter(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"パラメータが受け取れません ParameterType: {nameof(ParameterType.DrillParameter)}",
                ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するリーマ径が無いとき例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            decimal diameter = 15m;
            decimal fastDrill = 10m;
            decimal secondDrill = 11.8m;
            decimal centerDrillDepth = -1.5m;
            decimal? chamferingDepth = -6.1m;
            MainProgramParametersRecord parametersRecord = new(
                new()
                {
                    {
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter("200", fastDrill, secondDrill, centerDrillDepth, chamferingDepth)
                        }
                    },
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter(fastDrill.ToString(), -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter(secondDrill.ToString(), -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                var expected = crystalReamingParameterRewriter.RewriteProgramParameter(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    diameter,
                    parametersRecord
                    );
                // 遅延実行
                _ = expected.ToList();
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"リーマ径 {diameter}のリストがありません",
                ex.Message);

        }
    }
}