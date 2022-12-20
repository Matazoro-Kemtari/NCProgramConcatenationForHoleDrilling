using Wada.AOP.Logging;
using Wada.NCProgramConcatenationService;

namespace Wada.NCProgramFile
{
    public class StreamReaderOpener : IStreamReaderOpener
    {
        [Logging]
        public StreamReader Open(string path)
        {
            StreamReader reader = new(path);

            return reader;
        }
    }
}
