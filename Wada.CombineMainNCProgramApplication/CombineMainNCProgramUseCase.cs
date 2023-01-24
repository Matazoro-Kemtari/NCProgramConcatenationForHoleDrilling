using Wada.NCProgramConcatenationService.MainProgramCombiner;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.CombineMainNCProgramApplication
{
    public interface ICombineMainNCProgramUseCase
    {
        Task<NCProgramCode> ExecuteAsync(IEnumerable<NCProgramCode> combinableCodes);
    }

    public class CombineMainNCProgramUseCase : ICombineMainNCProgramUseCase
    {
        private readonly IMainProgramCombiner _mainProgramCombiner;

        public CombineMainNCProgramUseCase(IMainProgramCombiner mainProgramCombiner)
        {
            _mainProgramCombiner = mainProgramCombiner;
        }

        public async Task<NCProgramCode> ExecuteAsync(IEnumerable<NCProgramCode> combinableCodes)
            => await Task.Run(() => _mainProgramCombiner.Combine(combinableCodes));
    }
}