using System.Collections.Generic;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.ParameterRewriter;

public interface IMainProgramSequenceBuilder
{
    /// <summary>
    /// メインプログラムのパラメータを書き換える
    /// </summary>
    /// <param name="rewriteByToolArg"></param>
    /// <returns></returns>
    IEnumerable<NcProgramCode> RewriteByTool(RewriteByToolArg rewriteByToolArg);
}
