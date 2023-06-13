namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal interface IUsingParameterListRule
{
    /// <summary>
    /// パラメータリストを使用できるかを判定するルール
    /// </summary>
    /// <param name="mainProgramParameters">パラメータリスト</param>
    /// <param name="ToolDiameter">ツール径</param>
    /// <returns>条件を満たす場合true</returns>
    bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter);
}
