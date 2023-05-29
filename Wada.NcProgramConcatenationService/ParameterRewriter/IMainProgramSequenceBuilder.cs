using Wada.Extensions;
using Wada.NcProgramConcatenationService.NcProgramAggregation;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public interface IMainProgramSequenceBuilder
{
    /// <summary>
    /// メインプログラムのパラメータを書き換える
    /// </summary>
    /// <param name="rewriteByToolArg"></param>
    /// <returns></returns>
    Task<IEnumerable<NcProgramCode>> RewriteByToolAsync(ToolParameter rewriteByToolArg);
}

public enum RewriterSelector
{
    [EnumDisplayName("タップ")]
    Tapping,

    [EnumDisplayName("クリスタルリーマー")]
    CrystalReaming,

    [EnumDisplayName("スキルリーマー")]
    SkillReaming,

    [EnumDisplayName("ドリル")]
    Drilling,
}