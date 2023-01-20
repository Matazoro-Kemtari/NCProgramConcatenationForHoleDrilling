using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService.ParameterRewriter
{
    public class DrillingParameterRewriter : IMainProgramParameterRewriter
    {
        public IEnumerable<NCProgramCode> RewriteByTool(Dictionary<MainProgramType, NCProgramCode> rewritableCodes, MaterialType material, decimal thickness, decimal targetToolDiameter, MainProgramParametersRecord prameters)
        {
            throw new NotImplementedException();
        }
    }
}
