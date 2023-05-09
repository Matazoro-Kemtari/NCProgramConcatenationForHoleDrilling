namespace Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

public record class DrillSizeData
{
    private DrillSizeData(string sizeIdentifier, double inch, double millimeter)
    {
        SizeIdentifier = sizeIdentifier ?? throw new ArgumentNullException(nameof(sizeIdentifier));
        Inch = inch;
        Millimeter = millimeter;
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
