namespace Wada.NcProgramConcatenationService.MainProgramParameterAggregation.Policy;

internal class ToolParameterPolicy
{
    private readonly IList<IUsingParameterListRule> usingParameterListRules = new List<IUsingParameterListRule>();

    public void Add(IUsingParameterListRule rule) => usingParameterListRules.Add(rule);

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => usingParameterListRules.Select(x => x.Ok(mainProgramParameters, toolDiameter)).All(x => x);
}
