using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.NcProgramAggregation.Tests
{
    [TestClass()]
    public class SubNCProgramCodeTests
    {
        [DataTestMethod()]
        [DynamicData(nameof(NCBlockOperations))]
        public void 正常系_作業指示が返ってくること(NcBlock ncBlock, DirectedOperationType expected)
        {
            // given
            // when
            List<NcBlock?> ncBlocks = new()
            {
                TestNcBlockFactory.Create(),
                null,
                TestNcBlockFactory.Create(),
                ncBlock,
                TestNcBlockFactory.Create(),
                null,null,null,
                TestNcBlockFactory.Create(),
            };
            NcProgramCode ncProgramCode = new(NcProgramRole.CenterDrilling, "O1000", ncBlocks);
            SubNCProgramCode subNCProgram = SubNCProgramCode.Parse(ncProgramCode);
            DirectedOperationType actual = subNCProgram.DirectedOperationClassification;

            // then
            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<object[]> NCBlockOperations => new List<object[]>
        {
            new object[] {
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("3-M10") }),
                DirectedOperationType.Tapping,
            },
            new object[] {
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("3-D4.76H7") }),
                DirectedOperationType.Reaming,
            },
            new object[] {
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("4-D10DR") }),
                DirectedOperationType.Drilling,
            },
        };

        [TestMethod]
        public void 異常系_作業指示がすべて無いときUndetectedを返すこと()
        {
            // given
            // when
            List<NcBlock?> ncBlocks = new()
            {
                TestNcBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                null,
                TestNcBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                TestNcBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                TestNcBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                null,null,null,
                TestNcBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
            };
            NcProgramCode ncProgramCode = new(NcProgramRole.CenterDrilling, "O1000", ncBlocks);
            void target()
            {
                _ = SubNCProgramCode.Parse(ncProgramCode);
            }

            // then
            var ex = Assert.ThrowsException<DirectedOperationNotFoundException>(target);
            string msg = "作業指示が見つかりません";
            Assert.AreEqual(msg, ex.Message);
        }

        [TestMethod]
        public void 異常系_作業指示が複数あるとき例外を返すこと()
        {
            // given
            // when
            List<NcBlock?> ncBlocks = new()
            {
                TestNcBlockFactory.Create(),
                null,
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("3-M10") }),
                null,
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("3-D4.76H7") }),
                null,
                TestNcBlockFactory.Create(new List<INcWord> { new NcComment("4-D10DR") }),
                null,null,null,
                TestNcBlockFactory.Create(),
            };
            NcProgramCode ncProgramCode = new(NcProgramRole.CenterDrilling, "O1000", ncBlocks);
            void target()
            {
                _ = SubNCProgramCode.Parse(ncProgramCode);
            }

            // then
            var ex = Assert.ThrowsException<NCProgramConcatenationServiceException>(target);
            string msg = $"作業指示が3件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
            Assert.AreEqual(msg, ex.Message);
        }
    }
}