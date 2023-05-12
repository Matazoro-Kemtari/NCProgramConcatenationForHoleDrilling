using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.NCProgramAggregation
{
    /// <summary>
    /// 作業指示者
    /// </summary>
    public record class OperationDirecter
    {
        private readonly IEnumerable<DrillSizeData> _drillSizeData;

        private OperationDirecter(NcProgramCode subNcProgramCode, IEnumerable<DrillSizeData> drillSizeData)
        {
            SubNcProgramCode = subNcProgramCode ?? throw new ArgumentNullException(nameof(subNcProgramCode));
            _drillSizeData = drillSizeData ?? throw new ArgumentNullException(nameof(drillSizeData));

            DirectedOperationClassification = FetchDirectedOperationType();
            DirectedOperationToolDiameter = FetchDirectedOperationToolDiameter();
        }

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="ncProgramCode">NCプログラム</param>
        /// <param name="drillSizeData">インチミリ変換表</param>
        /// <returns></returns>
        public static OperationDirecter Create(NcProgramCode ncProgramCode, IEnumerable<DrillSizeData> drillSizeData)
            => new(ncProgramCode, drillSizeData);

        public static OperationDirecter ReConstruct(NcProgramCode subNcProgramCode, IEnumerable<DrillSizeData> drillSizeData)
            => new(subNcProgramCode, drillSizeData);

        /// <summary>
        /// 作業指示を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DirectedOperationNotFoundException"></exception>
        /// <exception cref="DomainException"></exception>
        [Logging]
        private DirectedOperationType FetchDirectedOperationType()
        {
            // 作業指示を探す
            IEnumerable<DirectedOperationType> hasOperationType = SubNcProgramCode.NcBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NcComment))
                .Select(w =>
                {
                    // (3-M12)これに一致するか
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)M\d+"))
                        return DirectedOperationType.Tapping;

                    // (2-D10H7)
                    // (2-D10G7)
                    // (2-3/16 P.H)
                    // (2-#C P.H)
                    // (2-#99 P.H)
                    // これらに一致するか
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)((D\d+(\.?\d+)?[HG]\d+)|\d{1,2}\/\d{1,2}\sP\.H|#[A-Z]\sP\.H|#\d{1,2}\sP\.H)"))
                        return DirectedOperationType.Reaming;

                    // (4-D10DR)これに一致するか
                    if (Regex.IsMatch(w.ToString()!, @"(?<=-)D\d+(\.?\d+)?DR"))
                        return DirectedOperationType.Drilling;

                    return DirectedOperationType.Undetected;
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
                throw new DomainException(msg);
            }

            return hasOperationType.First(x => x != DirectedOperationType.Undetected);
        }

        /// <summary>
        /// ツール径を取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DirectedOperationToolDiameterNotFoundException"></exception>
        /// <exception cref="DomainException"></exception>
        /// <exception cref="DrillSizeDataException"></exception>
        [Logging]
        public decimal FetchDirectedOperationToolDiameter()
        {
            // 作業指示を探す
            IEnumerable<decimal> hasOperationType = SubNcProgramCode.NcBlocks
                .Where(x => x != null)
                .Select(block => block!.NCWords
                .Where(w => w.GetType() == typeof(NcComment))
                .Select(w =>
                {
                    var tapMatch = Regex.Match(w.ToString()!, @"(?<=-M)\d+(\.\d+)?");
                    if (tapMatch.Success)
                        return decimal.Parse(tapMatch.Value);

                    var reamerMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=[HG]\d+)");
                    if (reamerMatch.Success)
                        return decimal.Parse(reamerMatch.Value);

                    var reamerInchMatch = Regex.Match(w.ToString()!, @"(?<=-)(\d{1,2}\/\d{1,2}|#[A-Z]|#\d{1,2})(?=\sP\.H)");
                    if (reamerInchMatch.Success)
                    {
                        try
                        {
                            return ConvertInchToMillimeter(reamerInchMatch.Value);
                        }
                        catch (InvalidOperationException ex)
                        {
                            throw new DrillSizeDataException(
                                $"インチリストに該当がありません インチ: {reamerInchMatch.Value}",ex);
                        }
                    }

                    var drillMatch = Regex.Match(w.ToString()!, @"(?<=-D)\d+(\.\d+)?(?=DR)");
                    if (drillMatch.Success)
                        return decimal.Parse(drillMatch.Value);

                    return decimal.MinValue;
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
                throw new DomainException(msg);
            }

            return hasOperationType.First(x => x != decimal.MinValue);
        }

        private decimal ConvertInchToMillimeter(string inchValue)
        {
            var drillSizeData = _drillSizeData.Where(x => x.SizeIdentifier == inchValue).Single();
            return (decimal)drillSizeData.Millimeter;
        }

        public NcProgramCode SubNcProgramCode { get; init; }

        public DirectedOperationType DirectedOperationClassification { get; init; }

        public decimal DirectedOperationToolDiameter { get; init; }
    }
}
