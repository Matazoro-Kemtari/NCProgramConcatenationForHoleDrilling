using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NCProgramConcatenationService
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
