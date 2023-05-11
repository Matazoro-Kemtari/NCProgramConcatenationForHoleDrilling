using Wada.NcProgramConcatenationService;
using Wada.UseCase.DataClass;

namespace Wada.StoreNcProgramCodeApplication
{
    public interface IStoreNcProgramCodeUseCase
    {
        Task ExecuteAsync(string path, NcProgramCodeAttempt storableCode);
    }

    public class StoreNcProgramCodeUseCase : IStoreNcProgramCodeUseCase
    {
        private readonly IStreamWriterOpener _streamWriterOpener;
        private readonly INcProgramRepository _ncProgramRepository;

        public StoreNcProgramCodeUseCase(IStreamWriterOpener streamWriterOpener, INcProgramRepository ncProgramRepository)
        {
            _streamWriterOpener = streamWriterOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        public async Task ExecuteAsync(string path, NcProgramCodeAttempt storableCode)
        {
            // 結合プログラムを書き込む
            using var writer = _streamWriterOpener.Open(path);
            await _ncProgramRepository.WriteAllAsync(writer, storableCode.ToString());
        }
    }
}