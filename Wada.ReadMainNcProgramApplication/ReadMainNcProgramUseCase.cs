using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNcProgramApplication
{
    public interface IReadMainNcProgramUseCase
    {
        Task<IEnumerable<MainNcProgramCodeDto>> ExecuteAsync();
    }

    public record class MainNcProgramCodeDto(
        MachineToolTypeAttempt MachineToolClassification,
        IEnumerable<NcProgramCodeAttempt> NcProgramCodeAttempts);

    public class ReadMainNcProgramUseCase : IReadMainNcProgramUseCase
    {
        private readonly IConfiguration _configuration;
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INcProgramReadWriter _ncProgramReadWriter;

        public ReadMainNcProgramUseCase(IConfiguration configuration, IStreamReaderOpener streamReaderOpener, INcProgramReadWriter ncProgramReadWriter)
        {
            _configuration = configuration;
            _streamReaderOpener = streamReaderOpener;
            _ncProgramReadWriter = ncProgramReadWriter;
        }

        [Logging]
        public async Task<IEnumerable<MainNcProgramCodeDto>> ExecuteAsync()
        {
            var mainPrograms = FetchMainPrograms();
            var machineNames = FetchMachineNames();

            var directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                _configuration["applicationConfiguration:MainNcProgramDirectory"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:MainNcProgramDirectory"));

            var task = machineNames.Select(async machine =>
            {
                NcProgramCodeAttempt[] ncProgramCodeAttempts;
                try
                {
                    ncProgramCodeAttempts = await Task.WhenAll(mainPrograms.Select(async program =>
                    {
                        var fileName = Path.GetFileNameWithoutExtension(program.FileName);
                        var path = Path.Combine(directory, $"{machine}_{program.FileName}");

                        // メインプログラムを読み込む
                        using StreamReader reader = _streamReaderOpener.Open(path);
                        var ncProgramCode = await _ncProgramReadWriter.ReadAllAsync(reader, program.NcProgramType, fileName);
                        return NcProgramCodeAttempt.Parse(ncProgramCode);
                    }));
                }
                catch (OpenFileStreamReaderException ex)
                {
                    throw new ReadMainNcProgramUseCaseException(ex.Message);
                }
                catch (Exception ex) when (ex is DomainException || ex is InvalidOperationException)
                {
                    throw new ReadMainNcProgramUseCaseException(
                        $"メインプログラムの読み込みでエラーが発生しました\n{ex.Message}", ex);
                }

                MachineToolTypeAttempt machineClassification = machine switch
                {
                    "RB250F" => MachineToolTypeAttempt.RB250F,
                    "RB260" => MachineToolTypeAttempt.RB260,
                    "3軸立型" => MachineToolTypeAttempt.Triaxial,
                    _ => throw new NotImplementedException(),
                };

                return new MainNcProgramCodeDto(
                    machineClassification,
                    ncProgramCodeAttempts);
            });

            return await Task.WhenAll(task);
        }

        /// <summary>
        /// 設定ファイルから設備名のリストを取得する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private List<string> FetchMachineNames()
            => _configuration.GetSection("applicationConfiguration:MachineNames")
                             .Get<List<string>>()
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:MachineNames");

        /// <summary>
        /// 設定ファイルからメインプログラム名を取得してtupleのリストを返す
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private List<(string FileName, NcProgramType NcProgramType)> FetchMainPrograms()
        {
            return new()
            {
                (_configuration["applicationConfiguration:CenterDrillingProgramName"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:CenterDrillingProgramName"),
                    NcProgramType.CenterDrilling),
                (_configuration["applicationConfiguration:DrillingProgramName"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:DrillingProgramName"),
                    NcProgramType.Drilling),
                (_configuration["applicationConfiguration:ChamferingProgramName"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:ChamferingProgramName"),
                    NcProgramType.Chamfering),
                (_configuration["applicationConfiguration:ReamingProgramName"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:ReamingProgramName"),
                    NcProgramType.Reaming),
                (_configuration["applicationConfiguration:TappingProgramName"]
                ?? throw new InvalidOperationException(
                    "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                    "applicationConfiguration:TappingProgramName"),
                    NcProgramType.Tapping),
            };
        }
    }
}
