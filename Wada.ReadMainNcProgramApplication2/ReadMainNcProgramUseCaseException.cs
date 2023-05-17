using System.Runtime.Serialization;
using Wada.NcProgramConcatenationService;

namespace Wada.ReadMainNcProgramApplication
{
    [Serializable]
    public class ReadMainNcProgramUseCaseException : Exception
    {
        public ReadMainNcProgramUseCaseException()
        {
        }

        public ReadMainNcProgramUseCaseException(string? message) : base(message)
        {
        }

        public ReadMainNcProgramUseCaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadMainNcProgramUseCaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}