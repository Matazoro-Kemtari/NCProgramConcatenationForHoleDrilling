using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramFile.Tests
{
    [TestClass()]
    public class NCProgramRepositoryTests
    {
        [TestMethod()]
        public async Task 正常系_NCプログラムが読み込めること()
        {
            // given
            // テストデータ作成
            using StreamReader reader = new StreamReader(
                new MemoryStream(
                    Encoding.UTF8.GetBytes(ncProgramSource)))
                ?? throw new ArgumentNullException(
                    "StreamReader作るときに失敗した");

            // when
            string pgName = "O0150";
            INCProgramRepository ncProgramRepository = new NCProgramRepository();
            NCProgramCode actual = await ncProgramRepository.ReadAllAsync(reader, pgName);

            // then
            NCProgramCode expected = new(pgName, testNCBlocks);
            Assert.AreEqual(pgName, actual.ProgramName);

            CollectionAssert.AreEqual(
                expected.NCBlocks.Select(x => x?.ToString()).ToList(),
                actual.NCBlocks.Select(x => x?.ToString()).ToList());
        }

        internal static readonly string ncProgramSource =
@"%
O1000(SAMPLE)
(3-M10)
#100=10.

N100
S2000
M3
G01X50.Y-50.Z-10.F500
M5
M02
%
";
        internal static readonly IEnumerable<NCBlock?> testNCBlocks =
            new List<NCBlock?>
            {
                new NCBlock(
                    new List< INCWord>
                    {
                        new NCWord(new Address('O'),new NumericalValue("1000")),
                        new NCComment("SAMPLE"),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCComment("3-M10"),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCVariable(new VariableAddress(100),new CoordinateValue("10.")),
                    }),
                null,
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('N'),new NumericalValue("100")),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('S'),new NumericalValue("2000")),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("3")),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('G'),new NumericalValue("01")),
                        new NCWord(new Address('X'),new CoordinateValue("50.")),
                        new NCWord(new Address('Y'),new CoordinateValue("-50.")),
                        new NCWord(new Address('Z'),new CoordinateValue("-10.")),
                        new NCWord(new Address('F'),new NumericalValue("500")),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("5")),
                    }),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("02")),
                    }),
            };
    }
}