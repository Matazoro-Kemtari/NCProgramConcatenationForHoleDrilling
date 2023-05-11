using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService
{
    public interface INcProgramRepository
    {
        Task<NcProgramCode> ReadAllAsync(StreamReader reader, NcProgramType ncProgram, string programName);
        
        Task WriteAllAsync(StreamWriter writer, string ncProgramCode);
    }
}
