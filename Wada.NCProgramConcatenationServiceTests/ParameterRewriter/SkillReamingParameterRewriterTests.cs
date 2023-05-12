using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Tests
{
    [TestClass()]
    public class SkillReamingParameterRewriterTests
    {
        [DataTestMethod()]
        [DataRow(MaterialType.Aluminum, 2000, 150)]
        [DataRow(MaterialType.Iron, 1500, 100)]
        public void 正常系_工程センタードリルが書き換えられること(MaterialType material, int expectedSpin, int expectedFeed)
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(material: material);
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.CenterDrilling);
            decimal expectedCenterDrillDepth = param.SkillReamerParameters
                .Select(x => x.CenterDrillDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedCenterDrillDepth, rewritedDepth, "Z値");

            var rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.CenterDrilling);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }

        private static decimal NCWordから値を取得する(IEnumerable<NcProgramCode> ncProgramCode, char address, NcProgramType ncProgram, int skip = 0)
        {
            return ncProgramCode
                .Where(x => x.MainProgramClassification == ncProgram)
                .Skip(skip)
                .Select(x => x.NcBlocks)
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
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            var directedDiameter = param.DirectedOperationToolDiameter;
            var drDiameter = param.SkillReamerParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.PreparedHoleDiameter)
                .First();
            Assert.AreEqual($"DR {drDiameter}", NCWordから始めのコメントを取得する(actual, NcProgramType.Drilling));
            var dr2ndDiameter = param.SkillReamerParameters
                .Where(x => x.DirectedOperationToolDiameter == directedDiameter)
                .Select(x => x.SecondPreparedHoleDiameter)
                .First(); Assert.AreEqual($"DR {dr2ndDiameter}", NCWordから始めのコメントを取得する(actual, NcProgramType.Drilling, 1));
            Assert.AreEqual($"REAMER {directedDiameter}", NCWordから始めのコメントを取得する(actual, NcProgramType.Reaming));
        }

        private static string NCWordから始めのコメントを取得する(IEnumerable<NcProgramCode> ncProgramCode, NcProgramType ncProgram, int skip = 0)
        {
            return ncProgramCode.Where(x => x.MainProgramClassification == ncProgram)
                .Skip(skip)
                .Select(x => x.NcBlocks)
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
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
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
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
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
            var param = TestRewriteByToolRecordFactory.Create(material: material, thickness: (decimal)thickness);
            var skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            var rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Drilling);
            var spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForIron);
            Assert.AreEqual(spin, rewritedSpin, "下穴1の回転数");

            rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Drilling, 1);
            spin = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForAluminum, 1)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.SpinForIron, 1);
            Assert.AreEqual(spin, rewritedSpin, "下穴2の回転数");

            decimal rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Drilling);
            decimal depth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => -x.DrillTipLength - (decimal)thickness);
            Assert.AreEqual(depth, rewritedDepth, "下穴1のZ");

            rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Drilling, 1);
            depth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => -x.DrillTipLength - (decimal)thickness, 1);
            Assert.AreEqual(depth, rewritedDepth, "下穴2のZ");

            decimal rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NcProgramType.Drilling);
            decimal cutDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.CutDepth);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴1の切込");

            cutDepth = cutDepth = ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.CutDepth, 1);
            rewritedCutDepth = NCWordから値を取得する(actual, 'Q', NcProgramType.Drilling, 1);
            Assert.AreEqual(cutDepth, rewritedCutDepth, "下穴2の切込");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.Drilling);
            decimal feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForAluminum)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForIron);
            Assert.AreEqual(feed, rewritedFeed, "下穴1の送り");

            rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.Drilling, 1);
            feed = material == MaterialType.Aluminum
                ? ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForAluminum, 1)
                : ドリルパラメータから値を取得する(param.DrillingPrameters, x => x.FeedForIron, 1);
            Assert.AreEqual(feed, rewritedFeed, "下穴2の送り");
        }

        private static decimal ドリルパラメータから値を取得する(IEnumerable<DrillingProgramPrameter> drillingProgramPrameter, Func<DrillingProgramPrameter, decimal> select, int skip = 0)
        {
            return drillingProgramPrameter.Skip(skip)
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
                skillReamerParameters: new List<ReamingProgramPrameter>
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
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.SkillReamerParameters
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
                skillReamerParameters: new List<ReamingProgramPrameter>
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
                IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
                _ = skillReamingParameterRewriter.RewriteByTool(param);
            }

            // then
            var fastDrill = param.SkillReamerParameters
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

            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Chamfering);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Chamfering);
            decimal? expectedChamferingDepth = param.SkillReamerParameters
                .Select(x => x.ChamferingDepth)
                .FirstOrDefault();
            Assert.AreEqual(expectedChamferingDepth, rewritedDepth, "Z値");
        }

        [TestMethod]
        public void 正常系_パラメータで面取りが無のとき面取りのNCプログラムがないこと()
        {
            // given
            // when
            var param = TestRewriteByToolRecordFactory.Create(skillReamerParameters: new List<ReamingProgramPrameter>
            {
                new("13.3", 10m, 20m, 0.1m, null),
            });
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            var cnt = actual.Count(x => x.MainProgramClassification == NcProgramType.Chamfering);
            Assert.AreEqual(0, cnt);
        }

        [DataTestMethod()]
        [DataRow(13.3,MaterialType.Aluminum, 10.5, 1130, 140)]
        [DataRow(13.3,MaterialType.Iron, 12.4, 360, 40)]
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
            IMainProgramParameterRewriter skillReamingParameterRewriter = new SkillReamingParameterRewriter();
            var actual = skillReamingParameterRewriter.RewriteByTool(param);

            // then
            decimal rewritedSpin = NCWordから値を取得する(actual, 'S', NcProgramType.Reaming);
            Assert.AreEqual(expectedSpin, rewritedSpin, "回転数");

            var rewritedDepth = NCWordから値を取得する(actual, 'Z', NcProgramType.Reaming);
            Assert.AreEqual((decimal)-expectedThickness - 5m, rewritedDepth, "Z値");

            decimal rewritedFeed = NCWordから値を取得する(actual, 'F', NcProgramType.Reaming);
            Assert.AreEqual(expectedFeed, rewritedFeed, "送り");
        }
    }
}