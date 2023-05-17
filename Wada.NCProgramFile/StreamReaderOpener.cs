using Wada.AOP.Logging;
using Wada.NcProgramConcatenationService;

namespace Wada.NcProgramFile
{
    public class StreamReaderOpener : IStreamReaderOpener
    {
        [Logging]
        public StreamReader Open(string path)
        {
            StreamReader reader;
            try
            {
                reader = new(path);
            }
            catch (FileNotFoundException ex)
            {
                string msg = $"ファイルが見つかりません ファイル: {path}";
                throw new OpenFileStreamReaderException(msg, ex);
            }
            catch (IOException ex)
            {
                string msg = $"ファイルが使用中です ファイル: {path}";
                throw new OpenFileStreamReaderException(msg, ex);
            }

            return reader;
        }
    }
}
