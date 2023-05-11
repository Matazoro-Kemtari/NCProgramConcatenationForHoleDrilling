using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;

namespace Wada.NcProgramFile
{
    public class StreamWriterOpener : IStreamWriterOpener
    {
        [Logging]
        public StreamWriter Open(string path, bool apend = false)
        {
            StreamWriter writer;
            try
            {
                writer = new(path, apend);
            }
            catch (DirectoryNotFoundException ex)
            {
                string msg = "フォルダが見つかりません";
                throw new OpenFileStreamReaderException(msg, ex);
            }
            catch (IOException ex)
            {
                string msg = "ファイルが使用中です";
                throw new OpenFileStreamReaderException(msg, ex);
            }

            return writer;
        }
    }
}
