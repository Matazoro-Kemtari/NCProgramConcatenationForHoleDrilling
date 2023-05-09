using Wada.AOP.Logging;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNCProgramParametersApplication
{
    public interface IReadMainNCProgramParametersUseCase
    {
        /// <summary>
        /// リストフォルダのエクセルを読み込む
        /// </summary>
        /// <returns></returns>
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
        private readonly IMainProgramPrameterReader _reamingPrameterReader;
        private readonly IMainProgramPrameterReader _tappingPrameterReader;
        private readonly IMainProgramPrameterReader _drillingPrameterReader;

        public ReadMainNCProgramParametersUseCase(IStreamOpener streamOpener, ReamingPrameterReader reamingPrameterReader, TappingPrameterReader tappingPrameterReader, DrillingParameterReader drillingPrameterReader)
        {
            _streamOpener = streamOpener;
            _reamingPrameterReader = reamingPrameterReader;
            _tappingPrameterReader = tappingPrameterReader;
            _drillingPrameterReader = drillingPrameterReader;
        }

        [Logging]
        public async Task<MainNCProgramParametersDTO> ExecuteAsync()
        {
            string directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "リスト");

            Task<IEnumerable<IMainProgramPrameter>> crystalReamerTask;
            Task<IEnumerable<IMainProgramPrameter>> skillReamerTask;
            Task<IEnumerable<IMainProgramPrameter>> tapTask;
            Task<IEnumerable<IMainProgramPrameter>> drillTask;
            try
            {
                crystalReamerTask = Task.Run(() =>
                {
                    var path = Path.Combine(
                        directory,
                        "クリスタルリーマー.xlsx");
                    using Stream stream = _streamOpener.Open(path);
                    return _reamingPrameterReader.ReadAll(stream);
                });

                skillReamerTask = Task.Run(() =>
                {
                    var path = Path.Combine(
                        directory,
                        "スキルリーマー.xlsx");
                    using Stream stream = _streamOpener.Open(path);
                    return _reamingPrameterReader.ReadAll(stream);
                });

                tapTask = Task.Run(() =>
                {
                    var path = Path.Combine(
                        directory,
                        "タップ.xlsx");
                    using Stream stream = _streamOpener.Open(path);
                    return _tappingPrameterReader.ReadAll(stream);
                });

                drillTask = Task.Run(() =>
                {
                    var path = Path.Combine(
                        directory,
                        "ドリル.xlsx");
                    using Stream stream = _streamOpener.Open(path);
                    return _drillingPrameterReader.ReadAll(stream);
                });
            }
            catch (OpenFileStreamException ex)
            {
                throw new ReadMainNCProgramParametersApplicationException(ex.Message);
            }
            catch (Exception ex) when (ex is NCProgramConcatenationServiceException || ex is InvalidOperationException)
            {
                throw new ReadMainNCProgramParametersApplicationException(
                    $"リストの読み込みでエラーが発生しました\n{ex.Message}", ex);
            }

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