using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class TappingSequenceBuilderTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_タップシーケンスのセンタードリル工程が書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(material: material);
            IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
            var actual = tappingSequenceBuilder.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.CenterDrilling);
            decimal expectedCenterDrillDepth = param.TapParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramRole.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NcWordから値を取得する(IEnumerable<NcProgramCode> expected, char address, NcProgramRole ncProgram, int skip = 0)
        {
            return expected
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
            var param = TestRewriteByToolArgFactory.Create();
            IMainProgramSequenceBuilder crystalReamingParameterRewriter = new TappingSequenceBuilder();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            var drDiameter = param.TapParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.PilotHoleDiameter)
                .First();
            Assert.AreEqual($"DR {drDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramRole.Drilling));
            Assert.AreEqual($"TAP M{directedDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramRole.Tapping));
        }

        private static string NcWordから始めのコメントを取得する(IEnumerable<NcProgramCode> ncProgramCode, NcProgramRole ncProgram)
        {
            return ncProgramCode.Where(x => x.MainProgramClassification == ncProgram)
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
            var param = TestRewriteByToolArgFactory.Create(material: MaterialType.Undefined);

            void target()
            {
                IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
                _ = tappingSequenceBuilder.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するタップ径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolArgFactory.Create(directedOperationToolDiameter: diameter);

            void target()
            {
                IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
                _ = tappingSequenceBuilder.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<DomainException>(target);
            Assert.AreEqual($"タップ径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_タップシーケンスの下穴工程が書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(
                material: material,
                thickness: (decimal)thickness);
            var tappingSequenceBuilder = new TappingSequenceBuilder();
            var actual = tappingSequenceBuilder.RewriteByTool(param);

            // then
            var rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            decimal rewritedCutDepth = NcWordから値を取得する(actual, 'Q', NcProgramRole.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramRole.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(ToolParameter param, Func<DrillingProgramParameter, decimal> select)
        {
            decimal drillDiameter = param.TapParameters
                .Where(x => x.DirectedOperationToolDiameter == param.DirectedOperationToolDiameter)
                .Select(x => x.PilotHoleDiameter)
                .First();

            return param.DrillingParameters
                .Where(x => x.DirectedOperationToolDiameter == drillDiameter)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [TestMethod]
        public void 異常系_下穴に該当するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal reamerDiameter = 5.5m;
            var param = TestRewriteByToolArgFactory.Create(
                directedOperationToolDiameter: reamerDiameter,
                tapParameters: new List<TappingProgramParameter>
                {
                    TestTappingProgramParameterFactory.Create(DiameterKey: $"M{reamerDiameter}", PilotHoleDiameter: 3),
                },
                drillingParameters: new List<DrillingProgramParameter>
                {
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramParameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
                _ = tappingSequenceBuilder.RewriteByTool(param);
            }

            // then
            var fastDrill = param.TapParameters
                .Select(x => x.PilotHoleDiameter)
                .FirstOrDefault();
            var ex = Assert.ThrowsException<DomainException>(target);
            Assert.AreEqual($"穴径に該当するリストがありません 穴径: {fastDrill}",
                ex.Message);
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_タップシーケンスの面取り工程が書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(material: material);

            IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
            var actual = tappingSequenceBuilder.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Chamfering);
            decimal? expectedChamferingDepth = param.TapParameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "Z値");
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_タップシーケンスのタップ工程が書き換えられること(MaterialType material, double expectedThickness)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(
                material: material,
                thickness: (decimal)expectedThickness);
            IMainProgramSequenceBuilder tappingSequenceBuilder = new TappingSequenceBuilder();
            var actual = tappingSequenceBuilder.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.Tapping);
            decimal expectedSpin = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.SpinForAluminum : x.SpinForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Tapping);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramRole.Tapping);
            decimal expectedFeed = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.FeedForAluminum : x.FeedForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        [TestMethod]
        public void 正常系_タップシーケンスの止まり穴の穴深さが書き換えられること()
        {
            // given
            var param = TestRewriteByToolArgFactory.Create(
                drillingMethod: DrillingMethod.BlindHole,
                blindPilotHoleDepth: 10.25m,
                blindHoleDepth: 8.75m);

            // when
            var tappingSequenceBuilder = new TappingSequenceBuilder();
            var actual = tappingSequenceBuilder.RewriteByTool(param);

            // then
            var rewritedPilotDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Drilling);
            Assert.AreEqual(-param.BlindPilotHoleDepth, rewritedPilotDepth, "下穴-Z値");
            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Tapping);
            Assert.AreEqual(-param.BlindHoleDepth, rewritedDepth, "タップ-Z値");
        }
    }
}
