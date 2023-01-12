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

        private static ReamingProgramPrameter FetchParameter(IXLRangeRow row, IXLWorksheet paramSheet)
        {
            const string ReamerDiameterColumnLetter = "A";
            const string PreparedHoleDiameterColumnLetter = "B";
            const string SecondPreparedHoleDiameterColumnLetter = "C";
            const string CenterDrillDepthColumnLetter = "D";
            const string ChamferingDepthColumnLetter = "E";

            if (!row.Cell(ReamerDiameterColumnLetter).TryGetValue(out string reamerDiameter)
                || !double.TryParse(reamerDiameter, out _))
                throw new NCProgramConcatenationServiceException(
                    $"リーマ径が取得できません" +
                    $" シート: {paramSheet.Name}," +
                    $" セル: {row.Cell(ReamerDiameterColumnLetter).Address}");

            if (!row.Cell(PreparedHoleDiameterColumnLetter).TryGetValue(out double preparedHoleDiameter))
                throw new NCProgramConcatenationServiceException(
                    $"DR1(φ)が取得できません" +
                    $" シート: {paramSheet.Name}," +
                    $" セル: {row.Cell(PreparedHoleDiameterColumnLetter).Address}");

            if (!row.Cell(SecondPreparedHoleDiameterColumnLetter).TryGetValue(out double secondPreparedHoleDiameter))
                throw new NCProgramConcatenationServiceException(
                    $"DR2(φ)が取得できません" +
                    $" シート: {paramSheet.Name}," +
                    $" セル: {row.Cell(SecondPreparedHoleDiameterColumnLetter).Address}");

            if (!row.Cell(CenterDrillDepthColumnLetter).TryGetValue(out double centerDrillDepth))
                throw new NCProgramConcatenationServiceException(
                    $"C/D深さが取得できません" +
                    $" シート: {paramSheet.Name}," +
                    $" セル: {row.Cell(CenterDrillDepthColumnLetter).Address}");

            if (!row.Cell(ChamferingDepthColumnLetter).TryGetValue(out double? chamferingDepth))
                chamferingDepth = null;

            return new ReamingProgramPrameter(
                reamerDiameter,
                preparedHoleDiameter,
                secondPreparedHoleDiameter,
                centerDrillDepth,
                chamferingDepth);
        }
    }
}