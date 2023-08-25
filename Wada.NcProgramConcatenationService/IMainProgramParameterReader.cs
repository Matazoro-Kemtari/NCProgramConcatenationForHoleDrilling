using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService
{
    public interface IMainProgramParameterReader
    {
        /// <summary>
        /// パラメータエクセルを読みだす
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        Task<IEnumerable<IMainProgramParameter>> ReadAllAsync(Stream stream);
    }
}
