namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class TapDiameterExistenceRule : IUsingParameterListRule
{
    public bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => mainProgramParameters.Any(x => x.DirectedOperationToolDiameter == toolDiameter);
}
