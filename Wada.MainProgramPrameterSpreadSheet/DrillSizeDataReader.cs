using ClosedXML.Excel;
using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.MainProgramPrameterSpreadSheet;

public class DrillSizeDataReader : IDrillSizeDataReader
{
    [Logging]
    public async Task<IEnumerable<DrillSizeData>> ReadAllAsync(Stream stream)
    {
        using var xlBook = new XLWorkbook(stream);
        var sheet = xlBook.Worksheets.First();

        // テーブル形式で一括読み込み
        var sizeTbl = sheet.RangeUsed().AsTable();

        // 段組み：左・中・右と順番に処理する
        var drillSizes = await Task.WhenAll(
            Enumerable.Range(0, 3).Select(async x =>
            {
                return await Task.WhenAll(
                    sizeTbl.Rows().Skip(1)
                           // 値がある行だけ選択する
                           .Where(y => !y.Cell(1 + (3 * x)).IsEmpty())
                           .Select(async y => await Task.Run(() =>
                           {
                               // 列から値を取得し、ドリルサイズオブジェクトを作成する
                               string sizeIdentifier = y.Cell(1 + (3 * x)).GetString();

                               // 識別子の書式が合っているか確認する
                               if (!Regex.IsMatch(sizeIdentifier, @"(#(\d{1,2}|[A-Z])|\d{1,2}/\d{1,2})"))
                                   throw new DrillSizeDataException(
                                       $"識別子の値が不正です 値: {sizeIdentifier}, アドレス: {y.Cell(1 + (3 * x)).Address}");

                               if (!y.Cell(2 + (3 * x)).TryGetValue(out double inch))
                                   throw new DrillSizeDataException(
                                       $"Inchesが取得できませんでした 行: {y.RowNumber()}, 列: {2 + (3 * x)}");

                               if (inch <= 0)
                                   throw new DrillSizeDataException(
                                       $"Inchesの値が不正です 値: {inch}, アドレス: {y.Cell(2 + (3 * x)).Address}");

                               if (!y.Cell(3 + (3 * x)).TryGetValue(out double millimeter))
                                   throw new DrillSizeDataException(
                                       $"ISO Metric drill size(㎜)が取得できませんでした 行: {y.RowNumber()}, 列: {2 + (3 * x)}");

                               if (millimeter <= 0)
                                   throw new DrillSizeDataException(
                                       $"ISO Metric drill size(㎜)の値が不正です 値: {millimeter}, アドレス: {y.Cell(3 + (3 * x)).Address}");

                               return DrillSizeData.Create(sizeIdentifier, inch, millimeter);
                           })));

            }));
        // 一次元のコレクションに変換して返す
        return drillSizes.SelectMany(x => x);
    }
}
