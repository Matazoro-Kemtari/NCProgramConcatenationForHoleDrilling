using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService.MainProgramCombiner
{
    public interface IMainProgramCombiner
    {
        NCProgramCode Combine(IEnumerable<NCProgramCode> combinableCode);
    }

    public class MainProgramCombiner : IMainProgramCombiner
    {
        public NCProgramCode Combine(IEnumerable<NCProgramCode> combinableCode)
        {
            throw new NotImplementedException();
        }
    }
}
