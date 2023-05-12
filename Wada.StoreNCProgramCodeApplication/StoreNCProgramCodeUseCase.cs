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
        private readonly INcProgramReadWriter _ncProgramReadWriter;

        public StoreNcProgramCodeUseCase(IStreamWriterOpener streamWriterOpener, INcProgramReadWriter ncProgramReadWriter)
        {
            _streamWriterOpener = streamWriterOpener;
            _ncProgramReadWriter = ncProgramReadWriter;
        }

        public async Task ExecuteAsync(string path, NcProgramCodeAttempt storableCode)
        {
            // 結合プログラムを書き込む
            using var writer = _streamWriterOpener.Open(path);
            await _ncProgramReadWriter.WriteAllAsync(writer, storableCode.ToString());
        }
    }
}