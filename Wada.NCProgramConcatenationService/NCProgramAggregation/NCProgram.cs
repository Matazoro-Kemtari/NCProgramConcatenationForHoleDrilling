using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService.NCProgramAggregation
{
    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class NCProgram
    {
        public NCProgram(int programNumber, IEnumerable<NCBlock?> ncBlocks)
        {
            ID = Ulid.NewUlid();
            ProgramNumber = programNumber;
            NCBlocks = ncBlocks;
        }

        public Ulid ID { get; }

        /// <summary>
        /// プログラム番号
        /// </summary>
        [IgnoreDuringEquals]
        public int ProgramNumber { get; init; }

        [IgnoreDuringEquals]
        public IEnumerable<NCBlock?> NCBlocks { get; init; }
    }

    [Equals(DoNotAddEqualityOperators = true), ToString]
    public class NCBlock
    {
        public NCBlock(IEnumerable<INCWord?> ncWords)
        {
            SequenceID= Ulid.NewUlid();
            NCWords = ncWords ?? throw new ArgumentNullException(nameof(ncWords));
        }

        public Ulid SequenceID { get; }

        [IgnoreDuringEquals]
        public IEnumerable<INCWord?> NCWords { get; init; }
    }
}
