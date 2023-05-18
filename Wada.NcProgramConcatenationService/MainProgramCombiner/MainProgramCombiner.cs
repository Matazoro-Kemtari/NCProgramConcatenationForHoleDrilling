using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService.MainProgramCombiner
{
    public interface IMainProgramCombiner
    {
        NcProgramCode Combine(IEnumerable<NcProgramCode> combinableCode, string machineToolName, string materialName);
    }

    public class MainProgramCombiner : IMainProgramCombiner
    {
        /// <summary>
        /// メインプログラムを結合する
        /// </summary>
        /// <param name="combinableCode"></param>
        /// <param name="machineToolName"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
        public NcProgramCode Combine(IEnumerable<NcProgramCode> combinableCode, string machineToolName, string materialName)
        {
            var combinedBlocks = combinableCode.Select(
                (x, i) =>
                {
                    var blocks = x.NcBlocks.ToList();
                    if (i < combinableCode.Count() - 1)
                        blocks.Add(null);
                    return blocks;
                })
                .SelectMany(x => x)
                .Prepend(new NcBlock(
                    new List<INcWord>
                    {
                        new NcComment($"{machineToolName}-{materialName}")
                    },
                    OptionalBlockSkip.None));

            return new(
                NcProgramType.CombinedProgram,
                string.Join('>', combinableCode.Select(x => x.ProgramName)),
                combinedBlocks);
        }
    }
}
