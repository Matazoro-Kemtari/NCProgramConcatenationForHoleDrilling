using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
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
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.CenterDrilling);
            decimal expectedCenterDrillDepth = param.TapParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NcProgramCode> expected, char address, NcProgramType ncProgram, int skip = 0)
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
            IMainProgramParameterRewriter crystalReamingParameterRewriter = new TappingParameterRewriter();
            var actual = crystalReamingParameterRewriter.RewriteByTool(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            var drDiameter = param.TapParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.PreparedHoleDiameter)
                .First();
            Assert.AreEqual($"DR {drDiameter}", NCWordから始めのコメントを取得する(actual, NcProgramType.Drilling));
            Assert.AreEqual($"TAP M{directedDiameter}", NCWordから始めのコメントを取得する(actual, NcProgramType.Tapping));
        }

        private static string NCWordから始めのコメントを取得する(IEnumerable<NcProgramCode> ncProgramCode, NcProgramType ncProgram)
        {
            return ncProgramCode.Where(x => x.MainProgramClassification == ncProgram)
                .Select(x => x.NCBlocks)
                .SelectMany(x => x)
                .Where(x => x != null)
                .Select(x => x?.NCWords)
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
            var ex = Assert.ThrowsException<DomainException>(target);
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
            var rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴1のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NcProgramType.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴1の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.Drilling);
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

            IMainProgramParameterRewriter tappingParameterRewriter = new TappingParameterRewriter();
            var actual = tappingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Chamfering);
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
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Tapping);
            decimal expectedSpin = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.SpinForAluminum : x.SpinForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Tapping);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.Tapping);
            decimal expectedFeed = param.TapParameters
                .Select(x => material == MaterialType.Aluminum ? x.FeedForAluminum : x.FeedForIron)
                .FirstOrDefault();
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}