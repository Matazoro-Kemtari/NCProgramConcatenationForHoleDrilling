using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

/// <summary>
/// ドリルパラメータ
/// </summary>
/// <param name="DiameterKey">ドリル径</param>
/// <param name="CenterDrillDepth">C/D深さ</param>
/// <param name="CutDepth">切込量</param>
/// <param name="SpinForAluminum">回転(アルミ)</param>
/// <param name="FeedForAluminum">送り(アルミ)</param>
/// <param name="SpinForIron">回転(SS400)</param>
/// <param name="FeedForIron">送り(SS400)</param>
public record class DrillingProgramParameter(
    string DiameterKey,
    decimal CenterDrillDepth,
    decimal CutDepth,
    int SpinForAluminum,
    int FeedForAluminum,
    int SpinForIron,
    int FeedForIron) : IMainProgramParameter
{
    private const decimal diameterMargin = 0.5m;

    [Logging]
    public bool CanUse(decimal diameter)
        => DirectedOperationToolDiameter <= diameter
           && diameter < (DirectedOperationToolDiameter + diameterMargin);

    [Logging]
    private static decimal Validate(string value) => decimal.Parse(value);

    [Logging]
    private static decimal CalcChamferingDepth(decimal diameter) => -(diameter / 2m + 0.2m);

    public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

    public decimal? ChamferingDepth => CalcChamferingDepth(DirectedOperationToolDiameter);

    public decimal DrillTipLength => new DrillTipLength(DirectedOperationToolDiameter).Value;
}

public class TestDrillingProgramParameterFactory
{
    public static DrillingProgramParameter Create(
    string DiameterKey = "13.3",
    decimal CenterDrillDepth = -1.5m,
    decimal CutDepth = 3.5m,
    int SpinForAluminum = 740,
    int FeedForAluminum = 100,
    int SpinForIron = 490,
    int FeedForIron = 70) => new(DiameterKey, CenterDrillDepth, CutDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
}
