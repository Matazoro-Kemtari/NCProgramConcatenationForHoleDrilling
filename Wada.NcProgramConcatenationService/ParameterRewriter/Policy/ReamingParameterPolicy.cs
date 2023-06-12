using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Policy;

internal class ReamingParameterPolicy
{
    private readonly ToolParameterPolicy _policy;

    public ReamingParameterPolicy()
    {
        _policy = new ToolParameterPolicy();
        _policy.Add(new ReamerDiameterExistsRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}