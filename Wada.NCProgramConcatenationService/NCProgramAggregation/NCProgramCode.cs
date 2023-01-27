using System.Text;
using System.Text.RegularExpressions;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    public record class NCProgramCode
    {
        public NCProgramCode(NCProgramType mainProgramClassification, string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            MainProgramClassification = mainProgramClassification;
            ProgramName = mainProgramClassification switch
            {
                NCProgramType.SubProgram => FetchProgramNumber(programName),
                _ => programName,
            };
            NCBlocks = ncBlocks;
        }

        protected NCProgramCode(Ulid id, NCProgramType mainProgramClassification, string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = id;
            MainProgramClassification = mainProgramClassification;
            ProgramName = programName;
            NCBlocks = ncBlocks;
        }

        private static string FetchProgramNumber(string programName)
        {
            Match programNumberMatcher = Regex.Match(programName, @"\d+");
            if (!programNumberMatcher.Success)
                throw new NCProgramConcatenationServiceException(
                    "プログラム番号が取得できません" +
                    $"ファイル名を確認してください ファイル名: {programName}");

            return programNumberMatcher.Value;
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public static NCProgramCode ReConstruct(
            string id,
            NCProgramType mainProgramClassification,
            string programName,
            IEnumerable<NCBlock?> ncBlocks) => new(Ulid.Parse(id), mainProgramClassification, programName, ncBlocks);

        public Ulid ID { get; }

        /// <summary>
        /// メインプログラム種別
        /// </summary>
        public NCProgramType MainProgramClassification { get; init; }

        /// <summary>
        /// プログラム番号
        /// </summary>
        public string ProgramName { get; init; }

        public IEnumerable<NCBlock?> NCBlocks { get; init; }
    }

    /// <summary>
    /// ブロック
    /// </summary>
    /// <param name="NCWords">ワード</param>
    /// <param name="HasBlockSkip">オプショナルブロックスキップの有無</param>
    public record class NCBlock(IEnumerable<INCWord> NCWords, OptionalBlockSkip HasBlockSkip)
    {
        public override string ToString()
        {
            StringBuilder buf = new();
            if (HasBlockSkip != OptionalBlockSkip.None)
            {
                if (HasBlockSkip == OptionalBlockSkip.BDT1)
                    buf.Append('/');
                else
                    buf.Append("/" + (int)HasBlockSkip);
            }

            NCWords.ToList().ForEach(x => buf.Append(x.ToString()));
            return buf.ToString();
        }
    }

    public class TestNCProgramCodeFactory
    {
        public static NCProgramCode Create(
            NCProgramType mainProgramType = NCProgramType.Reaming,
            string programName = "O0001",
            IEnumerable<NCBlock?>? ncBlocks = default)
        {
            var typeComment = mainProgramType switch
            {
                NCProgramType.CenterDrilling => "C/D",
                NCProgramType.Drilling => "DR",
                NCProgramType.Chamfering => "MENTORI",
                NCProgramType.Reaming => "REAMER",
                NCProgramType.Tapping => "TAP",
                _ => "COMMENT",
            };
            var lastMCode = mainProgramType switch
            {
                NCProgramType.Reaming => TestNCWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("30")),
                NCProgramType.Tapping => TestNCWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("30")),
                _ => TestNCWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("1")),
            };
            ncBlocks ??= new List<NCBlock>
            {
                TestNCBlockFactory.Create(
                    ncWords: new List<INCWord>
                    {
                        TestNCCommentFactory.Create(typeComment),
                    }),
                TestNCBlockFactory.Create(
                    ncWords: new List<INCWord>
                    {
                        TestNCWordFactory.Create(
                            address: TestAddressFactory.Create('M'),
                            valueData: TestNumericalValueFactory.Create("3")),
                        TestNCWordFactory.Create(
                            address: TestAddressFactory.Create('S'),
                            valueData: TestNumericalValueFactory.Create("*")),
                    }),
                TestNCBlockFactory.Create(),
                TestNCBlockFactory.Create(ncWords: new List<INCWord> { lastMCode }),
            };
            return new(mainProgramType, programName, ncBlocks);
        }
    }

    public class TestNCBlockFactory
    {
        public static NCBlock Create(IEnumerable<INCWord>? ncWords = default)
        {
            ncWords ??= new List<INCWord>
            {
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('G'),
                    valueData: TestNumericalValueFactory.Create("98")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('G'),
                    valueData: TestNumericalValueFactory.Create("82")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('R'),
                    valueData: TestCoordinateValueFactory.Create("3")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('Z'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('P'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('Q'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('F'),
                    valueData: TestNumericalValueFactory.Create("*")),
                TestNCWordFactory.Create(
                    address: TestAddressFactory.Create('L'),
                    valueData: TestNumericalValueFactory.Create("0")),
            };
            return new(ncWords, OptionalBlockSkip.None);
        }
    }
}
