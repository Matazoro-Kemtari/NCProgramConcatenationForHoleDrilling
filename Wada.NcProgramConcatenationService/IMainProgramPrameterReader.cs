using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService
{
    public interface IMainProgramPrameterReader
    {
        /// <summary>
        /// パラメータエクセルを読みだす
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        Task<IEnumerable<IMainProgramPrameter>> ReadAllAsync(Stream stream);
    }
}
