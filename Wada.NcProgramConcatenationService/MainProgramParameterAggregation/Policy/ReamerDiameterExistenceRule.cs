namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class ReamerDiameterExistenceRule : IUsingParameterListRule
{
    public bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => mainProgramParameters.Any(x => x.DirectedOperationToolDiameter == toolDiameter);
}
