using System.Runtime.Serialization;
using Wada.NcProgramConcatenationService;

namespace Wada.ReadMainNcProgramApplication
{
    [Serializable]
    public class ReadMainNcProgramApplicationException : Exception
    {
        public ReadMainNcProgramApplicationException()
        {
        }

        public ReadMainNcProgramApplicationException(string? message) : base(message)
        {
        }

        public ReadMainNcProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadMainNcProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}