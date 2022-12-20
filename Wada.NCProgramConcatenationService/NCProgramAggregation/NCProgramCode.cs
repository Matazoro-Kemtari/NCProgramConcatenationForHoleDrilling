using System.Text;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    public record class NCProgramCode
    {
        public NCProgramCode(string programName, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            ProgramName = programName;
            NCBlocks = ncBlocks;
        }

        public override string ToString()
        {
            var ncBlocksString = string.Join("\n", NCBlocks.Select(x => x?.ToString()));
            return $"%\n{ncBlocksString}\n%\n";
        }

        public Ulid ID { get; }

        /// <summary>
        /// プログラム番号
        /// </summary>
        public string ProgramName { get; init; }

        public IEnumerable<NCBlock?> NCBlocks { get; init; }
    }

    /// <summary>
    /// ブロック
    /// </summary>
    /// <param name="NCWords">ワード</param>
    public record class NCBlock(IEnumerable<INCWord> NCWords)
    {
        public override string ToString()
        {
            StringBuilder buf = new();
            NCWords.ToList().ForEach(x => buf.Append(x.ToString()));
            return buf.ToString();
        }
    }
}
