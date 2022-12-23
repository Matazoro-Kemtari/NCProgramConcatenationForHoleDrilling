using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService
{
    public interface INCProgramRepository
    {
        Task<NCProgramCode> ReadAllAsync(StreamReader reader, string programName);
    }
}
