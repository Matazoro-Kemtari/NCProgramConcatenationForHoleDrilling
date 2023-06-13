using System.Text.RegularExpressions;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

/// <summary>
/// タップパラメータ
/// </summary>
/// <param name="DiameterKey">タップ径</param>
/// <param name="PilotHoleDiameter">下穴径</param>
/// <param name="CenterDrillDepth">C/D深さ</param>
/// <param name="ChamferingDepth">面取り深さ</param>
/// <param name="SpinForAluminum">回転(アルミ)</param>
/// <param name="FeedForAluminum">送り(アルミ)</param>
/// <param name="SpinForIron">回転(SS400)</param>
/// <param name="FeedForIron">送り(SS400)</param>
public record class TappingProgramParameter(
    string DiameterKey,
    decimal PilotHoleDiameter,
    decimal CenterDrillDepth,
    decimal? ChamferingDepth,
    int SpinForAluminum,
    int FeedForAluminum,
    int SpinForIron,
    int FeedForIron) : IPilotHoleDrilledParameter
{
    [Logging]
    public bool CanUse(decimal diameter)
        => DirectedOperationToolDiameter == diameter;

    [Logging]
    private static decimal Validate(string value)
    {
        var matchedDiameter = Regex.Match(value, @"(?<=M)\d+(\.\d)?");
        if (!matchedDiameter.Success)
            throw new DomainException(
                "タップ径の値が読み取れません\n" +
                $"書式を確認してください タップ径: {value}");

        return decimal.Parse(matchedDiameter.Value);
    }

    public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

    public decimal DrillTipLength => 5m;

    /// <summary>
    /// 下穴のドリル先端の長さ
    /// </summary>
    public DrillTipLength PilotHoleDrillTipLength => new(PilotHoleDiameter);
}

public class TestTappingProgramParameterFactory
{
    public static TappingProgramParameter Create(
        string DiameterKey = "M13.3",
        decimal PilotHoleDiameter = 11.1m,
        decimal CenterDrillDepth = -3.1m,
        decimal? ChamferingDepth = -1.7m,
        int SpinForAluminum = 700,
        int FeedForAluminum = 300,
        int SpinForIron = 700,
        int FeedForIron = 300) => new(DiameterKey, PilotHoleDiameter, CenterDrillDepth, ChamferingDepth, SpinForAluminum, FeedForAluminum, SpinForIron, FeedForIron);
}
