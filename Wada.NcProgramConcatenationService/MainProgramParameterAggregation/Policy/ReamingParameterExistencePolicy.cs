namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class ReamingParameterExistencePolicy
{
    private readonly ToolParameterPolicy _policy;

    public ReamingParameterExistencePolicy()
    {
        _policy = new ToolParameterPolicy();
        _policy.Add(new ReamerDiameterExistenceRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}