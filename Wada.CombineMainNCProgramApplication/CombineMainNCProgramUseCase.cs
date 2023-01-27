using Wada.Extension;
using Wada.NCProgramConcatenationService.MainProgramCombiner;
using Wada.UseCase.DataClass;

namespace Wada.CombineMainNCProgramApplication
{
    public interface ICombineMainNCProgramUseCase
    {
        Task<CombineMainNCProgramDTO> ExecuteAsync(CombineMainNCProgramParam combineMainNCProgramParam);
    }

    public class CombineMainNCProgramUseCase : ICombineMainNCProgramUseCase
    {
        private readonly IMainProgramCombiner _mainProgramCombiner;

        public CombineMainNCProgramUseCase(IMainProgramCombiner mainProgramCombiner)
        {
            _mainProgramCombiner = mainProgramCombiner;
        }

        public async Task<CombineMainNCProgramDTO> ExecuteAsync(CombineMainNCProgramParam combineMainNCProgramParam)
            => await Task.Run(
                () => new CombineMainNCProgramDTO(
                    NCProgramCodeAttempt.Parse(
                        _mainProgramCombiner.Combine(
                            combineMainNCProgramParam.CombinableCodes.Select(x => x.Convert()),
                            combineMainNCProgramParam.MachineTool.GetEnumDisplayName() ?? string.Empty,
                            combineMainNCProgramParam.Material.GetEnumDisplayName() ?? string.Empty))));
    }

    public record class CombineMainNCProgramParam(IEnumerable<NCProgramCodeAttempt> CombinableCodes, MachineToolTypeAttempt MachineTool, MaterialTypeAttempt Material);

    public record class CombineMainNCProgramDTO(NCProgramCodeAttempt NCProgramCode);
}