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
        public void 正常系_工程センタードリルが書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var actual = tappingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.CenterDrilling);
            decimal expectedCenterDrillDepth = param.TapParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NCProgramCode> expected, char address, NCProgramType ncProgram, int skip = 0)
        {
            return expected
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
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                _ = tappingParameterRewriter.RewriteByTool(param);
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
            var param = TestRewriteByToolRecordFactory.Create(directedOperationToolDiameter: diameter);

            void target()
            {
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                _ = tappingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            Assert.AreEqual($"タップ径 {diameter}のリストがありません",
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
            var tappingParameterRewriter = new TappingParameterRewriter();
            var actual = tappingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NCProgramType.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(RewriteByToolRecord param, Func<DrillingProgramPrameter, decimal> select)
        {
            decimal drillDiameter = param.TapParameters
                .Where(x => x.DirectedOperationToolDiameter == param.DirectedOperationToolDiameter)
                .Select(x => x.PreparedHoleDiameter)
                .First();

            return param.DrillingPrameters
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
            var param = TestRewriteByToolRecordFactory.Create(
                directedOperationToolDiameter: reamerDiameter,
                tapParameters: new List<TappingProgramPrameter>
                {
                    TestTappingProgramPrameterFactory.Create(DiameterKey: $"M{reamerDiameter}", PreparedHoleDiameter: 3),
                },
                drillingPrameters: new List<DrillingProgramPrameter>
                {
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "20"),
                    TestDrillingProgramPrameterFactory.Create(DiameterKey: "22"),
                });

            void target()
            {
                IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
                _ = tappingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.TapParameters
                .Select(x => x.PreparedHoleDiameter)
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

            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var actual = tappingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Chamfering);
            decimal? expectedChamferingDepth = param.TapParameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "Z値");
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public void 正常系_工程タップが書き換えられること(MaterialType material, double expectedThickness)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(
                material: material,
                thickness: (decimal)expectedThickness);
            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var actual = tappingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NCProgramType.Tapping);
            decimal expectedSpin = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.SpinForAluminum : x.SpinForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NCProgramType.Tapping);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NCProgramType.Tapping);
            decimal expectedFeed = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.FeedForAluminum : x.FeedForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}