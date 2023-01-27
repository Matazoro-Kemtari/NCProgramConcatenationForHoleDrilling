using System.Runtime.Serialization;
using Wada.NCProgramConcatenationService;

namespace Wada.ReadMainNCProgramApplication
{
    [Serializable]
    public class ReadMainNCProgramApplicationException : Exception
    {
        public ReadMainNCProgramApplicationException()
        {
        }

        public ReadMainNCProgramApplicationException(string? message) : base(message)
        {
        }

        public ReadMainNCProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadMainNCProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}