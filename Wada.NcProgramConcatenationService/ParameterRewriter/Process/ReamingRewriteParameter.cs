using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal record class ReamingRewriteParameter(
    NcProgramCode RewritableCode,
    MaterialType Material,
    ReamerType Reamer,
    decimal ReamingDepth, 
    IMainProgramParameter RewritingParameter,
    string SubProgramNumber) : INcProgramRewriteParameter;
