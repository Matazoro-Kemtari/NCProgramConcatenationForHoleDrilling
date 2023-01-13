using System.Runtime.Serialization;

namespace Wada.NCProgramConcatenationService
{
    [Serializable]
    public class OpenFileStreamReaderException : Exception
    {
        public OpenFileStreamReaderException()
        {
        }

        public OpenFileStreamReaderException(string? message) : base(message)
        {
        }

        public OpenFileStreamReaderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected OpenFileStreamReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}