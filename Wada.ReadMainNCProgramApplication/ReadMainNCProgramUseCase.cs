using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.ReadMainNCProgramApplication
{
    public interface IReadMainNCProgramUseCase
    {
        Task<IEnumerable<MainNCProgramDTO>> ExecuteAsync();
    }

    public record class MainNCProgramDTO(string ID, NCProgramCode NCProgramCode);

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
        public async Task<IEnumerable<MainNCProgramDTO>> ExecuteAsync()
        {
            // TODO: 設備ごとメインプログラム　引数付けてる必要あり
            List<string> _mainProgramNames = new()
            {
                "CD.txt",
                "DR.txt",
                "MENTORI.txt",
                "REAMER.txt",
                "TAP.txt",
            };

            string directory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "..",
                "メインプログラム");

            var task = _mainProgramNames.Select(async x =>
            {
                var fileName = Path.GetFileNameWithoutExtension(x);
                var path = Path.Combine(directory, x);

                // サブプログラムを読み込む
                using StreamReader reader = _streamReaderOpener.Open(path);
                var ncProgramCode = await _ncProgramRepository.ReadAllAsync(reader, fileName);
                return new MainNCProgramDTO(fileName, ncProgramCode);
            });

            return await Task.WhenAll(task);
        }
    }
}