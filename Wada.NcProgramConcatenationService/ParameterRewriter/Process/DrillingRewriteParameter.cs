using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter.Process;

internal record class DrillingRewriteParameter(
    NcProgramCode RewritableCode,
    MaterialType Material,
    decimal DrillingDepth,
    IMainProgramParameter RewritingParameter,
    string SubProgramNumber,
    decimal DrillDiameter) : INcProgramRewriteParameter;
