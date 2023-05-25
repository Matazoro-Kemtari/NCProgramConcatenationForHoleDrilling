using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

/// <summary>
/// スキルリーマのパラメータを書き換える
/// </summary>
public class SkillReamingSequenceBuilder : ReamingSequenceBuilderBase, IMainProgramSequenceBuilder
{
    public SkillReamingSequenceBuilder()
      : base(ParameterType.SkillReamerParameter, ReamerType.SkillReamerParameter)
    { }
}
