namespace Wada.NCProgramConcatenationService
{
    public interface IStreamReaderOpener
    {
        /// <summary>
        /// ストリームリーダーを開く
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        StreamReader Open(string path);
    }
}
