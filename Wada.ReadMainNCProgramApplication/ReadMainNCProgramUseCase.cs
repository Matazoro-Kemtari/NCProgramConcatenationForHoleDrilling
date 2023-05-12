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
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INcProgramReadWriter _ncProgramReadWriter;

        public ReadMainNcProgramUseCase(IStreamReaderOpener streamReaderOpener, INcProgramReadWriter ncProgramReadWriter)
        {
            _streamReaderOpener = streamReaderOpener;
            _ncProgramReadWriter = ncProgramReadWriter;
        }

        [Logging]
        public async Task<IEnumerable<MainNcProgramCodeDto>> ExecuteAsync()
        {
            List<(string FileName, NcProgramType NCProgramType)> mainPrograms = new()
            {
                ("CD.txt",NcProgramType.CenterDrilling),
                ("DR.txt",NcProgramType.Drilling),
                ("MENTORI.txt",NcProgramType.Chamfering),
                ("REAMER.txt",NcProgramType.Reaming),
                ("TAP.txt",NcProgramType.Tapping),
            };

            List<string> machineName = new()
            {
                "RB250F",
                "RB260",
                "3軸立型",
            };

            string directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "メインプログラム");

            var task = machineName.Select(async machine =>
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
                        var ncProgramCode = await _ncProgramReadWriter.ReadAllAsync(reader, program.NCProgramType, fileName);
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
    }
}