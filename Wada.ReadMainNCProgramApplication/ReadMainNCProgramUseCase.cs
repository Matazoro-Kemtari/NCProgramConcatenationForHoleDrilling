using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

namespace Wada.ReadMainNCProgramApplication
{
    public interface IReadMainNCProgramUseCase
    {
        Task<NCProgramCode> ExecuteAsync(string path);
    }

    public class ReadMainNCProgramUseCase: IReadMainNCProgramUseCase
    {
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INCProgramRepository _ncProgramRepository;

        public ReadMainNCProgramUseCase(IStreamReaderOpener streamReaderOpener, INCProgramRepository ncProgramRepository)
        {
            _streamReaderOpener = streamReaderOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        [Logging]
        public async Task<NCProgramCode> ExecuteAsync(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            // サブプログラムを読み込む
            using StreamReader reader = _streamReaderOpener.Open(path);
            return await _ncProgramRepository.ReadAllAsync(reader, fileName);
        }
    }
}