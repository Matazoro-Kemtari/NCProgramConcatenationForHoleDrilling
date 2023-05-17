using System.Runtime.Serialization;

namespace Wada.ReadMainNcProgramParametersApplication
{
    [Serializable]
    public class ReadMainNcProgramParametersUseCaseException : Exception
    {
        public ReadMainNcProgramParametersUseCaseException()
        {
        }

        public ReadMainNcProgramParametersUseCaseException(string? message) : base(message)
        {
        }

        public ReadMainNcProgramParametersUseCaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadMainNcProgramParametersUseCaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}