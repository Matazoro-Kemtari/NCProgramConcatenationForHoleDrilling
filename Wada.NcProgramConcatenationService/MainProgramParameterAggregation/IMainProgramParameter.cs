using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

public interface IMainProgramParameter
{
    /// <summary>
    /// パラメータが使えるかどうかを判定する
    /// </summary>
    /// <param name="diameter"></param>
    /// <returns>責任があるときtrue</returns>
    bool CanUse(decimal diameter);

    /// <summary>
    /// ツール径キー
    /// </summary>
    string DiameterKey { get; }

    /// <summary>
    /// ツール径
    /// </summary>
    decimal DirectedOperationToolDiameter { get; }

    /// <summary>
    /// C/D深さ
    /// </summary>
    decimal CenterDrillDepth { get; }

    /// <summary>
    /// 面取り深さ
    /// </summary>
    decimal? ChamferingDepth { get; }


    /// <summary>
    /// ドリル先端の長さ
    /// </summary>
    decimal DrillTipLength { get; }
}

/// <summary>
/// 下穴工程のあるパラメーター
/// </summary>
public interface IPilotHoleDrilledParameter : IMainProgramParameter
{
    /// <summary>
    /// 下穴径
    /// </summary>
    decimal PilotHoleDiameter { get; }

    /// <summary>
    /// 下穴のドリル先端の長さ
    /// </summary>
    DrillTipLength PilotHoleDrillTipLength { get; }
}
