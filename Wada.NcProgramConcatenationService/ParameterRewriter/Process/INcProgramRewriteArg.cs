using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal interface INcProgramRewriteArg
{
    MaterialType Material { get; init; }
    NcProgramCode RewritableCode { get; init; }
    IMainProgramParameter RewritingParameter { get; init; }
    string SubProgramNumber { get; init; }
}
