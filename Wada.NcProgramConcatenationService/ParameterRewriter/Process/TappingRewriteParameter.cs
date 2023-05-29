using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal record class TappingRewriteParameter(
    NcProgramCode RewritableCode,
    MaterialType Material,
    decimal TappingDepth,
    IMainProgramParameter RewritingParameter,
    string SubProgramNumber) : INcProgramRewriteParameter;
