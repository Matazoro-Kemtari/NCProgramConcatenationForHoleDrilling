using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NCProgramConcatenationService;

public interface IDrillSizeDataReader
{
    /// <summary>
    /// ExcelファイルのStreamからすべてのドリルサイズデータを読み込む
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    Task<IEnumerable<DrillSizeData>> ReadAllAsync(Stream stream);
}
