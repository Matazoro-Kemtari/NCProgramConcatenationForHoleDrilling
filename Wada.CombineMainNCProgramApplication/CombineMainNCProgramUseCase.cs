using Wada.Extension;
using Wada.NcProgramConcatenationService.MainProgramCombiner;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNcProgramApplication
{
    public interface ICombineMainNcProgramUseCase
    {
        Task<CombineMainNCProgramDto> ExecuteAsync(CombineMainNcProgramParam combineMainNCProgramParam);
    }

    public class CombineMainNcProgramUseCase : ICombineMainNcProgramUseCase
    {
        private readonly IMainProgramCombiner _mainProgramCombiner;

        public CombineMainNcProgramUseCase(IMainProgramCombiner mainProgramCombiner)
        {
            _mainProgramCombiner = mainProgramCombiner;
        }

        public async Task<CombineMainNCProgramDto> ExecuteAsync(CombineMainNcProgramParam combineMainNCProgramParam)
            => await Task.Run(
                async () => new CombineMainNCProgramDto(
                    NcProgramCodeAttempt.Parse(
                        _mainProgramCombiner.Combine(
                            await Task.WhenAll(
                                combineMainNCProgramParam.CombinableCodes
                                .Select(
                                    async x => await Task.Run(() => x.Convert()))),
                            combineMainNCProgramParam.MachineTool.GetEnumDisplayName() ?? string.Empty,
                            combineMainNCProgramParam.Material.GetEnumDisplayName() ?? string.Empty))));
    }

    public record class CombineMainNcProgramParam(IEnumerable<NcProgramCodeAttempt> CombinableCodes, MachineToolTypeAttempt MachineTool, MaterialTypeAttempt Material);

    public record class CombineMainNCProgramDto(NcProgramCodeAttempt NCProgramCode);
}