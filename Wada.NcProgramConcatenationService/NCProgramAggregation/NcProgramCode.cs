using System.Text;
using System.Text.RegularExpressions;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.NcProgramAggregation
{
    public record class NcProgramCode
    {
        public NcProgramCode(NcProgramType mainProgramClassification, string programName, IEnumerable<NcBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            MainProgramClassification = mainProgramClassification;
            ProgramName = mainProgramClassification switch
            {
                NcProgramType.SubProgram => FetchProgramNumber(programName),
                _ => programName,
            };
            NcBlocks = ncBlocks;
        }

        protected NcProgramCode(Ulid id, NcProgramType mainProgramClassification, string programName, IEnumerable<NcBlock?> ncBlocks)
        {
            ID = id;
            MainProgramClassification = mainProgramClassification;
            ProgramName = programName;
            NcBlocks = ncBlocks;
        }

        private static string FetchProgramNumber(string programName)
        {
            Match programNumberMatcher = Regex.Match(programName, @"\d+");
            if (!programNumberMatcher.Success)
                throw new DomainException(
                    "プログラム番号が取得できません" +
                    $"ファイル名を確認してください ファイル名: {programName}");

            return programNumberMatcher.Value;
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NcBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public static NcProgramCode ReConstruct(
            string id,
            NcProgramType mainProgramClassification,
            string programName,
            IEnumerable<NcBlock?> ncBlocks) => new(Ulid.Parse(id), mainProgramClassification, programName, ncBlocks);

        public Ulid ID { get; }

        /// <summary>
        /// メインプログラム種別
        /// </summary>
        public NcProgramType MainProgramClassification { get; init; }

        /// <summary>
        /// プログラム番号
        /// </summary>
        public string ProgramName { get; init; }

        public IEnumerable<NcBlock?> NcBlocks { get; init; }
    }

    /// <summary>
    /// ブロック
    /// </summary>
    /// <param name="NcWords">ワード</param>
    /// <param name="HasBlockSkip">オプショナルブロックスキップの有無</param>
    public record class NcBlock(IEnumerable<INcWord> NcWords, OptionalBlockSkip HasBlockSkip)
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

            NcWords.ToList().ForEach(x => buf.Append(x.ToString()));
            return buf.ToString();
        }
    }

    public class TestNcProgramCodeFactory
    {
        public static NcProgramCode Create(
            NcProgramType mainProgramType = NcProgramType.Reaming,
            string programName = "O0001",
            IEnumerable<NcBlock?>? ncBlocks = default)
        {
            var typeComment = mainProgramType switch
            {
                NcProgramType.CenterDrilling => "C/D",
                NcProgramType.Drilling => "DR",
                NcProgramType.Chamfering => "MENTORI",
                NcProgramType.Reaming => "REAMER",
                NcProgramType.Tapping => "TAP",
                _ => "COMMENT",
            };
            var lastMCode = mainProgramType switch
            {
                NcProgramType.Reaming => TestNcWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("30")),
                NcProgramType.Tapping => TestNcWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("30")),
                _ => TestNcWordFactory.Create(TestAddressFactory.Create('M'), TestNumericalValueFactory.Create("1")),
            };
            ncBlocks ??= new List<NcBlock>
            {
                TestNcBlockFactory.Create(
                    ncWords: new List<INcWord>
                    {
                        TestNcCommentFactory.Create(typeComment),
                    }),
                TestNcBlockFactory.Create(
                    ncWords: new List<INcWord>
                    {
                        TestNcWordFactory.Create(
                            address: TestAddressFactory.Create('M'),
                            valueData: TestNumericalValueFactory.Create("3")),
                        TestNcWordFactory.Create(
                            address: TestAddressFactory.Create('S'),
                            valueData: TestNumericalValueFactory.Create("*")),
                    }),
                TestNcBlockFactory.Create(),
                TestNcBlockFactory.Create(ncWords: new List<INcWord> { lastMCode }),
            };
            return new(mainProgramType, programName, ncBlocks);
        }
    }

    public class TestNcBlockFactory
    {
        public static NcBlock Create(IEnumerable<INcWord>? ncWords = default)
        {
            ncWords ??= new List<INcWord>
            {
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('G'),
                    valueData: TestNumericalValueFactory.Create("98")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('G'),
                    valueData: TestNumericalValueFactory.Create("82")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('R'),
                    valueData: TestCoordinateValueFactory.Create("3")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('Z'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('P'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('Q'),
                    valueData: TestCoordinateValueFactory.Create("*")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('F'),
                    valueData: TestNumericalValueFactory.Create("*")),
                TestNcWordFactory.Create(
                    address: TestAddressFactory.Create('L'),
                    valueData: TestNumericalValueFactory.Create("0")),
            };
            return new(ncWords, OptionalBlockSkip.None);
        }
    }
}
