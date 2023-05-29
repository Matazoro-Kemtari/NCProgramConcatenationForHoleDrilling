using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class DrillingSequenceBuilderTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public async Task 正常系_ドリルシーケンスのセンタードリル工程が書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(material: material);
            IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.CenterDrilling);
            decimal expectedCenterDrillDepth = param.DrillingParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値", NcProgramRole.CenterDrilling);

            var rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramRole.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NcWordから値を取得する(IEnumerable<NcProgramCode> ncProgramCode, char address, NcProgramRole ncProgram, int skip = 0)
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
        public async Task 正常系_コメントにツール径が追記されること()
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create();
            IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            Assert.AreEqual($"DR {directedDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramRole.Drilling));
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
        public async Task 異常系_素材が未定義の場合例外を返すこと()
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(material: MaterialType.Undefined);
            async Task targetAsync()
            {
                IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
                _ = await drillingSequenceBuilder.RewriteByToolAsync(param);
            }

            // then
            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(targetAsync);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public async Task 異常系_リストに一致するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolArgFactory.Create(directedOperationToolDiameter: diameter);
            async Task targetAsync()
            {
                IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
                _ = await drillingSequenceBuilder.RewriteByToolAsync(param);
            }

            // then
            var ex = await Assert.ThrowsExceptionAsync<DomainException>(targetAsync);
            Assert.AreEqual($"ドリル径 {diameter}のリストがありません",
                ex.Message);
        }

        [DataTestMethod]
        [DataRow(MaterialType.Aluminum, 10.5)]
        [DataRow(MaterialType.Iron, 12.4)]
        public async Task 正常系_ドリルシーケンスの下穴工程が書き換えられること(MaterialType material, double thickness)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(
                material: material,
                thickness: (decimal)thickness);
            var drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            var rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴のZ");

            decimal rewritedCutDepth = NcWordから値を取得する(actual, 'Q', NcProgramRole.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴の切込");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramRole.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(ToolParameter param, Func<DrillingProgramParameter, decimal> select)
        {
            return param.DrillingParameters
                .Where(x => x.DirectedOperationToolDiameter == param.DirectedOperationToolDiameter)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public async Task 正常系_ドリルシーケンスの面取り工程が書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create(material: material);
            IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramRole.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Chamfering);
            decimal? expectedChamferingDepth = param.DrillingParameters
                .Where(x => x.DiameterKey == param.DirectedOperationToolDiameter.ToString())
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "面取り深さ");
        }

        [TestMethod()]
        public async Task 面取りの最後Mコードが30になっていること()
        {
            // given
            // when
            var param = TestRewriteByToolArgFactory.Create();
            IMainProgramSequenceBuilder drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            var lastM30 = actual.Where(x => x.MainProgramClassification == NcProgramRole.Chamfering)
                                .Select(x => x.NcBlocks)
                                .SelectMany(x => x)
                                .Where(x => x != null)
                                .Select(x => x?.NcWords)
                                .Where(x => x != null)
                                .SelectMany(x => x!)
                                .Where(x => x.GetType() == typeof(NcWord))
                                .Cast<NcWord>()
                                .Where(x => x.Address.Value == 'M')
                                .Select(x => x.ValueData.Number)
                                .Last();

            Assert.AreEqual(30, lastM30);
        }

        [TestMethod]
        public async Task 正常系_ドリルシーケンスの止まり穴のドリル工程が書き換えられること()
        {
            // given
            var param = TestRewriteByToolArgFactory.Create(
                drillingMethod: DrillingMethod.BlindHole,
                blindHoleDepth: 4m);

            // when
            var drillingSequenceBuilder = new DrillingSequenceBuilder();
            var actual = await drillingSequenceBuilder.RewriteByToolAsync(param);

            // then
            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramRole.Drilling);
            decimal expectedCenterDrillDepth = -param.BlindHoleDepth;
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値", NcProgramRole.CenterDrilling);
        }
    }
}
