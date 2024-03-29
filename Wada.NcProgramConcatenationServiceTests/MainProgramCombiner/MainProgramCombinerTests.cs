﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramCombiner.Tests
{
    [TestClass()]
    public class MainProgramCombinerTests
    {
        [DataTestMethod]
        [DataRow("RB250F", "AL")]
        [DataRow("RB260", "AL")]
        [DataRow("611V", "AL")]
        [DataRow("RB250F", "SS400")]
        [DataRow("RB260", "SS400")]
        [DataRow("611V", "SS400")]
        public void 正常系_NCプログラムが結合されること(string machineToolName, string materialName)
        {
            // given
            // when
            string[] name = new[]
            {
                "O0001",
                "O0002",
                "O0003",
            };
            List<NcProgramCode> combinableCodes = new()
            {
                TestNcProgramCodeFactory.Create(programName: name[0]),
                TestNcProgramCodeFactory.Create(programName: name[1]),
                TestNcProgramCodeFactory.Create(programName: name[2]),
            };
            IMainProgramCombiner combiner = new MainProgramCombiner();
            var combinedCode = combiner.Combine(combinableCodes, machineToolName, materialName);

            // then
            Assert.AreEqual(string.Join('>', name), combinedCode.ProgramName);

            // 設備名1行＋プログラム間の改行＋ブロック数
            var count = 1;
            count += combinableCodes.Count() - 1;
            count += combinableCodes
                .Select(x => x.NcBlocks.Count())
                .Sum();
            Assert.AreEqual(count, combinedCode.NcBlocks.Count());

            Assert.AreEqual($"{machineToolName}-{materialName}", combinedCode.NcBlocks.First()?.NcWords.Cast<NcComment>().First().Comment);
        }
    }
}
