using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.UseCase.DataClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;
using Wada.NCProgramConcatenationService;

namespace Wada.UseCase.DataClass.Tests
{
    [TestClass()]
    public class SubNCProgramCodeAttempTests
    {
        [DataTestMethod()]
        [DynamicData(nameof(NCBlockOperations))]
        public void 正常系_作業指示が返ってくること(NCBlock ncBlock, DirectedOperationTypeAttempt expected)
        {
            // given
            // when
            List<NCBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(),
                null,
                TestNCBlockFactory.Create(),
                ncBlock,
                TestNCBlockFactory.Create(),
                null,null,null,
                TestNCBlockFactory.Create(),
            };
            NCProgramCode ncProgramCode = new(NCProgramType.CenterDrilling, "O1000", ncBlocks);
            SubNCProgramCodeAttemp subNCProgram = SubNCProgramCodeAttemp.Parse(ncProgramCode);
            DirectedOperationTypeAttempt actual = subNCProgram.DirectedOperationClassification;

            // then
            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<object[]> NCBlockOperations => new List<object[]>
        {
            new object[] {
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("3-M10") }),
                DirectedOperationTypeAttempt.Tapping,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("3-D4.76H7") }),
                DirectedOperationTypeAttempt.Reaming,
            },
            new object[] {
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("4-D10DR") }),
                DirectedOperationTypeAttempt.Drilling,
            },
        };

        [TestMethod]
        public void 正常系_作業指示がすべて無いときUndetectedを返すこと()
        {
            // given
            // when
            List<NCBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(new List<INCWord>
                {
                    TestNCCommentFactory.Create(),
                    TestNCWordFactory.Create(),
                }),
                null,
                TestNCBlockFactory.Create(new List<INCWord>
                {
                    TestNCCommentFactory.Create(),
                    TestNCWordFactory.Create(),
                }),
                TestNCBlockFactory.Create(new List<INCWord>
                {
                    TestNCCommentFactory.Create(),
                    TestNCWordFactory.Create(),
                }),
                TestNCBlockFactory.Create(new List<INCWord>
                {
                    TestNCCommentFactory.Create(),
                    TestNCWordFactory.Create(),
                }),
                null,null,null,
                TestNCBlockFactory.Create(new List<INCWord>
                {
                    TestNCCommentFactory.Create(),
                    TestNCWordFactory.Create(),
                }),
            };
            NCProgramCode ncProgramCode = new(NCProgramType.CenterDrilling, "O1000", ncBlocks);
            SubNCProgramCodeAttemp subNCProgram = SubNCProgramCodeAttemp.Parse(ncProgramCode);
            DirectedOperationTypeAttempt actual = subNCProgram.DirectedOperationClassification;

            // then
            Assert.AreEqual(DirectedOperationTypeAttempt.Undetected, actual);
        }

        [TestMethod]
        public void 異常系_作業指示が複数あるとき例外を返すこと()
        {
            // given
            // when
            List<NCBlock?> ncBlocks = new()
            {
                TestNCBlockFactory.Create(),
                null,
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("3-M10") }),
                null,
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("3-D4.76H7") }),
                null,
                TestNCBlockFactory.Create(new List<INCWord> { new NCComment("4-D10DR") }),
                null,null,null,
                TestNCBlockFactory.Create(),
            };
            NCProgramCode ncProgramCode = new(NCProgramType.CenterDrilling, "O1000", ncBlocks);
            void target()
            {
                _ = SubNCProgramCodeAttemp.Parse(ncProgramCode);
            }

            // then
            var ex = Assert.ThrowsException<UseCase_DataClassException>(target);
            string msg = $"作業指示が3件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
            Assert.AreEqual(msg, ex.Message);
        }
    }
}