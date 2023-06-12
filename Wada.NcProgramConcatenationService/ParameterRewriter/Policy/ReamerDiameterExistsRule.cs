using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Policy;

internal class ReamerDiameterExistsRule : IUsingParameterListRule
{
    public bool Ok(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => mainProgramParameters.Any(x => x.DirectedOperationToolDiameter == toolDiameter);
}
