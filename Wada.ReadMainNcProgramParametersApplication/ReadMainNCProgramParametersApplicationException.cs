using System.Runtime.Serialization;

namespace Wada.ReadMainNCProgramParametersApplication
{
    [Serializable]
    public class ReadMainNCProgramParametersApplicationException : Exception
    {
        public ReadMainNCProgramParametersApplicationException()
        {
        }

        public ReadMainNCProgramParametersApplicationException(string? message) : base(message)
        {
        }

        public ReadMainNCProgramParametersApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadMainNCProgramParametersApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}