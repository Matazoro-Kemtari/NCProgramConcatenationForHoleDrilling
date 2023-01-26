using System.Text;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    public record class NCProgramCode
    {
        public NCProgramCode(NCProgramType mainProgramClassification, string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            MainProgramClassification = mainProgramClassification;
            ProgramName = programName;
            NCBlocks = ncBlocks;
        }

        private NCProgramCode(Ulid id, NCProgramType mainProgramClassification, string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = id;
            MainProgramClassification = mainProgramClassification;
            ProgramName = programName;
            NCBlocks = ncBlocks;
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        /// <summary>
        /// 作業指示を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        [Logging]
        public DirectedOperationType FetchOperationType()
        {
            // 作業指示を探す
            IEnumerable<DirectedOperationType> hasOperationType = NCBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NCComment))
                .Select(w =>
                {
                    DirectedOperationType responce;
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)M\d+"))
                        responce = DirectedOperationType.Tapping;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d+(\.?\d+)?[HG]\d+"))
                        responce = DirectedOperationType.Reaming;
                    else if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d+(\.?\d+)?DR"))
                        responce = DirectedOperationType.Drilling;
                    else
                        responce = DirectedOperationType.Undetected;

                    return responce;
                }))
                .SelectMany(x => x);

            if (hasOperationType.All(x => x == DirectedOperationType.Undetected))
                // 有効な指示が1件もない場合
                return DirectedOperationType.Undetected;
            
            if (hasOperationType.Count(x => x != DirectedOperationType.Undetected) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != DirectedOperationType.Undetected)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new NCProgramConcatenationServiceException(msg);
            }

            return hasOperationType.First(x => x != DirectedOperationType.Undetected);
        }

        /// <summary>
        /// ツール径を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        [Logging]
        public decimal FetchTargetToolDiameter()
        {
            // 作業指示を探す
            IEnumerable<decimal> hasOperationType = NCBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NCComment))
                .Select(w =>
                {
                    var tapMatch = Regex.Match(w.ToString()!, @"(?<=-M)\d+(\.\d+)?");
                    var reamerMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=[HG]\d+)");
                    var drillMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=DR)");

                    decimal diameter;
                    if (tapMatch.Success)
                        diameter = decimal.Parse(tapMatch.Value);
                    else if (reamerMatch.Success)
                        diameter = decimal.Parse(reamerMatch.Value);
                    else if (drillMatch.Success)
                        diameter = decimal.Parse(drillMatch.Value);
                    else
                        diameter = decimal.MinValue;

                    return diameter;
                }))
                .SelectMany(x => x);

            if (hasOperationType.All(x => x == decimal.MinValue))
                // 有効な指示が1件もない場合
                return 0m;

            if (hasOperationType.Count(x => x != decimal.MinValue) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != decimal.MinValue)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new NCProgramConcatenationServiceException(msg);
            }

            return hasOperationType.First(x => x != decimal.MinValue);
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
            ncBlocks ??= new List<NCBlock>
            {
                TestNCBlockFactory.Create(
                    ncWords: new List<INCWord>
                    {
                        TestNCCommentFactory.Create(),
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
