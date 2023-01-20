namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    /// <summary>
    /// スキルリーマのパラメータを書き換える
    /// </summary>
    public class SkillReamingParameterRewriter : ReamingParameterRewriterBase, IMainProgramParameterRewriter
    {
        public SkillReamingParameterRewriter()
          : base(ParameterType.SkillReamerParameter, ReamerType.SkillReamerParameter)
        { }
    }
}
