namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

public class DrillingParameterExistencePolicy
{
    private readonly ToolParameterPolicy _policy;

    public DrillingParameterExistencePolicy()
    {
        _policy = new();
        _policy.Add(new DrillDiameterAndOverThanListMinValueRule());
        _policy.Add(new DrillDiameterAndBelowThanListMaxValueRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}
