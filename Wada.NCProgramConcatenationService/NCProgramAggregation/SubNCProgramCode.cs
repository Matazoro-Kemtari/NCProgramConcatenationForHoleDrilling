using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    public record class SubNCProgramCode : NCProgramCode
    {
        public SubNCProgramCode(
            NCProgramType mainProgramClassification,
            string programName,
            IEnumerable<NCBlock?> ncBlocks)
            : base(mainProgramClassification, programName, ncBlocks)
        {
            DirectedOperationClassification = FetchDirectedOperationType(ncBlocks);
            DirectedOperationToolDiameter = FetchDirectedOperationToolDiameter(ncBlocks);
        }

        private SubNCProgramCode(
            Ulid id,
            NCProgramType mainProgramClassification,
            string programName,
            IEnumerable<NCBlock?> ncBlocks)
            : base(id ,mainProgramClassification, programName, ncBlocks)
        {
            DirectedOperationClassification = FetchDirectedOperationType(ncBlocks);
            DirectedOperationToolDiameter = FetchDirectedOperationToolDiameter(ncBlocks);
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public static SubNCProgramCode Parse(NCProgramCode ncProgramCode)
        {
            return new SubNCProgramCode(
                ncProgramCode.ID,
                ncProgramCode.MainProgramClassification,
                ncProgramCode.ProgramName,
                ncProgramCode.NCBlocks);
        }

        public static new SubNCProgramCode ReConstruct(
            string id,
            NCProgramType mainProgramClassification,
            string programName,
            IEnumerable<NCBlock?> ncBlocks)
            => new(Ulid.Parse(id), mainProgramClassification, programName, ncBlocks);

        /// <summary>
        /// 作業指示を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DirectedOperationNotFoundException"></exception>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        [Logging]
        private static DirectedOperationType FetchDirectedOperationType(IEnumerable<NCBlock?> ncBlocks)
        {
            // 作業指示を探す
            IEnumerable<DirectedOperationType> hasOperationType = ncBlocks
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
                throw new DirectedOperationNotFoundException("作業指示が見つかりません");

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
        /// <exception cref="DirectedOperationToolDiameterNotFoundException"></exception>
        /// <exception cref="NCProgramConcatenationServiceException"></exception>
        [Logging]
        public static decimal FetchDirectedOperationToolDiameter(IEnumerable<NCBlock?> ncBlocks)
        {
            // 作業指示を探す
            IEnumerable<decimal> hasOperationType = ncBlocks
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
                throw new DirectedOperationToolDiameterNotFoundException("ツール径が見つかりません");

            if (hasOperationType.Count(x => x != decimal.MinValue) > 1)
            {
                // 有効な指示が複数ある場合
                string msg = $"作業指示が{hasOperationType.Count(x => x != decimal.MinValue)}件あります\n" +
                    $"サブプログラムを確認して、作業指示は1件にしてください";
                throw new NCProgramConcatenationServiceException(msg);
            }

            return hasOperationType.First(x => x != decimal.MinValue);
        }

        public DirectedOperationType DirectedOperationClassification { get; init; }

        public decimal DirectedOperationToolDiameter { get; init; }
    }
}
