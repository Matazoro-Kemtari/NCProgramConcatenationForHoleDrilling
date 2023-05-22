namespace Wada.NcProgramConcatenationService.ParameterRewriter
{
    /// <summary>
    /// クリスタルリーマのパラメータを書き換える
    /// </summary>
    public class CrystalReamingSequenceBuilder : ReamingSequenceBuilderBase, IMainProgramSequenceBuilder
    {
        public CrystalReamingSequenceBuilder()
            : base(ParameterType.CrystalReamerParameter, ReamerType.CrystalReamerParameter)
        { }
    }
}
