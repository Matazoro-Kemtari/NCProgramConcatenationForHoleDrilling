using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

/// <summary>
/// リーマパラメータ
/// </summary>
/// <param name="DiameterKey">リーマー径</param>
/// <param name="PilotHoleDiameter">下穴1径</param>
/// <param name="SecondaryPilotHoleDiameter">下穴2径</param>
/// <param name="CenterDrillDepth">C/D深さ</param>
/// <param name="ChamferingDepth">面取り深さ</param>
public record class ReamingProgramParameter(
    string DiameterKey,
    decimal PilotHoleDiameter,
    decimal SecondaryPilotHoleDiameter,
    decimal CenterDrillDepth,
    decimal? ChamferingDepth) : IPilotHoleDrilledParameter
{
    [Logging]
    private static decimal Validate(string value) => decimal.Parse(value);

    public decimal DirectedOperationToolDiameter => Validate(DiameterKey);

    public decimal DrillTipLength => 5m;

    /// <summary>
    /// 下穴1のドリル先端の長さ
    /// </summary>
    public DrillTipLength PilotHoleDrillTipLength => new(PilotHoleDiameter);

    /// <summary>
    /// 下穴2のドリル先端の長さ
    /// </summary>
    public DrillTipLength SecondaryPilotHoleDrillTipLength => new(SecondaryPilotHoleDiameter);
}

public class TestReamingProgramParameterFactory
{
    public static ReamingProgramParameter Create(
        string DiameterKey = "13.3",
        decimal PilotHoleDiameter = 9.1m,
        decimal SecondaryPilotHoleDiameter = 11.1m,
        decimal CenterDrillDepth = -3.1m,
        decimal? ChamferingDepth = -1.7m) => new(DiameterKey, PilotHoleDiameter, SecondaryPilotHoleDiameter, CenterDrillDepth, ChamferingDepth);
}
