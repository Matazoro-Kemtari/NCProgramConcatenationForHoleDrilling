using Wada.AOP.Logging;
using Wada.MainProgramPrameterSpreadSheet;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNcProgramParametersApplication;

public interface IReadMainNcProgramParametersUseCase
{
    /// <summary>
    /// リストフォルダのエクセルを読み込む
    /// </summary>
    /// <returns></returns>
    Task<MainNcProgramParametersDto> ExecuteAsync();
}

public record class MainNcProgramParametersDto(
    IEnumerable<ReamingProgramPrameterAttempt> CrystalReamerParameters,
    IEnumerable<ReamingProgramPrameterAttempt> SkillReamerParameters,
    IEnumerable<TappingProgramPrameterAttempt> TapParameters,
    IEnumerable<DrillingProgramPrameterAttempt> DrillingPrameters,
    IEnumerable<DrillSizeData> DrillSizeData)
{
    public MainNcProgramParametersAttempt Convert()
        => new(CrystalReamerParameters,
               SkillReamerParameters,
               TapParameters,
               DrillingPrameters);
}

public class ReadMainNcProgramParametersUseCase : IReadMainNcProgramParametersUseCase
{
    private readonly IStreamOpener _streamOpener;
    private readonly IMainProgramPrameterReader _reamingPrameterReader;
    private readonly IMainProgramPrameterReader _tappingPrameterReader;
    private readonly IMainProgramPrameterReader _drillingPrameterReader;
    private readonly IDrillSizeDataReader _drillSizeDataReader;

    public ReadMainNcProgramParametersUseCase(IStreamOpener streamOpener, ReamingPrameterReader reamingPrameterReader, TappingPrameterReader tappingPrameterReader, DrillingParameterReader drillingPrameterReader, IDrillSizeDataReader drillSizeDataReader)
    {
        _streamOpener = streamOpener;
        _reamingPrameterReader = reamingPrameterReader;
        _tappingPrameterReader = tappingPrameterReader;
        _drillingPrameterReader = drillingPrameterReader;
        _drillSizeDataReader = drillSizeDataReader;
    }

    [Logging]
    public async Task<MainNcProgramParametersDto> ExecuteAsync()
    {
        string directory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..",
            "リスト");

        IEnumerable<IMainProgramPrameter>[] parameters;
        IEnumerable<DrillSizeData> drillSizeData;
        try
        {
            var crystalRemmerPath = Path.Combine(
                directory,
                "クリスタルリーマー.xlsx");
            using Stream crystalRemmerStream = _streamOpener.Open(crystalRemmerPath);
            var crystalReamerTask = _reamingPrameterReader.ReadAllAsync(crystalRemmerStream);

            var skillReammerPath = Path.Combine(
                directory,
                "スキルリーマー.xlsx");
            using Stream skillReammerStream = _streamOpener.Open(skillReammerPath);
            var skillReamerTask = _reamingPrameterReader.ReadAllAsync(skillReammerStream);

            var tapPath = Path.Combine(
                directory,
                "タップ.xlsx");
            using Stream tapStream = _streamOpener.Open(tapPath);
            var tapTask = _tappingPrameterReader.ReadAllAsync(tapStream);

            var drillPath = Path.Combine(
                directory,
                "ドリル.xlsx");
            using Stream drillStream = _streamOpener.Open(drillPath);
            var drillTask = _drillingPrameterReader.ReadAllAsync(drillStream);

            parameters = await Task.WhenAll(crystalReamerTask,
                                            skillReamerTask,
                                            tapTask,
                                            drillTask);

            var inchPath = Path.Combine(
                directory,
                "インチ.xlsx");
            using Stream inchSream = _streamOpener.Open(inchPath);
            drillSizeData = await _drillSizeDataReader.ReadAllAsync(inchSream);
        }
        catch (OpenFileStreamException ex)
        {
            throw new ReadMainNcProgramParametersUseCaseException(ex.Message);
        }
        catch (Exception ex) when (ex is DomainException or InvalidOperationException)
        {
            throw new ReadMainNcProgramParametersUseCaseException(
                $"リストの読み込みでエラーが発生しました\n{ex.Message}", ex);
        }

        return new MainNcProgramParametersDto(
            parameters[0].Select(x => ReamingProgramPrameterAttempt.Parse((ReamingProgramPrameter)x)),
            parameters[1].Select(x => ReamingProgramPrameterAttempt.Parse((ReamingProgramPrameter)x)),
            parameters[2].Select(x => TappingProgramPrameterAttempt.Parse((TappingProgramPrameter)x)),
            parameters[3].Select(x => DrillingProgramPrameterAttempt.Parse((DrillingProgramPrameter)x)),
            drillSizeData);
    }
}