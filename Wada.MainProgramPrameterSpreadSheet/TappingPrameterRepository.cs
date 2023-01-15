using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet
{
    public class TappingPrameterRepository : IMainProgramPrameterRepository
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
        private static TappingProgramPrameter FetchParameter(IXLRangeRow row, IXLWorksheet paramSheet)
        {
            [Logging]
            T GetValueWithVaridate<T>(string columnLetter, string columnHedder)
            {
                if (!row.Cell(columnLetter).TryGetValue(out T cellValue))
                    throw new NCProgramConcatenationServiceException(
                        $"{columnHedder}が取得できません" +
                        $" シート: {paramSheet.Name}," +
                        $" セル: {row.Cell(columnLetter).Address}");
                return cellValue;
            }

            var reamerDiameter = GetValueWithVaridate<string>("A", "タップ径");
            var preparedHoleDiameter = GetValueWithVaridate<double>("B", "DR1(φ)");
            var centerDrillDepth = GetValueWithVaridate<double>("C", "C/D深さ");
            var chamferingDepth = GetValueWithVaridate<double>("D", "面取深さ");
            var spinForAluminum = GetValueWithVaridate<double>("E", "回転(AL)");
            var feedForAluminum = GetValueWithVaridate<double>("F", "送り(AL)");
            var spinForIron = GetValueWithVaridate<double>("G", "回転(SS400)");
            var feedForIron = GetValueWithVaridate<double>("H", "送り(SS400)");

            return new TappingProgramPrameter(
                reamerDiameter,
                preparedHoleDiameter,
                centerDrillDepth,
                chamferingDepth,
                spinForAluminum,
                feedForAluminum,
                spinForIron,
                feedForIron);
        }
    }
}
