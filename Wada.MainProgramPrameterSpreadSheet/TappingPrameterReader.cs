using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet
{
    public class TappingPrameterReader : IMainProgramPrameterReader
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
        private static async Task<TappingProgramPrameter> FetchParameterAsync(IXLRangeRow row, IXLWorksheet paramSheet)
        {
            [Logging]
            Task<T> GetValueWithVaridateAsync<T>(string columnLetter, string columnHedder) => Task.Run(
                () =>
                {
                    if (!row.Cell(columnLetter).TryGetValue(out T cellValue))
                        throw new MainProgramParameterException(
                            $"{columnHedder}が取得できません" +
                            $" シート: {paramSheet.Name}," +
                            $" セル: {row.Cell(columnLetter).Address}");
                    return cellValue;
                });

            var reamerDiameter = await GetValueWithVaridateAsync<string>("A", "タップ径");
            var preparedHoleDiameter = await GetValueWithVaridateAsync<decimal>("B", "DR1(φ)");
            var centerDrillDepth = await GetValueWithVaridateAsync<decimal>("C", "C/D深さ");
            var chamferingDepth = await GetValueWithVaridateAsync<decimal>("D", "面取深さ");
            var spinForAluminum = await GetValueWithVaridateAsync<int>("E", "回転(AL)");
            var feedForAluminum = await GetValueWithVaridateAsync<int>("F", "送り(AL)");
            var spinForIron = await GetValueWithVaridateAsync<int>("G", "回転(SS400)");
            var feedForIron = await GetValueWithVaridateAsync<int>("H", "送り(SS400)");

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
