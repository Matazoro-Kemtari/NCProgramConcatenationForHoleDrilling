using Wada.AOP.Logging;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.ReadMainNCProgramParametersApplication
{
    public interface IReadMainNCProgramParametersUseCase
    {
        Task<MainNCProgramParametersDTO> ExecuteAsync();
    }

    public record class MainNCProgramParametersDTO(IEnumerable<ReamingProgramPrameter> CrystalReamerParameters, IEnumerable<ReamingProgramPrameter> SkillReamerParameters, IEnumerable<TappingProgramPrameter> TapParameters, IEnumerable<DrillingProgramPrameter> DrillingPrameters);

    public class ReadMainNCProgramParametersUseCase : IReadMainNCProgramParametersUseCase
    {
        private readonly IStreamOpener _streamOpener;
        private readonly IMainProgramPrameterRepository _reamingPrameterRepository;
        private readonly IMainProgramPrameterRepository _tappingPrameterRepository;

        public ReadMainNCProgramParametersUseCase(IStreamOpener streamOpener, ReamingPrameterRepository reamingPrameterRepository, TappingPrameterRepository tappingPrameterRepository)
        {
            _streamOpener = streamOpener;
            _reamingPrameterRepository = reamingPrameterRepository;
            _tappingPrameterRepository = tappingPrameterRepository;
        }

        [Logging]
        public async Task<MainNCProgramParametersDTO> ExecuteAsync()
        {
            string directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "リスト");
            var crystalReamerTask = Task.Run(() =>
            {
                var path = Path.Combine(
                    directory,
                    "クリスタルリーマー.xlsx");
                using Stream stream = _streamOpener.Open(path);
                return _reamingPrameterRepository.ReadAll(stream);
            });

            var skillReamerTask = Task.Run(() =>
            {
                var path = Path.Combine(
                    directory,
                    "スキルリーマー.xlsx");
                using Stream stream = _streamOpener.Open(path);
                return _reamingPrameterRepository.ReadAll(stream);
            });

            var tapTask = Task.Run(() =>
            {
                var path = Path.Combine(
                    directory,
                    "タップ.xlsx");
                using Stream stream = _streamOpener.Open(path);
                return _tappingPrameterRepository.ReadAll(stream);
            });

            var drillTask = Task.Run(() =>
            {
                var path = Path.Combine(
                    directory,
                    "ドリル.xlsx");
                using Stream stream = _streamOpener.Open(path);
                return _tappingPrameterRepository.ReadAll(stream);
            });

            IEnumerable<IMainProgramPrameter>[] parameters = await Task.WhenAll(crystalReamerTask, skillReamerTask, tapTask, drillTask);
            return new MainNCProgramParametersDTO(
                (IEnumerable<ReamingProgramPrameter>)parameters[0],
                (IEnumerable<ReamingProgramPrameter>)parameters[1],
                (IEnumerable<TappingProgramPrameter>)parameters[2],
                (IEnumerable<DrillingProgramPrameter>)parameters[3]);
        }
    }
}