using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;

namespace Wada.NcProgramConcatenationService
{
    public interface INcProgramReadWriter
    {
        Task<NcProgramCode> ReadAllAsync(StreamReader reader, NcProgramType ncProgram, string programName);

        Task WriteAllAsync(StreamWriter writer, string ncProgramCode);
    }
}
