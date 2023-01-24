using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.CombineMainNCProgramApplication
{
    public interface ICombineMainNCProgramUseCase
    {
        Task<NCProgramCode> ExecuteAsync(IEnumerable<NCProgramCode> combinableCode);
    }

    public class CombineMainNCProgramUseCase : ICombineMainNCProgramUseCase
    {
        public Task<NCProgramCode> ExecuteAsync(IEnumerable<NCProgramCode> combinableCode)
        {
            throw new NotImplementedException();
        }
    }
}