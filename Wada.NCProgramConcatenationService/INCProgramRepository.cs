using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.NCProgramConcatenationService
{
    public interface INCProgramRepository
    {
        NCProgram ReadAll(StreamReader reader);
    }
}
