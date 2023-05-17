using Microsoft.Extensions.Configuration;
using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;
using Wada.NcProgramConcatenationService.NcProgramAggregation;
using Wada.NcProgramConcatenationService.NCProgramAggregation;
using Wada.NcProgramConcatenationService.ValueObjects;
using Wada.UseCase.DataClass;

namespace Wada.ReadSubNcProgramApplication;

public interface IReadSubNcProgramUseCase
{
    Task<OperationDirecterAttemp> ExecuteAsync(string path);
}

public class ReadSubNcProgramUseCase : IReadSubNcProgramUseCase
{
    private readonly IConfiguration _configuration;
    private readonly IStreamReaderOpener _streamReaderOpener;
    private readonly INcProgramReadWriter _ncProgramReadWriter;
    private readonly IStreamOpener _streamOpener;
    private readonly IDrillSizeDataReader _drillSizeDataReader;

    public ReadSubNcProgramUseCase(IConfiguration configuration,
                                   IStreamReaderOpener streamReaderOpener,
                                   INcProgramReadWriter ncProgramReadWriter,
                                   IStreamOpener streamOpener,
                                   IDrillSizeDataReader drillSizeDataReader)
    {
        _configuration = configuration;
        _streamReaderOpener = streamReaderOpener;
        _ncProgramReadWriter = ncProgramReadWriter;
        _streamOpener = streamOpener;
        _drillSizeDataReader = drillSizeDataReader;
    }

    [Logging]
    public async Task<OperationDirecterAttemp> ExecuteAsync(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        // サブプログラムを読み込む
        using StreamReader reader = _streamReaderOpener.Open(path);

        try
        {
            var ncProgramCode = await _ncProgramReadWriter.ReadAllAsync(reader, NcProgramType.SubProgram, fileName);
            var drillSizeData = await ReadDrillSizeDatasAsync();
            return OperationDirecterAttemp.Parse(
                OperationDirecter.Create(ncProgramCode, drillSizeData));
        }
        catch (Exception ex) when (ex is DomainException || ex is DirectedOperationNotFoundException || ex is DirectedOperationToolDiameterNotFoundException)
        {
            throw new ReadSubNcProgramUseCaseException(ex.Message, ex);
        }
    }

    [Logging]
    private async Task<IEnumerable<DrillSizeData>> ReadDrillSizeDatasAsync()
    {
        var path = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..",
            _configuration["applicationConfiguration:ListDirectory"]
            ?? throw new InvalidOperationException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:ListDirectory"),
            _configuration["applicationConfiguration:InchTable"]
            ?? throw new InvalidOperationException(
                "設定情報が取得できませんでした システム担当まで連絡してしてください\n" +
                "applicationConfiguration:InchTable"));
        using Stream stream = _streamOpener.Open(path);
        return await _drillSizeDataReader.ReadAllAsync(stream);
    }
}