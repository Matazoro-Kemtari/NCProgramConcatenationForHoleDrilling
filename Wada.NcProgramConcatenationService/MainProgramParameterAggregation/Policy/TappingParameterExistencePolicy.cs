namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class TappingParameterExistencePolicy
{
    private readonly ToolParameterPolicy _policy;

    public TappingParameterExistencePolicy()
    {
        _policy = new ToolParameterPolicy();
        _policy.Add(new TapDiameterExistenceRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}