using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
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
            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            decimal expectedCenterDrillDepth = param.CrystalReamerParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NcWordから値を取得する(IEnumerable<NcProgramCode> ncProgramCode, char address, NcProgramType ncProgram, int skip = 0)
        {
            return ncProgramCode
                .Where(x => x.MainProgramClassification == ncProgram)
                .Skip(skip)
                .Select(x => x.NcBlocks)
                .SelectMany(x => x)
                .Where(x => x != null)
                .Select(x => x?.NcWords)
                .Where(x => x != null)
                .SelectMany(x => x!)
                .Where(y => y!.GetType() == typeof(NcWord))
                .Cast<NcWord>()
                .Where(z => z.Address.Value == address)
                .Select(z => z.ValueData.Number)
                .FirstOrDefault();
        }

        [TestMethod]
        public void 正常系_コメントにツール径が追記されること()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create();
            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            var drDiameter = param.CrystalReamerParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.PreparedHoleDiameter)
                .First();
            Assert.AreEqual($"DR {drDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramType.Drilling));
            var dr2ndDiameter = param.CrystalReamerParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.SecondPreparedHoleDiameter)
                .First(); Assert.AreEqual($"DR {dr2ndDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramType.Drilling, 1));
            Assert.AreEqual($"REAMER {directedDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramType.Reaming));
        }

        private static string NcWordから始めのコメントを取得する(IEnumerable<NcProgramCode> ncProgramCode, NcProgramType ncProgram, int skip = 0)
        {
            return ncProgramCode.Where(x => x.MainProgramClassification == ncProgram)
                .Skip(skip)
                .Select(x => x.NcBlocks)
                .SelectMany(x => x)
                .Where(x => x != null)
                .Select(x => x?.NcWords)
                .Where(x => x != null)
                .SelectMany(x => x!)
                .Where(x => x!.GetType() == typeof(NcComment))
                .Cast<NcComment>()
                .First()
                .Comment;
        }

        [TestMethod]
        public void 異常系_素材が未定義の場合例外を返すこと()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: MaterialType.Undefined);
            void target()
            {
                IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
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
                IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<DomainException>(target);
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
            var crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴1の回転数");

            rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Drilling, 1);
            expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum, 1)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron, 1);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴2の回転数");

            decimal rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Drilling, 1);
            expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness, 1);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴2のZ");

            decimal rewritedCutDepth = NcWordから値を取得する(actual, 'Q', NcProgramType.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            expectedCutDepth = expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth, 1);
            rewritedCutDepth = NcWordから値を取得する(actual, 'Q', NcProgramType.Drilling, 1);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴2の切込");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramType.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");

            rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramType.Drilling, 1);
            expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum, 1)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron, 1);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴2の送り");
        }

        private static decimal ドリルパラメータから値を取得する(RewriteByToolRecord param, Func<DrillingProgramParameter, decimal> select, int skip = 0)
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
            return param.DrillingParameters
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
                crystalReamerParameters: new List<ReamingProgramParameter>
                {
                    TestReamingProgramParameterFactory.Create(DiameterKey: reamerDiameter.ToString(), PreparedHoleDiameter: 3),
                },
                drillingParameters: new List<DrillingProgramParameter>
                {
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.CrystalReamerParameters
                .Select(x => x.PreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<DomainException>(target);
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
                crystalReamerParameters: new List<ReamingProgramParameter>
                {
                    TestReamingProgramParameterFactory.Create(
                        DiameterKey: reamerDiameter.ToString(),
                        PreparedHoleDiameter: 20m,
                        SecondPreparedHoleDiameter: 3m),
                },
                drillingParameters: new List<DrillingProgramParameter>
                {
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
                _ = crystalReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.CrystalReamerParameters
                .Select(x => x.SecondPreparedHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<DomainException>(target);
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

            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");
            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Chamfering);
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
            var param = TestRewriteByToolRecordFactory.Create(crystalReamerParameters: new List<ReamingProgramParameter>
            {
                new("13.3", 10m, 20m, 0.1m, null),
            });
            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var cnt = actual.Count(x => x.MainProgramClassification == NcProgramType.Chamfering);
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
            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new CrystalReamingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Reaming);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Reaming);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramType.Reaming);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}
