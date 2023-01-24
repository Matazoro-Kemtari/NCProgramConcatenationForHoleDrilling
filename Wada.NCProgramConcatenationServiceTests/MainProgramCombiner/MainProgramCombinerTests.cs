using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService.MainProgramCombiner.Tests
{
    [TestClass()]
    public class MainProgramCombinerTests
    {
        [TestMethod()]
        public void 正常系_NCプログラムが結合されること()
        {
            // given
            // when
            string[] name = new[]
            {
                "O0001",
                "O0002",
                "O0003",
            };
            List<NCProgramCode> combinableCodes = new()
            {
                TestNCProgramCodeFactory.Create(programName: name[0]),
                TestNCProgramCodeFactory.Create(programName: name[1]),
                TestNCProgramCodeFactory.Create(programName: name[2]),
            };
            IMainProgramCombiner combiner = new MainProgramCombiner();
            var combinedCode = combiner.Combine(combinableCodes);

            // then
            Assert.AreEqual(string.Join('>', name), combinedCode.ProgramName);
            var count = combinableCodes
                .Select(x => x.NCBlocks.Count())
                .Sum();
            Assert.AreEqual(count,combinedCode.NCBlocks.Count());
        }
    }
}