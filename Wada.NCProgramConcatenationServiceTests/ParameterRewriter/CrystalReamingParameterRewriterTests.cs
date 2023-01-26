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
        public void 正常系_工程センタードリルが書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            decimal expectedCenterDrillDepth = param.CrystalReamerParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.CenterDrilling);
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NCProgramCode> ncProgramCode, char address, NCProgramType ncProgram, int skip = 0)
        {
            return ncProgramCode
                .Where(x => x.MainProgramClassification == ncProgram)
                .Skip(skip)
                .Select(x => x.NCBlocks)
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
            var param = TestRewriteByToolRecordFactory.Create(material: MaterialType.Undefined);
            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するリーマ径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolRecordFactory.Create(directedOperationToolDiameter: diameter);

            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"リーマ径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_工程下穴が書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(
                material: material,
                thickness: (decimal)thickness);
            var crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴1の回転数");

            rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Drilling, 1);
            expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum, 1)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron, 1);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴2の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Drilling, 1);
            expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness, 1);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴2のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NCProgramType.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            expectedCutDepth = expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth, 1);
            rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NCProgramType.Drilling, 1);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴2の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");

            rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.Drilling, 1);
            expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum, 1)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron, 1);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴2の送り");
        }

        private static decimal ドリルパラメータから値を取得する(RewriteByToolRecord param, Func<DrillingProgramPrameter, decimal> select, int skip = 0)
        {
            decimal drillDiameter = skip switch
            {
                1 => param.CrystalReamerParameters
                        .Where(x => x.DirectedOperationToolDiameter <= param.DirectedOperationToolDiameter)
                        .Select(x => x.SecondPreparedHoleDiameter)
                        .Max(),
                _ => param.CrystalReamerParameters
                        .Where(x => x.DirectedOperationToolDiameter <= param.DirectedOperationToolDiameter)
                        .Select(x => x.PreparedHoleDiameter)
                        .Max(),
            };
            return param.DrillingPrameters
                .Where(x => x.DirectedOperationToolDiameter == drillDiameter)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [TestMethod]
        public void 異常系_下穴1回目に該当するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal reamerDiameter = 5.5m;
            var param = TestRewriteByToolRecordFactory.Create(
                directedOperationToolDiameter: reamerDiameter,
                crystalReamerParameters: new List<ReamingProgramPrameter>
                {
                    TestReamingProgramPrameterFactory.Create(DiameterKey: reamerDiameter.ToString(), PreparedHoleDiameter: 3),
                },
                drillingPrameters: new List<DrillingProgramPrameter>
                {
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.CrystalReamerParameters
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
            decimal reamerDiameter = 5.5m;
            var param = TestRewriteByToolRecordFactory.Create(
                directedOperationToolDiameter: reamerDiameter,
                crystalReamerParameters: new List<ReamingProgramPrameter>
                {
                    TestReamingProgramPrameterFactory.Create(
                        DiameterKey: reamerDiameter.ToString(),
                        PreparedHoleDiameter: 20m,
                        SecondPreparedHoleDiameter: 3m),
                },
                drillingPrameters: new List<DrillingProgramPrameter>
                {
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.CrystalReamerParameters
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_工程面取りが書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);

            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");
            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Chamfering);
            decimal? expectedChamferingDepth = param.CrystalReamerParameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "Z値");
        }

        [TestMethod]
        public void 正常系_パラメータで面取りが無のとき面取りのNCプログラムがないこと()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(crystalReamerParameters: new List<ReamingProgramPrameter>
            {
                new("13.3", 10m, 20m, 0.1m, null),
            });
            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var cnt = actual.Count(x => x.MainProgramClassification == NCProgramType.Chamfering);
            Assert.AreEqual(0, cnt);
        }

        [DataTestMethod()]
        [DataRow(13.3, MaterialType.Aluminum, 10.5, 380, 80)]
        [DataRow(13.3, MaterialType.Iron, 12.4, 290, 40)]
        public void 正常系_工程リーマが書き換えられること(
            double toolDiameter,
            MaterialType material,
            double expectedThickness,
            int expectedSpin,
            int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(
                material: material,
                thickness: (decimal)expectedThickness,
                directedOperationToolDiameter: (decimal)toolDiameter);
            IMainProgramParameterRewriter crystalReamingParameterRewriter = new CrystalReamingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Reaming);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Reaming);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.Reaming);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}
