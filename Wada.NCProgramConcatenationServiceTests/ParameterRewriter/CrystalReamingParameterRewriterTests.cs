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
        public void 正常系_センタードリルプログラムがリーマパラメータで書き換えられること(MaterialType materialType, int spin, int feed)
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
            decimal diameter = 15m;
            MainProgramParametersRecord parametersRecord = TestMainProgramParametersRecordFactory.Create();
            #endregion
            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(
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
            Assert.AreEqual($"リーマ径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_下穴プログラムがリーマパラメータで書き換えられること(MaterialType material, double thickness)
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
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            var fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            var secondDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            var centerDrillDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            var chamferingDepth = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            #endregion

            var crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                material,
                (decimal)thickness,
                diameter,
                parametersRecord
                );

            // then
            var rewritedSpin = NCWordから値を取得する(expected, 'S');
            var spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron);
            Assert.AreEqual(spin, rewritedSpin, "下穴1の回転数");

            rewritedSpin = NCWordから値を取得する(expected, 'S', 1);
            spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForAluminum, 1)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.SpinForIron, 1);
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
            decimal feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForIron);
            Assert.AreEqual(feed, rewritedFeed, "下穴1の送り");

            rewritedFeed = NCWordから値を取得する(expected, 'F', 1);
            feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForAluminum, 1)
                : ドリルパラメータから値を取得する(parametersRecord, x => x.FeedForIron, 1);
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

        [TestMethod]
        public void 異常系_下穴1回目に該当するドリル径が無いとき例外を返すこと()
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
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter("200", 1m, 200m, 0.1m, 0.1m)
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
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    200m,
                    parametersRecord
                    );
            }

            // then
            var fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [TestMethod]
        public void 異常系_下穴2回目に該当するドリル径が無いとき例外を返すこと()
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
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter("200", 100m, 2m, 0.1m, 0.1m)
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
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(
                    rewritableCodeDic,
                    MaterialType.Aluminum,
                    10m,
                    200m,
                    parametersRecord
                    );
            }

            // then
            var fastDrill = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Cast<ReamingProgramPrameter>()
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_面取りプログラムがリーマパラメータで書き換えられること(MaterialType materialType, int spin)
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
            Assert.AreEqual(chamferingDepth, rewritedDepth);
        }

        [TestMethod]
        public void 正常系_面取りプログラムが無いパラメータで書き換えをしたとき何もしないこと()
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Chamfering, rewritableCode },
            };
            var parametersRecord = TestMainProgramParametersRecordFactory.Create(
                new()
                {
                    {
                        ParameterType.CrystalReamerParameter,
                        new List<IMainProgramPrameter>
                        {
                            new ReamingProgramPrameter("200", 10m, 20m, 0.1m, null)
                        }
                    },
                    {
                        ParameterType.DrillParameter,
                        new List<IMainProgramPrameter>
                        {
                            new DrillingProgramPrameter("10", -1.5m, 3m, 960m, 130m, 640m, 90m),
                            new DrillingProgramPrameter("10.5", -1.5m, 3.5m, 84m, 110m, 560m, 80m)
                        }
                    }
                });
            #endregion
            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                MaterialType.Aluminum,
                10m,
                200m,
                parametersRecord
                );

            // then
            Assert.AreEqual(0, expected.Count());
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 10.5, 380, 80)]
        [DataRow(MaterialType.Iron, 12.4, 290, 40)]
        public void 正常系_リーマプログラムがリーマパラメータで書き換えられること(MaterialType materialType, double thickness, int spin, int feed)
        {
            // given
            // when
            #region テストデータ
            NCProgramCode rewritableCode = TestNCProgramCodeFactory.Create();
            Dictionary<MainProgramType, NCProgramCode> rewritableCodeDic = new()
            {
                { MainProgramType.Reaming, rewritableCode },
            };
            // リーマ径13.3のテストデータ
            MainProgramParametersRecord parametersRecord =
                TestMainProgramParametersRecordFactory.Create();
            decimal diameter = parametersRecord.Parameters
                .GetValueOrDefault(ParameterType.CrystalReamerParameter)!
                .Select(x => x.TargetToolDiameter)
                .FirstOrDefault();
            #endregion

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var expected = crystalReamingParameterRewriter.RewriteByTool(
                rewritableCodeDic,
                materialType,
                (decimal)thickness,
                diameter,
                parametersRecord
                );

            // then
            decimal rewritedSpin = NCWordから値を取得する(expected, 'S');
            Assert.AreEqual((decimal)spin, rewritedSpin);
            var rewritedDepth = NCWordから値を取得する(expected, 'Z');
            Assert.AreEqual((decimal)-thickness - 5m, rewritedDepth);
            decimal rewritedFeed = NCWordから値を取得する(expected, 'F');
            Assert.AreEqual((decimal)feed, rewritedFeed);
        }
    }
}