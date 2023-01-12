namespace Wada.NCProgramConcatenationService
{
    public interface IStreamOpener
    {
        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Stream Open(string path);
    }
}
