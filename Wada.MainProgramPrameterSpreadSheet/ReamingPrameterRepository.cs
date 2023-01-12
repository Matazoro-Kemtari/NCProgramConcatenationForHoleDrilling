using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet
{
    public class ReamingPrameterRepository : IReamingPrameterRepository
    {
        [Logging]
        public IEnumerable<ReamingProgramPrameter> ReadAll(Stream stream)
        {
            using var xlBook = new XLWorkbook(stream);
            // パラメーターのシートを取得 シートは1つの想定
            IXLWorksheet paramSheet = xlBook.Worksheets.First();

            // テーブル形式で一括読み込み
            var paramTbl = paramSheet.RangeUsed().AsTable();

            return paramTbl.Rows().Skip(1)
                .Select(row => FetchParameter(row, paramSheet))
                .ToList();
        }

        [Logging]
        private static ReamingProgramPrameter FetchParameter(IXLRangeRow row, IXLWorksheet paramSheet)
        {
            [Logging]
            T GetValueWithVaridate<T>(string columnLetter, string columnHedder)
            {
                if (!row.Cell(columnLetter).TryGetValue(out T cellValue)
                    || !double.TryParse(cellValue?.ToString(), out _))
                    throw new NCProgramConcatenationServiceException(
                        $"{columnHedder}が取得できません" +
                        $" シート: {paramSheet.Name}," +
                        $" セル: {row.Cell(columnLetter).Address}");
                return cellValue;
            }

            var reamerDiameter = GetValueWithVaridate<string>("A", "リーマ径");
            var preparedHoleDiameter = GetValueWithVaridate<double>("B", "DR1(φ)");
            var secondPreparedHoleDiameter = GetValueWithVaridate<double>("C", "DR2(φ)");
            var centerDrillDepth = GetValueWithVaridate<double>("D", "C/D深さ");
            var chamferingDepth = GetValueWithVaridate<double>("E", "面取深さ");

            return new ReamingProgramPrameter(
                reamerDiameter,
                preparedHoleDiameter,
                secondPreparedHoleDiameter,
                centerDrillDepth,
                chamferingDepth);
        }
    }
}