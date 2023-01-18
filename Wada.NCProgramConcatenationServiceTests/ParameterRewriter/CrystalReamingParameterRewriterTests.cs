using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class CrystalReamingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_センタードリルがリーマパラメータで書き換えられること(MaterialType materialType, int spin, int feed)
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
            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            decimal fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            decimal secondDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            decimal centerDrillDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            decimal? chamferingDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            #endregion

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                materialType,
                10m,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            Assert.AreEqual(spin, rewritedSpin);
            var rewritedDepth = NCWordから値を取得する(expected, 'Z');
            Assert.AreEqual(centerDrillDepth, rewritedDepth);
            var rewritedFeed = NCWordから値を取得する(expected, 'F');
            Assert.AreEqual(feed, rewritedFeed);
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
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create(
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
                _ = crystalReamingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
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
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create(
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
                _ = crystalReamingParameterRewriter.RewriteByTool(
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
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create(
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
                var expected = crystalReamingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
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

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴がリーマパラメータで書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Drilling, rewritableCode },
            };
            MainProgramParametersRecord parametersRecord =
                TestMainProgramParametersRecordFactory.Create();
            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            decimal fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            decimal secondDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            decimal centerDrillDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            decimal? chamferingDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            #endregion

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                material,
                (decimal)thickness,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            decimal spin;
            if (material == MaterialType.Aluminum)
                spin = ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum);
            else
                spin = ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron);
            Assert.AreEqual(spin, rewritedSpin, "下穴1の回転数");
            rewritedSpin = NCWordから値を取得する(expected, 'S', 1);
            if (material == MaterialType.Aluminum)
                spin = ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum, 1);
            else
                spin = ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron, 1);
            Assert.AreEqual(spin, rewritedSpin, "下穴2の回転数");

            decimal rewritedDepth = NCWordから値を取得する(expected, 'Z');
            decimal depth = ドリルパラメータから値を取得する(parametersRecord, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(depth, rewritedDepth, "下穴1のZ");
            rewritedDepth = NCWordから値を取得する(expected, 'Z', 1);
            depth = ドリルパラメータから値を取得する(parametersRecord, x => -x.DrillTipLength - (decimal)thickness, 1);
            Assert.AreEqual(depth, rewritedDepth, "下穴2のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(expected, 'Q');
            decimal cutDepth = ドリルパラメータから値を取得する(parametersRecord, x => x.CutDepth);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴1の切込");
            cutDepth = cutDepth = ドリルパラメータから値を取得する(parametersRecord, x => x.CutDepth, 1);
            rewritedCutDepth = NCWordから値を取得する(expected, 'Q', 1);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴2の切込");

            decimal rewritedFeed = NCWordから値を取得する(expected, 'F');
            decimal feed;
            if (material == MaterialType.Aluminum)
                feed = ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForAluminum);
            else
                feed = ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForIron);
            Assert.AreEqual(feed, rewritedFeed, "下穴1の送り");
            rewritedFeed = NCWordから値を取得する(expected, 'F', 1);
            if (material == MaterialType.Aluminum)
                feed = ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForAluminum, 1);
            else
                feed = ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForIron, 1);
            Assert.AreEqual(feed, rewritedFeed, "下穴2の送り");
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
    }
}