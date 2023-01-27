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
            var combinedBlocks = combinableCode.Select(
                (x, i) =>
                {
                    var blocks = x.NCBlocks.ToList();
                    if (i < combinableCode.Count() - 1)
                        blocks.Add(null);
                    return blocks;
                })
                .SelectMany(x => x);
            return new(
                NCProgramType.CombinedProgram,
                string.Join('>', combinableCode.Select(x => x.ProgramName)),
                combinedBlocks);
        }
    }
}
