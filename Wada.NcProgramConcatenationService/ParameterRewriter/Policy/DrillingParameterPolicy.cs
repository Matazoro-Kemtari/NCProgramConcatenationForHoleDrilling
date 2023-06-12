using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Policy;

public class DrillingParameterPolicy
{
    private readonly ToolParameterPolicy _policy;

    public DrillingParameterPolicy()
    {
        _policy = new ToolParameterPolicy();
        _policy.Add(new DrillDiameterAndOverThanListMinValueRule());
        _policy.Add(new DrillDiameterAndBelowThanListMaxValueRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}
