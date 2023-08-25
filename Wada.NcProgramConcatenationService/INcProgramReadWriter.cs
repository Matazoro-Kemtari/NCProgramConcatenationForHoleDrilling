using Wada.NcProgramConcatenationService.NcProgramAggregation;

namespace Wada.NcProgramConcatenationService;

public interface INcProgramReadWriter
{
    Task<NcProgramCode> ReadAllAsync(StreamReader reader, ReadNcProgramType ncProgram, string programName);

    Task<NcProgramCode> ReadSubProgramAll(StreamReader reader, string programName);

    Task WriteAllAsync(StreamWriter writer, string ncProgramCode);
}

/// <summary>
/// 読み込み対象のNCプログラム種類
/// </summary>
public enum ReadNcProgramType
{
    /// <summary>
    /// センタードリル
    /// </summary>
    CenterDrilling,
    /// <summary>
    /// ドリル
    /// </summary>
    Drilling,
    /// <summary>
    /// 面取り
    /// </summary>
    Chamfering,
    /// <summary>
    /// リーマー
    /// </summary>
    Reaming,
    /// <summary>
    /// タップ
    /// </summary>
    Tapping,

    /// <summary>
    /// サブプログラム
    /// </summary>
    SubProgram = int.MaxValue - 1,
}
