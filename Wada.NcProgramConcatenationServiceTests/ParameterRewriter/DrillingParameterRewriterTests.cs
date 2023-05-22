using Wada.NcProgramConcatenationService.ParameterRewriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class DrillingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_工程センタードリルが書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.CenterDrilling);
            decimal expectedCenterDrillDepth = param.DrillingParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値", NcProgramType.CenterDrilling);

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
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            Assert.AreEqual($"DR {directedDiameter}", NcWordから始めのコメントを取得する(actual, NcProgramType.Drilling));
        }

        private static string NcWordから始めのコメントを取得する(IEnumerable<NcProgramCode> ncProgramCode, NcProgramType ncProgram)
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
            var param = TestRewriteByToolRecordFactory.Create(material: MaterialType.Undefined);
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                _ = drillingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<ArgumentException>(target);
            Assert.AreEqual("素材が未定義です", ex.Message);
        }

        [TestMethod]
        public void 異常系_リストに一致するドリル径が無いとき例外を返すこと()
        {
            // given
            // when
            decimal diameter = 3m;
            var param = TestRewriteByToolRecordFactory.Create(directedOperationToolDiameter: diameter);
            void target()
            {
                IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
                _ = drillingParameterRewriter.RewriteByTool(param);
            }

            // then
            var ex = Assert.ThrowsException<DomainException>(target);
            Assert.AreEqual($"ドリル径 {diameter}のリストがありません",
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
            var drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Drilling);
            var expectedSpin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.SpinForIron);
            Assert.AreEqual(expectedSpin, rewritedSpin, "下穴の回転数");

            decimal rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Drilling);
            decimal expectedDepth = ドリルパラメータから値を取得する(param, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(expectedDepth, rewritedDepth, "下穴のZ");

            decimal rewritedCutDepth = NcWordから値を取得する(actual, 'Q', NcProgramType.Drilling);
            decimal expectedCutDepth = ドリルパラメータから値を取得する(param, x => x.CutDepth);
            Assert.AreEqual(expectedCutDepth, rewritedCutDepth, "下穴の切込");

            decimal rewritedFeed = NcWordから値を取得する(actual, 'F', NcProgramType.Drilling);
            decimal expectedFeed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param, x => x.FeedForIron);
            Assert.AreEqual(expectedFeed, rewritedFeed, "下穴1の送り");
        }

        private static decimal ドリルパラメータから値を取得する(RewriteByToolRecord param, Func<DrillingProgramParameter, decimal> select)
        {
            return param.DrillingParameters
                .Where(x => x.DirectedOperationToolDiameter == param.DirectedOperationToolDiameter)
                .Select(x => select(x))
                .FirstOrDefault();
        }

        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 1400)]
        [DataRow(MaterialType.Iron, 1100)]
        public void 正常系_工程面取りが書き換えられること(MaterialType material, int expectedSpin)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NcWordから値を取得する(actual, 'S', NcProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NcWordから値を取得する(actual, 'Z', NcProgramType.Chamfering);
            decimal? expectedChamferingDepth = param.DrillingParameters
                .Where(x => x.DiameterKey == param.DirectedOperationToolDiameter.ToString())
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "面取り深さ");
        }

        [TestMethod()]
        public void 面取りの最後Mコードが30になっていること()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create();
            IMainProgramParameterRewriter drillingParameterRewriter = new DrillingParameterRewriter();
            var actual = drillingParameterRewriter.RewriteByTool(param);

            // then
            var lastM30 = actual.Where(x => x.MainProgramClassification == NcProgramType.Chamfering)
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
    }
}
