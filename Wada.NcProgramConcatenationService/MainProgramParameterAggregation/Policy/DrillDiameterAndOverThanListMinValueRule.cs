namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class DrillDiameterAndOverThanListMinValueRule : IUsingParameterListRule
{
    /// <summary>
    /// ドリル径がリスト最大値以上か
    /// </summary>
    /// <param name="mainProgramParameters"></param>
    /// <param name="toolDiameter"></param>
    /// <returns></returns>
    public bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => mainProgramParameters.Any(x => x.DirectedOperationToolDiameter <= toolDiameter);
}
