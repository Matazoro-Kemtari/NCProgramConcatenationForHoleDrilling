using ClosedXML.Excel;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NcProgramAggregation;

namespace Wada.InchMillimeterConvertSpreadSheet;

public class DrillSizeDataReader : IDrillSizeDataReader
{
    [Logging]
    public async Task<IEnumerable<DrillSizeData>> ReadAllAsync(Stream stream)
    {
        using var xlBook = new XLWorkbook(stream);
        var sheet = xlBook.Worksheets.First();

        // テーブル形式で一括読み込み
        var sizeTbl = sheet.RangeUsed().AsTable();

        // 左・中・右の各列からドリルサイズオブジェクトを作成する
        var drillSizes = await Task.WhenAll(
            ReadDrillSizesByColumn(0, sizeTbl),
            ReadDrillSizesByColumn(1, sizeTbl),
            ReadDrillSizesByColumn(2, sizeTbl));

        // 各リストを結合して一つのリストにする
        return drillSizes.SelectMany(x => x);
    }

    [Logging]
    private static async Task<IEnumerable<DrillSizeData>> ReadDrillSizesByColumn(int columnIndex, IXLTable sizeTbl)
    {
        var idIndex = 1 + (3 * columnIndex);
        var inchIndex = idIndex + 1;
        var millIndex = inchIndex + 1;
        return await Task.WhenAll(
            sizeTbl.Rows()
                // ヘッダ行を飛ばす
                .Skip(1)
                // 値がある行だけ選択する
                .Where(y => !y.Cell(idIndex).IsEmpty())
                .Select(async y => await Task.Run(() =>
                {
                    // 列から値を取得し、ドリルサイズオブジェクトを作成する
                    string sizeIdentifier = y.Cell(idIndex).GetString();

                    if (!y.Cell(inchIndex).TryGetValue(out decimal inch))
                        throw new DrillSizeDataException(
                            $"Inchesが取得できませんでした 行: {y.RowNumber()}, 列: {inchIndex}");

                    if (!y.Cell(millIndex).TryGetValue(out decimal millimeter))
                        throw new DrillSizeDataException(
                            $"ISO Metric drill size(㎜)が取得できませんでした 行: {y.RowNumber()}, 列: {inchIndex}");

                    try
                    {
                        return DrillSizeData.Create(sizeIdentifier, inch, millimeter);
                    }
                    catch(DrillSizeDataException ex)
                    {
                        throw new DrillSizeDataException($"{ex.Message}, 行: {y.RowNumber()}", ex);
                    }
                })));
    }
}
