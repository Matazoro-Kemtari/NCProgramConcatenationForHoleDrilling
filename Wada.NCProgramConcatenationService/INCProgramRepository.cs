using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;

namespace Wada.NCProgramConcatenationService
{
    public interface INCProgramRepository
    {
        Task<NCProgramCode> ReadAllAsync(StreamReader reader, NCProgramType ncProgram, string programName);
    }
}
