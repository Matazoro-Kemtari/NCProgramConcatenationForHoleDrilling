using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

namespace Wada.ReadMainNCProgramApplication
{
    public interface IReadMainNCProgramUseCase
    {
        Task<IEnumerable<MainNCProgramCodeDTO>> ExecuteAsync();
    }

    public record class MainNCProgramCodeDTO(
        MachineToolTypeAttempt MachineToolClassification,
        IEnumerable<NCProgramCodeAttempt> NCProgramCodeAttempts);

    public class ReadMainNCProgramUseCase : IReadMainNCProgramUseCase
    {
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INCProgramRepository _ncProgramRepository;

        public ReadMainNCProgramUseCase(IStreamReaderOpener streamReaderOpener, INCProgramRepository ncProgramRepository)
        {
            _streamReaderOpener = streamReaderOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        [Logging]
        public async Task<IEnumerable<MainNCProgramCodeDTO>> ExecuteAsync()
        {
            List<(string FileName, NCProgramType NCProgramType)> mainPrograms = new()
            {
                ("CD.txt",NCProgramType.CenterDrilling),
                ("DR.txt",NCProgramType.Drilling),
                ("MENTORI.txt",NCProgramType.Chamfering),
                ("REAMER.txt",NCProgramType.Reaming),
                ("TAP.txt",NCProgramType.Tapping),
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
                NCProgramCodeAttempt[] ncProgramCodeAttempts;
                try
                {
                    ncProgramCodeAttempts = await Task.WhenAll(mainPrograms.Select(async program =>
                    {
                        var fileName = Path.GetFileNameWithoutExtension(program.FileName);
                        var path = Path.Combine(directory, $"{machine}_{program.FileName}");

                        // メインプログラムを読み込む
                        using StreamReader reader = _streamReaderOpener.Open(path);
                        var ncProgramCode = await _ncProgramRepository.ReadAllAsync(reader, program.NCProgramType, fileName);
                        return NCProgramCodeAttempt.Parse(ncProgramCode);
                    }));
                }
                catch (OpenFileStreamReaderException ex)
                {
                    throw new ReadMainNCProgramApplicationException(ex.Message);
                }
                catch (Exception ex) when (ex is DomainException || ex is InvalidOperationException)
                {
                    throw new ReadMainNCProgramApplicationException(
                        $"メインプログラムの読み込みでエラーが発生しました\n{ex.Message}", ex);
                }

                MachineToolTypeAttempt machineClassification = machine switch
                {
                    "RB250F" => MachineToolTypeAttempt.RB250F,
                    "RB260" => MachineToolTypeAttempt.RB260,
                    "3軸立型" => MachineToolTypeAttempt.Triaxial,
                    _ => throw new NotImplementedException(),
                };

                return new MainNCProgramCodeDTO(
                    machineClassification,
                    ncProgramCodeAttempts);
            });

            return await Task.WhenAll(task);
        }
    }
}