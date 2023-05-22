using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.MainProgramParameterSpreadSheet;
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
    IEnumerable<ReamingProgramParameterAttempt> CrystalReamerParameters,
    IEnumerable<ReamingProgramParameterAttempt> SkillReamerParameters,
    IEnumerable<TappingProgramParameterAttempt> TapParameters,
    IEnumerable<DrillingProgramParameterAttempt> DrillingParameters)
{
    public MainNcProgramParametersAttempt Convert()
        => new(CrystalReamerParameters,
               SkillReamerParameters,
               TapParameters,
               DrillingParameters);
}

public class ReadMainNcProgramParametersUseCase : IReadMainNcProgramParametersUseCase
{
    private readonly IConfiguration _configuration;
    private readonly IStreamOpener _streamOpener;
    private readonly IMainProgramParameterReader _reamingParameterReader;
    private readonly IMainProgramParameterReader _tappingParameterReader;
    private readonly IMainProgramParameterReader _drillingParameterReader;

    public ReadMainNcProgramParametersUseCase(IConfiguration configuration, IStreamOpener streamOpener, ReamingParameterReader reamingParameterReader, TappingParameterReader tappingParameterReader, DrillingParameterReader drillingParameterReader)
    {
        _configuration = configuration;
        _streamOpener = streamOpener;
        _reamingParameterReader = reamingParameterReader;
        _tappingParameterReader = tappingParameterReader;
        _drillingParameterReader = drillingParameterReader;
    }

    [Logging]
    public async Task<MainNcProgramParametersDto> ExecuteAsync()
    {
        string directory = Path.Combine(
            _configuration["applicationConfiguration:ListDirectory"]
            ?? throw new InvalidOperationException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:ListDirectory"));

        IEnumerable<IMainProgramParameter>[] parameters;
        try
        {
            var crystalRemmerPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:CrystalRemmerTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:CrystalRemmerTable"));
            using Stream crystalRemmerStream = _streamOpener.Open(crystalRemmerPath);
            var crystalReamerTask = _reamingParameterReader.ReadAllAsync(crystalRemmerStream);

            var skillReammerPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:SkillRemmerTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:SkillRemmerTable"));
            using Stream skillReammerStream = _streamOpener.Open(skillReammerPath);
            var skillReamerTask = _reamingParameterReader.ReadAllAsync(skillReammerStream);

            var tapPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:TapTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:TapTable"));
            using Stream tapStream = _streamOpener.Open(tapPath);
            var tapTask = _tappingParameterReader.ReadAllAsync(tapStream);

            var drillPath = Path.Combine(
                directory,
                _configuration["applicationConfiguration:DrillTable"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:DrillTable"));
            using Stream drillStream = _streamOpener.Open(drillPath);
            var drillTask = _drillingParameterReader.ReadAllAsync(drillStream);

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
            parameters[0].Select(x => ReamingProgramParameterAttempt.Parse((ReamingProgramParameter)x)),
            parameters[1].Select(x => ReamingProgramParameterAttempt.Parse((ReamingProgramParameter)x)),
            parameters[2].Select(x => TappingProgramParameterAttempt.Parse((TappingProgramParameter)x)),
            parameters[3].Select(x => DrillingProgramParameterAttempt.Parse((DrillingProgramParameter)x)));
    }
}