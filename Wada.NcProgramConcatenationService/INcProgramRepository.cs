using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService
{
    public interface INcProgramRepository
    {
        Task<NcProgramCode> ReadAllAsync(StreamReader reader, NcProgramRole ncProgram, string programName);
        
        Task WriteAllAsync(StreamWriter writer, string ncProgramCode);
    }
}
