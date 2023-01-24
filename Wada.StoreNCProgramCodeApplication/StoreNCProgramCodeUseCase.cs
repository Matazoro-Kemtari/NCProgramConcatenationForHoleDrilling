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

        public Task ExecuteAsync(string path, NCProgramCodeAttempt storableCode)
        {
            throw new NotImplementedException();
        }
    }
}