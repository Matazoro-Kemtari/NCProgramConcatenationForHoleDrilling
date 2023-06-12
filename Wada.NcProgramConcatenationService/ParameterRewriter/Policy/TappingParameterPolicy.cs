using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Policy;

internal class TappingParameterPolicy
{
    private readonly ToolParameterPolicy _policy;

    public TappingParameterPolicy()
    {
        _policy = new ToolParameterPolicy();
        _policy.Add(new TapDiameterExistsRule());
    }

    public bool ComplyWithAll(IEnumerable<IMainProgramParameter> mainProgramParameters, decimal toolDiameter)
        => _policy.ComplyWithAll(mainProgramParameters, toolDiameter);
}