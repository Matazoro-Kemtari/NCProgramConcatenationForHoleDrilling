using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

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
            var combinedBlocks = combinableCode.Select(x => x.NCBlocks)
                .SelectMany(x => x);
            return new(
                NCProgramType.CombinedProgram,
                string.Join('>', combinableCode.Select(x => x.ProgramName)),
                combinedBlocks);
        }
    }
}
