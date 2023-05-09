using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;
using Wada.NCProgramConcatenationService.NCProgramAggregation;
using Wada.NCProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

namespace Wada.ReadSubNCProgramApplication
{
    public interface IReadSubNCProgramUseCase
    {
        Task<SubNCProgramCodeAttemp> ExecuteAsync(string path);
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
        public async Task<SubNCProgramCodeAttemp> ExecuteAsync(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            // サブプログラムを読み込む
            using StreamReader reader = _streamReaderOpener.Open(path);

            try
            {
                var ncProgramCode = await _ncProgramRepository.ReadAllAsync(reader, NCProgramType.SubProgram, fileName);
                return SubNCProgramCodeAttemp.Parse(
                    SubNCProgramCode.Parse(ncProgramCode));
            }
            catch (Exception ex) when (ex is DomainException || ex is DirectedOperationNotFoundException || ex is DirectedOperationToolDiameterNotFoundException )
            {
                throw new ReadSubNCProgramApplicationException(ex.Message, ex);
            }
        }
    }
}