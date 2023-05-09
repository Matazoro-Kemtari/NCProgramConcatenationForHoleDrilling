using Wada.NCProgramFile;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;
using System.Text.RegularExpressions;

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
                ?? throw new DomainException(
                    "StreamReader作るときに失敗した");

            // when
            string pgName = "O0150";
            Match pgNameMatcher = Regex.Match(pgName, @"\d+");
            NCProgramType ncProgram = NCProgramType.SubProgram;
            INCProgramRepository ncProgramRepository = new NCProgramRepository();
            NCProgramCode actual = await ncProgramRepository.ReadAllAsync(reader, ncProgram, pgName);

            // then
            NCProgramCode expected = new(ncProgram, pgName, testNCBlocks);
            Assert.AreEqual(pgNameMatcher.Value, actual.ProgramName);

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
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCComment("3-M10"),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCVariable(new VariableAddress(100),new CoordinateValue("10.")),
                    },
                    OptionalBlockSkip.None),
                null,
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('N'),new NumericalValue("100")),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('S'),new NumericalValue("2000")),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("3")),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('G'),new NumericalValue("01")),
                        new NCWord(new Address('X'),new CoordinateValue("50.")),
                        new NCWord(new Address('Y'),new CoordinateValue("-50.")),
                        new NCWord(new Address('Z'),new CoordinateValue("-10.")),
                        new NCWord(new Address('F'),new NumericalValue("500")),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("5")),
                    },
                    OptionalBlockSkip.None),
                new NCBlock(
                    new List<INCWord>
                    {
                        new NCWord(new Address('M'),new NumericalValue("02")),
                    },
                    OptionalBlockSkip.None),
            };

        [DataTestMethod]
        [DynamicData(nameof(MainPrograms))]
        public async Task 正常系_メインプログラムが読み込めること(string mainSource, int count, string sourceType, char[] parameterAddresses)
        {
            // given
            // テストデータ作成
            using StreamReader reader = new StreamReader(
                new MemoryStream(
                    Encoding.UTF8.GetBytes(mainSource)))
                ?? throw new DomainException(
                    "StreamReader作るときに失敗した");

            // when
            INCProgramRepository ncProgramRepository = new NCProgramRepository();
            NCProgramType ncProgram = NCProgramType.CenterDrilling;
            NCProgramCode actual = await ncProgramRepository.ReadAllAsync(reader, ncProgram, string.Empty);

            // then
            NCProgramCode expected = new(ncProgram, string.Empty, testNCBlocks);
            Assert.AreEqual(string.Empty, actual.ProgramName);
            Assert.AreEqual(count, actual.NCBlocks.Count());
            Assert.AreEqual(sourceType, actual.NCBlocks.FirstOrDefault()?.ToString());
            var actualParameterAddresses = actual.NCBlocks
                .Where(x => x != null)
                .Select(x => x!.NCWords
                    .Where(y => y.GetType() == typeof(NCWord))
                    .Cast<NCWord>()
                    .Where(y => y.ValueData.Value.Contains('*'))
                    .Select(y => y.Address.Value)
                )
                .SelectMany(x => x)
                .ToList();
            CollectionAssert.AreEquivalent(parameterAddresses.ToList(), actualParameterAddresses);
        }

        private static IEnumerable<object[]> MainPrograms => new List<object[]>
        {
            new object[] { mainCDSource, 23, "(C/D)", new char[] { 'S', 'Z'} },
            new object[] { mainDRSource, 23, "(DR)", new char[] { 'S', 'Z', 'Q', 'F' } },
            new object[] { mainChamfering, 23, "(MENTORI)", new char[] { 'S', 'Z' } },
            new object[] { mainReamer, 23, "(REAMER)", new char[] { 'S', 'Z', 'F' } },
            new object[] { mainTap , 23, "(TAP)", new char[] { 'S', 'Z', 'F' } },
        };
        private static readonly string mainCDSource = @"(C/D)

T40
M6Q0
G91G28G0Z0.
M1

G54
G90G60G0X0.Y0.
B0.C0.
W0.
G43Z100.H40
M01
/M8
M3S****

G98G82R3.Z***P1000F150L0

G80
M5
M9
G91G28G0Z0.
M1
";
        private static readonly string mainDRSource = @"(DR)

T40
M6Q0
G91G28G0Z0.
M1

G54
G90G60G0X0.Y0.
B0.C0.
W0.
G43Z100.H40
M01
/2M8
M3S****

G98G83R3.Z***.Q**.F***L0

G80
M5
M9
G91G28G0Z0.
M1
";
        private static readonly string mainChamfering = @"(MENTORI)

T40
M6Q0
G91G28G0Z0.
M1

G54
G90G60G0X0.Y0.
B0.C0.
W0.
G43Z100.H40
M01
/3M8
M3S****

G98G82R0.Z***.P1000.F100L0

G80
M5
M9
G91G28G0Z0.
M1
";
        private static readonly string mainReamer = @"(REAMER)

T40
M6Q0
G91G28G0Z0.
M1

G54
G90G60G0X0.Y0.
B0.C0.
W0.
G43Z100.H40
M00
/4M8
M3S****

G98G85R5.Z***.F***L0

G80
M5
M9
G91G28G0Z0.
M30
";
        private static readonly string mainTap = @"(TAP)

T40
M6Q0
G91G28G0Z0.
M1

G54
G90G60G0X0.Y0.
B0.C0.
W0.
G43Z100.H40
M00
/5M8
M3S****

G98G84R5.Z***.F***L0

G80
M5
M9
G91G28G0Z0.
M1
";

        [TestMethod()]
        public async Task 正常系_ストリームに書き込まれること()
        {
            // given
            // when
            using MemoryStream stream = new();
            using StreamWriter writer = new(stream);
            var expected = TestNCProgramCodeFactory.Create().ToString();
            INCProgramRepository repository = new NCProgramRepository();
            await repository.WriteAllAsync(writer, expected);

            // then
            // ストリームの位置を戻す
            stream.Seek(0, SeekOrigin.Begin);
            // 書き込んだ内容を取得する
            using StreamReader reader = new(stream);
            var actual = await reader.ReadToEndAsync();

            Assert.AreEqual(expected, actual);
        }
    }
}