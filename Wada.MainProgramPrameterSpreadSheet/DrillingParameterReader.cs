using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet;

public class DrillingParameterReader : IMainProgramPrameterReader
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
                    .Select(async row => await FetchParameterAsync(row, paramSheet)));

        return parameters.ToList();
    }

    [Logging]
    private static async Task<DrillingProgramPrameter> FetchParameterAsync(IXLRangeRow row, IXLWorksheet paramSheet)
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

        var drillDiameter = await GetValueWithVaridateAsync<string>("A", "DR(φ)");
        var centerDrillDepth = await GetValueWithVaridateAsync<decimal>("B", "C/D深さ");
        var cutDepth = await GetValueWithVaridateAsync<decimal>("E", "切込(Q)");
        var spinForAluminum = await GetValueWithVaridateAsync<int>("F", "回転(AL)");
        var feedForAluminum = await GetValueWithVaridateAsync<int>("G", "送り(AL)");
        var spinForIron = await GetValueWithVaridateAsync<int>("H", "回転(SS400)");
        var feedForIron = await GetValueWithVaridateAsync<int>("I", "送り(SS400)");

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
