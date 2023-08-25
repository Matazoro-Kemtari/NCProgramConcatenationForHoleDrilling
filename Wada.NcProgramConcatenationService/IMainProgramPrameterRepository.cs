using Wada.NcProgramConcatenationService.MainProgramParameterAggregation;

namespace Wada.NcProgramConcatenationService
{
    public interface IMainProgramPrameterRepository
    {
        /// <summary>
        /// パラメータエクセルを読みだす
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        IEnumerable<IMainProgramPrameter> ReadAll(Stream stream);
    }

    //public interface ITappingPrameterRepository
    //{
    //    /// <summary>
    //    /// パラメータエクセルを読みだす
    //    /// </summary>
    //    /// <param name="reader"></param>
    //    /// <returns></returns>
    //    IEnumerable<IMainProgramPrameter> ReadAll(Stream stream);
    //}
}
