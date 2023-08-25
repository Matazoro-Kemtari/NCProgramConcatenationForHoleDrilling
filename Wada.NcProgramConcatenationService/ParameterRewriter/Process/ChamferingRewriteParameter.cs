using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal record class ChamferingRewriteParameter(
    NcProgramCode RewritableCode,
    MaterialType Material,
    IMainProgramParameter RewritingParameter,
    string SubProgramNumber) : INcProgramRewriteParameter;
