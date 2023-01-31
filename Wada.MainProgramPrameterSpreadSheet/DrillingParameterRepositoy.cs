using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet
{
    public class DrillingParameterRepositoy : IMainProgramPrameterRepository
    {
        [Logging]
        public virtual IEnumerable<IMainProgramPrameter> ReadAll(Stream stream)
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
        private static DrillingProgramPrameter FetchParameter(IXLRangeRow row, IXLWorksheet paramSheet)
        {
            [Logging]
            T GetValueWithVaridate<T>(string columnLetter, string columnHedder)
            {
                if (!row.Cell(columnLetter).TryGetValue(out T cellValue)
                    || !decimal.TryParse(cellValue?.ToString(), out _))
                    throw new NCProgramConcatenationServiceException(
                        $"{columnHedder}が取得できません" +
                        $" シート: {paramSheet.Name}," +
                        $" セル: {row.Cell(columnLetter).Address}");
                return cellValue;
            }

            var drillDiameter = GetValueWithVaridate<string>("A", "DR(φ)");
            var centerDrillDepth = GetValueWithVaridate<decimal>("B", "C/D深さ");
            var cutDepth = GetValueWithVaridate<decimal>("E", "切込(Q)");
            var spinForAluminum = GetValueWithVaridate<int>("F", "回転(AL)");
            var feedForAluminum = GetValueWithVaridate<int>("G", "送り(AL)");
            var spinForIron = GetValueWithVaridate<int>("H", "回転(SS400)");
            var feedForIron = GetValueWithVaridate<int>("I", "送り(SS400)");

            return new DrillingProgramPrameter(
                drillDiameter,
                centerDrillDepth,
                cutDepth,
                spinForAluminum,
                feedForAluminum,
                spinForIron,
                feedForIron);
        }
    }
}
