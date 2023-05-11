using System.Text.RegularExpressions;
using System;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

public record class DrillSizeData
{
    private DrillSizeData(string sizeIdentifier, double inch, double millimeter)
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

    public static DrillSizeData Create(string sizeIdentifier, double inch, double millimeter)
        => new(sizeIdentifier, inch, millimeter);

    public string SizeIdentifier { get; init; }
    public double Inch { get; init; }
    public double Millimeter { get; init; }
}

public class TestDrillSizeDataFactory
{
    public static DrillSizeData Create(
        string sizeIdentifier = "#Q",
        double inch = 0.332d,
        double millimeter = 8.43d)
        => DrillSizeData.Create(sizeIdentifier, inch, millimeter);
}
