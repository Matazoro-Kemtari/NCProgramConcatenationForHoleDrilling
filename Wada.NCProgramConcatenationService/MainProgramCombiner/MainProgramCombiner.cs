using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.MainProgramCombiner
{
    public interface IMainProgramCombiner
    {
        NCProgramCode Combine(IEnumerable<NCProgramCode> combinableCode, string machineToolName, string materialName);
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
        public NCProgramCode Combine(IEnumerable<NCProgramCode> combinableCode, string machineToolName, string materialName)
        {
            var combinedBlocks = combinableCode.Select(
                (x, i) =>
                {
                    var blocks = x.NCBlocks.ToList();
                    if (i < combinableCode.Count() - 1)
                        blocks.Add(null);
                    return blocks;
                })
                .SelectMany(x => x)
                .Prepend(new NCBlock(
                    new List<INCWord>
                    {
                        new NCComment($"{machineToolName}-{materialName}")
                    },
                    OptionalBlockSkip.None));

            return new(
                NCProgramType.CombinedProgram,
                string.Join('>', combinableCode.Select(x => x.ProgramName)),
                combinedBlocks);
        }
    }
}
