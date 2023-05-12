using System.Text.RegularExpressions;

namespace Wada.NcProgramConcatenationService.NcProgramAggregation;

public record class DrillSizeData
{
    private DrillSizeData(string sizeIdentifier, decimal inch, decimal millimeter)
    {
        SizeIdentifier = sizeIdentifier ?? throw new ArgumentNullException(nameof(sizeIdentifier));
        Inch = inch;
        Millimeter = millimeter;

        // 識別子の書式が合っているか確認する
        if (!Regex.IsMatch(sizeIdentifier, @"(#(\d{1,2}|[A-Z])|\d{1,2}/\d{1,2})"))
            throw new DrillSizeDataException(
                $"識別子の値が不正です 値: {sizeIdentifier}");

        // 1以上か確認する
        if (inch <= 0)
            throw new DrillSizeDataException(
                $"Inchesの値が不正です 値: {inch}");

        if (millimeter <= 0)
            throw new DrillSizeDataException(
                $"ISO Metric drill size(㎜)の値が不正です 値: {millimeter}");

    }

    public static DrillSizeData Create(string sizeIdentifier, decimal inch, decimal millimeter)
        => new(sizeIdentifier, inch, millimeter);

    public string SizeIdentifier { get; init; }
    public decimal Inch { get; init; }
    public decimal Millimeter { get; init; }
}

public class TestDrillSizeDataFactory
{
    public static DrillSizeData Create(
        string sizeIdentifier = "#Q",
        decimal inch = 0.332m,
        decimal millimeter = 8.43m)
        => DrillSizeData.Create(sizeIdentifier, inch, millimeter);
}
