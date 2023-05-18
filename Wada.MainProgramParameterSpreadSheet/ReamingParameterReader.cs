using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramParameterSpreadSheet;

public class ReamingParameterReader : IMainProgramParameterReader
{
    [Logging]
    public virtual async Task<IEnumerable<IMainProgramPrameter>> ReadAllAsync(Stream stream)
    {
        using var xlBook = new XLWorkbook(stream);
        // パラメーターのシートを取得 シートは1つの想定
        IXLWorksheet paramSheet = xlBook.Worksheets.First();

        // テーブル形式で一括読み込み
        var paramTbl = paramSheet.RangeUsed().AsTable();

        var parameters = await Task.WhenAll(
            paramTbl.Rows()
                    .Skip(1)
                    .Select(row => FetchParameterAsync(row, paramSheet)));
                   
        return parameters.ToList();
    }

    [Logging]
    private static async Task<ReamingProgramPrameter> FetchParameterAsync(IXLRangeRow row, IXLWorksheet paramSheet)
    {
        [Logging]
        Task<T> GetValueWithVaridateAsync<T>(string columnLetter, string columnHedder) => Task.Run(
            () =>
            {
                if (!row.Cell(columnLetter).TryGetValue(out T cellValue)
                    || !decimal.TryParse(cellValue?.ToString(), out _))
                    throw new MainProgramParameterException(
                        $"{columnHedder}が取得できません" +
                        $" シート: {paramSheet.Name}," +
                        $" セル: {row.Cell(columnLetter).Address}");
                return cellValue;
            });

        [Logging]
        Task<T?> GetValueWithOutVaridateAsync<T>(string columnLetter, string columnHedder) => Task.Run(
            () =>
            {
                if (!row.Cell(columnLetter).TryGetValue(out T cellValue)
                    || !decimal.TryParse(cellValue?.ToString(), out _))
                    return default(T);
                return cellValue;
            });

        var reamerDiameter = await GetValueWithVaridateAsync<string>("A", "リーマ径");
        var preparedHoleDiameter = await GetValueWithVaridateAsync<decimal>("B", "DR1(φ)");
        var secondPreparedHoleDiameter = await GetValueWithVaridateAsync<decimal>("C", "DR2(φ)");
        var centerDrillDepth = await GetValueWithVaridateAsync<decimal>("D", "C/D深さ");
        var chamferingDepth = await GetValueWithOutVaridateAsync<decimal?>("E", "面取深さ");

        return new ReamingProgramPrameter(
            reamerDiameter,
            preparedHoleDiameter,
            secondPreparedHoleDiameter,
            centerDrillDepth,
            chamferingDepth);
    }
}