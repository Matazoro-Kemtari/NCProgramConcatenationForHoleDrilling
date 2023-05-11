using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

namespace Wada.ReadSubNcProgramApplication
{
    public interface IReadSubNcProgramUseCase
    {
        Task<SubNcProgramCodeAttemp> ExecuteAsync(string path);
    }

    public class ReadSubNcProgramUseCase : IReadSubNcProgramUseCase
    {
        private readonly IStreamReaderOpener _streamReaderOpener;
        private readonly INcProgramRepository _ncProgramRepository;

        public ReadSubNcProgramUseCase(IStreamReaderOpener streamReaderOpener, INcProgramRepository ncProgramRepository)
        {
            _streamReaderOpener = streamReaderOpener;
            _ncProgramRepository = ncProgramRepository;
        }

        [Logging]
        public async Task<SubNcProgramCodeAttemp> ExecuteAsync(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            // サブプログラムを読み込む
            using StreamReader reader = _streamReaderOpener.Open(path);

            try
            {
                var ncProgramCode = await _ncProgramRepository.ReadAllAsync(reader, NcProgramType.SubProgram, fileName);
                return SubNcProgramCodeAttemp.Parse(
                    SubNcProgramCode.Parse(ncProgramCode));
            }
            catch (Exception ex) when (ex is DomainException || ex is DirectedOperationNotFoundException || ex is DirectedOperationToolDiameterNotFoundException )
            {
                throw new ReadSubNcProgramUseCaseException(ex.Message, ex);
            }
        }
    }
}