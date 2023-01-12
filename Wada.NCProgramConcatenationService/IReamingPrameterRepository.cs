using Wada.NCProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NCProgramConcatenationService
{
    public interface IReamingPrameterRepository
    {
        /// <summary>
        /// パラメータエクセルを読みだす
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        IEnumerable<ReamingProgramPrameter> ReadAll(Stream stream);
    }
}
