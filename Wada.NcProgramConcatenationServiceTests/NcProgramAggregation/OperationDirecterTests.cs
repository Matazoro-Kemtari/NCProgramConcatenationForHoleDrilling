using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.NCProgramAggregation.Tests
{
    [TestClass()]
    public class OperationDirecterTests
    {
        [DataTestMethod()]
        [DynamicData(nameof(NCBlockOperations))]
        public void 正常系_作業指示が返ってくること(NcBlock ncBlock, DirectedOperationType directedOperationType, decimal toolDiameter)
        {
            // given
            var drillSizeData = new List<DrillSizeData>
            {
                TestDrillSizeDataFactory.Create(sizeIdentifier: "3/16", millimeter: 4.76m),
                TestDrillSizeDataFactory.Create(sizeIdentifier: "#C", millimeter: 6.15m),
                TestDrillSizeDataFactory.Create(sizeIdentifier: "#3", millimeter: 5.41m),
            };
            List<NcBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(),
                null,
                TestNCBlockFactory.Create(),
                ncBlock,
                TestNCBlockFactory.Create(),
                null,null,null,
                TestNCBlockFactory.Create(),
            };
            NcProgramCode ncProgramCode = new(NcProgramType.CenterDrilling, "O1000", ncBlocks);

            // when
            var operationDirecter = OperationDirecter.Create(ncProgramCode, drillSizeData);

            // then
            Assert.AreEqual(directedOperationType, operationDirecter.DirectedOperationClassification);
            Assert.AreEqual(toolDiameter, operationDirecter.DirectedOperationToolDiameter);
        }

        private static IEnumerable<object[]> NCBlockOperations => new List<object[]>
        {
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("3-M10") }),
                DirectedOperationType.Tapping,
                10m,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("3-D4.76H7") }),
                DirectedOperationType.Reaming,
                4.76m,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("4-D10DR") }),
                DirectedOperationType.Drilling,
                10m,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("2-3/16 P.H") }),
                DirectedOperationType.Reaming,
                4.76m,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("2-#C P.H") }),
                DirectedOperationType.Reaming,
                6.15m,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("2-#3 P.H") }),
                DirectedOperationType.Reaming,
                5.41m,
            },
        };

        [TestMethod]
        public void 異常系_作業指示がすべて無いときUndetectedを返すこと()
        {
            // given
            var drillSizeData = new List<DrillSizeData>
            {
                TestDrillSizeDataFactory.Create(),
            };
            List<NcBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                null,
                TestNCBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                TestNCBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                TestNCBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
                null,null,null,
                TestNCBlockFactory.Create(new List<INcWord>
                {
                    TestNcCommentFactory.Create(),
                    TestNcWordFactory.Create(),
                }),
            };
            NcProgramCode ncProgramCode = new(NcProgramType.CenterDrilling, "O1000", ncBlocks);
            
            // when
            void target()
            {
                _ = OperationDirecter.Create(ncProgramCode, drillSizeData);
            }

            // then
            var ex = Assert.ThrowsException<DirectedOperationNotFoundException>(target);
            string message = "作業指示が見つかりません";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public void 異常系_作業指示が複数あるとき例外を返すこと()
        {
            // given
            var drillSizeData = new List<DrillSizeData>
            {
                TestDrillSizeDataFactory.Create(),
            };
            List<NcBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(),
                null,
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("3-M10") }),
                null,
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("3-D4.76H7") }),
                null,
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment("4-D10DR") }),
                null,null,null,
                TestNCBlockFactory.Create(),
            };
            NcProgramCode ncProgramCode = new(NcProgramType.CenterDrilling, "O1000", ncBlocks);

            // when
            void target()
            {
                _ = OperationDirecter.Create(ncProgramCode, drillSizeData);
            }

            // then
            var ex = Assert.ThrowsException<DomainException>(target);
            string message = $"作業指示が3件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
            Assert.AreEqual(message, ex.Message);
        }

        [TestMethod]
        public void 異常系_インチリストにないインチ識別子が来たらDrillSizeDataExceptionがスローされること()
        {
            // given
            var drillSizeData = new List<DrillSizeData>
            {
                TestDrillSizeDataFactory.Create(),
            };
            var inch = "99/99";
            List<NcBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(new List<INcWord> { new NcComment($"2-{inch} P.H") }),
                null,
                TestNCBlockFactory.Create(),
            };
            NcProgramCode ncProgramCode = new(NcProgramType.CenterDrilling, "O1000", ncBlocks);

            // when
            void target()
            {
                _ = OperationDirecter.Create(ncProgramCode, drillSizeData);
            }

            // then
            var ex = Assert.ThrowsException<DrillSizeDataException>(target);
            var message = $"インチリストに該当がありません インチ: {inch}";
            Assert.AreEqual(message, ex.Message);
        }
    }
}