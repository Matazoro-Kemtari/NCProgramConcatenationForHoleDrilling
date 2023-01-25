using Wada.NCProgramConcatenationService;
using Wada.UseCase.DataClass;

namespace Wada.StoreNCProgramCodeApplication
{
    public interface IStoreNCProgramCodeUseCase
    {
        Task ExecuteAsync(string path, NCProgramCodeAttempt storableCode);
    }

    public class StoreNCProgramCodeUseCase : IStoreNCProgramCodeUseCase
    {
        private readonly IStreamWriterOpener _streamWriterOpener;
        private readonly INCProgramRepository _ncProgramRepository;

        public StoreNCProgramCodeUseCase(IStreamWriterOpener streamWriterOpener, INCProgramRepository ncProgramRepository)
        {
            _streamWriterOpener = streamWriterOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        public async Task ExecuteAsync(string path, NCProgramCodeAttempt storableCode)
        {
            // 結合プログラムを書き込む
            using var writer = _streamWriterOpener.Open(path);
            await _ncProgramRepository.WriteAllAsync(writer, storableCode.ToString());
        }
    }
}