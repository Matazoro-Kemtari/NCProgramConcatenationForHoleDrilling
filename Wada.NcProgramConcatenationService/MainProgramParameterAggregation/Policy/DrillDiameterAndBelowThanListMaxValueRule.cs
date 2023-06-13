namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class DrillDiameterAndBelowThanListMaxValueRule : IUsingParameterListRule
{
    private const decimal diameterMargin = 0.5m;
    /// <summary>
    /// ドリル径がリスト最大値以下か
    /// </summary>
    /// <param name="mainProgramParameters"></param>
    /// <param name="toolDiameter"></param>
    /// <returns></returns>
    public bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => mainProgramParameters.Select(x => x.DirectedOperationToolDiameter + diameterMargin)
                                .Any(x => x >= toolDiameter);
}
