using System.Runtime.Serialization;

namespace Wada.NcProgramConcatenationService
{
    [Serializable]
    public class OpenFileStreamException : Exception
    {
        public OpenFileStreamException()
        {
        }

        public OpenFileStreamException(string? message) : base(message)
        {
        }

        public OpenFileStreamException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected OpenFileStreamException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}