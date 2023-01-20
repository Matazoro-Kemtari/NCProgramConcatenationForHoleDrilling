using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class DrillingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルプログラムがドリルパラメータで書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            MainProgramParametersRecord parametersRecord =
                TestMainProgramParametersRecordFactory.Create();
            #endregion

            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.DrillParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(
                rewritableCodes: rewritableCodeDic,
                material: material,
                thickness: 10m,
                targetToolDiameter: diameter,
                prameterRecord: parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal expectedCenterDrillDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.DrillParameter)!
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F');
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NCProgramCode> expected, char address, int skip = 0)
        {
            return expected.Skip(skip).Select(x => x.NCBlocks)
                .SelectMany(x => x)
                .Where(x => x != null)
                .Select(x => x?.NCWords)
                .Where(x => x != null)
                .SelectMany(x => x!)
                .Where(y => y!.GetType() == typeof(NCWord))
                .Cast<NCWord>()
                .Where(z => z.Address.Value == address)
                .Select(z => z.ValueData.Number)
                .FirstOrDefault();
        }

        [TestMethod]
        public void 異常系_素材が未定義の場合例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create();
            #endregion
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                decimal diameter = 15m;
                _ = drillingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Undefined,
                    10m,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
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
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create(
                new()
                {
                    {
                        ParameterType.TapParameter,
                        new List<IMainProgramPrameter>
                        {
                            new TappingProgramPrameter(DiameterKey: "M12",
                                                       PreparedHoleDiameter: 10m,
                                                       CenterDrillDepth: 10.3m,
                                                       ChamferingDepth: -6.3m,
                                                       SpinForAluminum: 160m,
                                                       FeedForAluminum: 280m,
                                                       SpinForIron: 120m,
                                                       FeedForIron: 210m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                decimal diameter = 15m;
                _ = drillingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
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
        public void 異常系_リストに一致するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.CenterDrilling, rewritableCode },
            };
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create();
            #endregion

            decimal diameter = 9m;
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                _ = drillingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"ドリル径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴プログラムがドリルパラメータで書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Drilling, rewritableCode },
            };
            var parametersRecord = TestMainProgramParametersRecordFactory.Create();
            #endregion

            var diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.DrillParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            var drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                material,
                (decimal)thickness,
                diameter,
                parametersRecord
                );

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S');
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal expectedDepth = ドリルパラメータから値を取得する(parametersRecord, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q');
            decimal expectedCutDepth = ドリルパラメータから値を取得する(parametersRecord, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F');
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(MainProgramParametersRecord parametersRecord, Func<DrillingProgramPrameter, decimal> select, int skip = 0)
        {
            return parametersRecord.Parameters.GetValueOrDefault(ParameterType.DrillParameter)!
                .Where(x => x.GetType() == typeof(DrillingProgramPrameter))
                .Where(x => x != null)
                .Cast<DrillingProgramPrameter>()
                .Skip(skip)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_面取りプログラムがドリルパラメータで書き換えられること(MaterialType materialType, int expectedSpin)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Chamfering, rewritableCode },
            };
            MainProgramParametersRecord parametersRecord =
                TestMainProgramParametersRecordFactory.Create();
            #endregion

            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.DrillParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                materialType,
                10m,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z');
            decimal? expectedChamferingDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.DrillParameter)!
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "面取り深さ");
        }
    }
}