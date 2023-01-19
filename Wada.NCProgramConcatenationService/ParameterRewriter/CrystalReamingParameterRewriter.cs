namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    /// <summary>
    /// クリスタルリーマのパラメータを書き換える
    /// </summary>
    public class CrystalReamingParameterRewriter : ReamingParameterRewriterBase, IMainProgramParameterRewriter
    {
        public CrystalReamingParameterRewriter()
            : base(ParameterType.CrystalReamerParameter, ReamerType.CrystalReamerParameter)
        { }
    }
}
