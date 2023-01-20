using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class TappingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルプログラムがタップパラメータで書き換えられること(MaterialType materialType, int expectedSpin, int expectedFeed)
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
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var expected = tappingParameterRewriter.RewriteByTool(
                rewritableCodes: rewritableCodeDic,
                material: materialType,
                thickness: 10m,
                targetToolDiameter: diameter,
                prameters: parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin);

            var rewritedDepth = NCWordから値を取得する(expected, 'Z');
            decimal expectedCenterDrillDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Cast<TappingProgramPrameter>()
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth);

            var rewritedFeed = NCWordから値を取得する(expected, 'F');
            Assert.AreEqual(expectedFeed, rewritedFeed);
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
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                decimal diameter = 15m;
                _ = tappingParameterRewriter.RewriteByTool(
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
        public void 異常系_タップのパラメータを渡さないとき例外を返すこと()
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
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter(10m.ToString(), -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter(11.8m.ToString(), -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                decimal diameter = 15m;
                _ = tappingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"パラメータが受け取れません ParameterType: {nameof(ParameterType.TapParameter)}",
                ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するタップ径が無いとき例外を返すこと()
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
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create(
                new()
                {
                    {
                        ParameterType.TapParameter,
                        new List<IMainProgramPrameter>
                        {
                            new TappingProgramPrameter("M200", 10m, -1.5m, -6.1m,200m,250m,150m,160m)
                        }
                    },
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter(10m.ToString(), -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter(11.8m.ToString(), -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                _ = tappingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    diameter,
                    parametersRecord
                    );
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"タップ径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴プログラムがタップパラメータで書き換えられること(MaterialType material, double thickness)
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
            var diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            #endregion

            var tappingParameterRewriter = new TappingParameterRewriter();
            var expected = tappingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                material,
                (decimal)thickness,
                diameter,
                parametersRecord
                );

            // then
            var rewritedSpin = NCWordから値を取得する(expected, 'S');
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NCWordから値を取得する(expected, 'Z');
            decimal expectedDepth = ドリルパラメータから値を取得する(parametersRecord, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(expected, 'Q');
            decimal expectedCutDepth = ドリルパラメータから値を取得する(parametersRecord, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            decimal rewritedFeed = NCWordから値を取得する(expected, 'F');
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

        [TestMethod]
        public void 異常系_下穴に該当するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Drilling, rewritableCode },
            };
            var parametersRecord = TestMainProgramParametersRecordFactory.Create(
                new()
                {
                    {
                        ParameterType.TapParameter,
                        new List<IMainProgramPrameter>
                        {
                            new TappingProgramPrameter("M200", 6.3m, -5m, -5m, 200m, 200m, 200m, 200m)
                        }
                    },
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter("100", -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter("100.5", -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            void target()
            {
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                _ = tappingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    200m,
                    parametersRecord
                    );
            }

            // then
            var fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Cast<TappingProgramPrameter>()
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_面取りプログラムがタップパラメータで書き換えられること(MaterialType materialType, int expectedSpin)
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
            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            #endregion

            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var expected = tappingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                materialType,
                10m,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            Assert.AreEqual(expectedSpin, rewritedSpin);

            var rewritedDepth = NCWordから値を取得する(expected, 'Z');
            decimal? expectedChamferingDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_タッププログラムがタップパラメータで書き換えられること(MaterialType materialType, double expectedThickness)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Tapping, rewritableCode },
            };
            // タップ径13.3のテストデータ
            MainProgramParametersRecord parametersRecord =
                TestMainProgramParametersRecordFactory.Create();
            #endregion

            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var expected = tappingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                materialType,
                (decimal)expectedThickness,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            decimal expectedSpin = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Cast<TappingProgramPrameter>()
                .Select(x => materialType == MaterialType.Aluminum ? x.SpinForAluminum : x.SpinForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedSpin, rewritedSpin);

            var rewritedDepth = NCWordから値を取得する(expected, 'Z');
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth);

            decimal rewritedFeed = NCWordから値を取得する(expected, 'F');
            decimal expectedFeed = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.TapParameter)!
                .Cast<TappingProgramPrameter>()
                .Select(x => materialType == MaterialType.Aluminum ? x.FeedForAluminum : x.FeedForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedFeed, rewritedFeed);
        }
    }
}