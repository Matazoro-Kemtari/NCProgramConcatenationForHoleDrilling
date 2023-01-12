using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.ReadMainNCProgramParametersApplication
{
    public interface IReadMainNCProgramParametersUseCase
    {
        Task<IMainProgramPrameter> ExecuteAsync();
    }
    public class ReadMainNCProgramParametersUseCase : IReadMainNCProgramParametersUseCase
    {
        public Task<IMainProgramPrameter> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}