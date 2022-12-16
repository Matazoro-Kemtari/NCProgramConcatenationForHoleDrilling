using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;

[module: Logging] // https://stackoverflow.com/questions/49648179/how-to-use-methoddecorator-fody-decorator-in-another-project
namespace Wada.ReadSubNCProgramApplication
{
    public interface IReadSubNCProgramUseCase
    {
        Task<NCProgram> ExecuteAsync(string path);
    }

    public class ReadSubNCProgramUseCase : IReadSubNCProgramUseCase
    {
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INCProgramRepository _ncProgramRepository;

        public ReadSubNCProgramUseCase(IStreamReaderOpener streamReaderOpener, INCProgramRepository ncProgramRepository)
        {
            _streamReaderOpener = streamReaderOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        [Logging]
        public async Task<NCProgram> ExecuteAsync(string path)
        {
            try
            {
                // サブプログラムを読み込む
                return await Task.Run(() =>
                {
                    StreamReader reader = _streamReaderOpener.Open(path);
                    return _ncProgramRepository.ReadAll(reader);
                });
            }
            catch (NCProgramConcatenationServiceException ex)
            {
                throw new ReadSubNCProgramApplicationException(ex.Message, ex);
            }
        }
    }
}