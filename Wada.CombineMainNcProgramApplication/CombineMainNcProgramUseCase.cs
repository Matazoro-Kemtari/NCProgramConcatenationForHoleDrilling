using Wada.Extensions;
using Wada.NcProgramConcatenationService.MainProgramCombiner;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNcProgramApplication
{
    public interface ICombineMainNcProgramUseCase
    {
        Task<CombineMainNcProgramDto> ExecuteAsync(CombineMainNcProgramParam combineMainNcProgramParam);
    }

    public class CombineMainNcProgramUseCase : ICombineMainNcProgramUseCase
    {
        private readonly IMainProgramCombiner _mainProgramCombiner;

        public CombineMainNcProgramUseCase(IMainProgramCombiner mainProgramCombiner)
        {
            _mainProgramCombiner = mainProgramCombiner;
        }

        public async Task<CombineMainNcProgramDto> ExecuteAsync(CombineMainNcProgramParam combineMainNcProgramParam)
            => await Task.Run(
                async () => new CombineMainNcProgramDto(
                    NcProgramCodeAttempt.Parse(
                        _mainProgramCombiner.Combine(
                            await Task.WhenAll(
                                combineMainNcProgramParam.CombinableCodes
                                .Select(
                                    async x => await Task.Run(() => x.Convert()))),
                            combineMainNcProgramParam.MachineTool.GetEnumDisplayName() ?? string.Empty,
                            combineMainNcProgramParam.Material.GetEnumDisplayName() ?? string.Empty))));
    }

    public record class CombineMainNcProgramParam(IEnumerable<NcProgramCodeAttempt> CombinableCodes, MachineToolTypeAttempt MachineTool, MaterialTypeAttempt Material);

    public record class CombineMainNcProgramDto(NcProgramCodeAttempt NcProgramCode);
}
