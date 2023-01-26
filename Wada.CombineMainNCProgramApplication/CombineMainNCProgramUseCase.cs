using Wada.NCProgramConcatenationService.MainProgramCombiner;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNCProgramApplication
{
    public interface ICombineMainNCProgramUseCase
    {
        Task<CombineMainNCProgramDTO> ExecuteAsync(IEnumerable<NCProgramCodeAttempt> combinableCodes);
    }

    public class CombineMainNCProgramUseCase : ICombineMainNCProgramUseCase
    {
        private readonly IMainProgramCombiner _mainProgramCombiner;

        public CombineMainNCProgramUseCase(IMainProgramCombiner mainProgramCombiner)
        {
            _mainProgramCombiner = mainProgramCombiner;
        }

        public async Task<CombineMainNCProgramDTO> ExecuteAsync(IEnumerable<NCProgramCodeAttempt> combinableCodes)
            => await Task.Run(
                () => new CombineMainNCProgramDTO(
                    NCProgramCodeAttempt.Parse(
                        _mainProgramCombiner.Combine(
                            combinableCodes.Select(x => x.Convert())))));
    }

    public record class CombineMainNCProgramDTO(NCProgramCodeAttempt NCProgramCode);
}