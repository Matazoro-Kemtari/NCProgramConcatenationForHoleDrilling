using Wada.AOP.Logging;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNCProgramParametersApplication
{
    public interface IReadMainNCProgramParametersUseCase
    {
        Task<MainNCProgramParametersDTO> ExecuteAsync();
    }

    public record class MainNCProgramParametersDTO(
        IEnumerable<ReamingProgramPrameterAttempt> CrystalReamerParameters,
        IEnumerable<ReamingProgramPrameterAttempt> SkillReamerParameters,
        IEnumerable<TappingProgramPrameterAttempt> TapParameters,
        IEnumerable<DrillingProgramPrameterAttempt> DrillingPrameters)
    {
        public MainNCProgramParametersAttempt Convert()
            => new(CrystalReamerParameters,
                   SkillReamerParameters,
                   TapParameters,
                   DrillingPrameters);
    }

    public class ReadMainNCProgramParametersUseCase : IReadMainNCProgramParametersUseCase
    {
        private readonly IStreamOpener _streamOpener;
        private readonly IMainProgramPrameterRepository _reamingPrameterRepository;
        private readonly IMainProgramPrameterRepository _tappingPrameterRepository;
        private readonly IMainProgramPrameterRepository _drillingPrameterRepository;

        public ReadMainNCProgramParametersUseCase(IStreamOpener streamOpener, ReamingPrameterRepository reamingPrameterRepository, TappingPrameterRepository tappingPrameterRepository, DrillingParameterRepositoy drillingPrameterRepository)
        {
            _streamOpener = streamOpener;
            _reamingPrameterRepository = reamingPrameterRepository;
            _tappingPrameterRepository = tappingPrameterRepository;
            _drillingPrameterRepository = drillingPrameterRepository;
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
                return _drillingPrameterRepository.ReadAll(stream);
            });

            IEnumerable<IMainProgramPrameter>[] parameters =
                await Task.WhenAll(crystalReamerTask, skillReamerTask, tapTask, drillTask);
            return new MainNCProgramParametersDTO(
                parameters[0].Select(x => ReamingProgramPrameterAttempt.Parse((ReamingProgramPrameter)x)),
                parameters[1].Select(x => ReamingProgramPrameterAttempt.Parse((ReamingProgramPrameter)x)),
                parameters[2].Select(x => TappingProgramPrameterAttempt.Parse((TappingProgramPrameter)x)),
                parameters[3].Select(x => DrillingProgramPrameterAttempt.Parse((DrillingProgramPrameter)x)));
        }
    }
}