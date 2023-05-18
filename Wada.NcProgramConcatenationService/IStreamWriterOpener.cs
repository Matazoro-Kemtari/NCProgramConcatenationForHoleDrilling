namespace Wada.NcProgramConcatenationService
{
    public interface IStreamWriterOpener
    {
        /// <summary>
        /// ストリームライターを開く
        /// </summary>
        /// <param name="path"></param>
        /// <param name="apend"></param>
        /// <returns></returns>
        StreamWriter Open(string path, bool apend = false);
    }
}
