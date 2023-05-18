using Microsoft.Extensions.Configuration;
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
    IEnumerable<DrillingProgramPrameterAttempt> DrillingPrameters)
{
    public MainNcProgramParametersAttempt Convert()
        => new(CrystalReamerParameters,
               SkillReamerParameters,
               TapParameters,
               DrillingPrameters);
}

public class ReadMainNcProgramParametersUseCase : IReadMainNcProgramParametersUseCase
{
    private readonly IConfiguration _configuration;
    private readonly IStreamOpener _streamOpener;
    private readonly IMainProgramPrameterReader _reamingPrameterReader;
    private readonly IMainProgramPrameterReader _tappingPrameterReader;
    private readonly IMainProgramPrameterReader _drillingPrameterReader;

    public ReadMainNcProgramParametersUseCase(IConfiguration configuration, IStreamOpener streamOpener, ReamingPrameterReader reamingPrameterReader, TappingPrameterReader tappingPrameterReader, DrillingParameterReader drillingPrameterReader)
    {
        _configuration = configuration;
        _streamOpener = streamOpener;
        _reamingPrameterReader = reamingPrameterReader;
        _tappingPrameterReader = tappingPrameterReader;
        _drillingPrameterReader = drillingPrameterReader;
    }

    [Logging]
    public async Task<MainNcProgramParametersDto> ExecuteAsync()
    {
        string directory = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..",
            _configuration["applicationConfiguration:ListDirectory"]
            ?? throw new InvalidOperationException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:ListDirectory"));

        IEnumerable<IMainProgramPrameter>[] parameters;
        try
        {
            var crystalRemmerPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:CrystalRemmerTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:CrystalRemmerTable"));
            using Stream crystalRemmerStream = _streamOpener.Open(crystalRemmerPath);
            var crystalReamerTask = _reamingPrameterReader.ReadAllAsync(crystalRemmerStream);

            var skillReammerPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:SkillRemmerTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:SkillRemmerTable"));
            using Stream skillReammerStream = _streamOpener.Open(skillReammerPath);
            var skillReamerTask = _reamingPrameterReader.ReadAllAsync(skillReammerStream);

            var tapPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:TapTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:TapTable"));
            using Stream tapStream = _streamOpener.Open(tapPath);
            var tapTask = _tappingPrameterReader.ReadAllAsync(tapStream);

            var drillPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:DrillTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:DrillTable"));
            using Stream drillStream = _streamOpener.Open(drillPath);
            var drillTask = _drillingPrameterReader.ReadAllAsync(drillStream);

            parameters = await Task.WhenAll(crystalReamerTask,
                                            skillReamerTask,
                                            tapTask,
                                            drillTask);
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
            parameters[3].Select(x => DrillingProgramPrameterAttempt.Parse((DrillingProgramPrameter)x)));
    }
}