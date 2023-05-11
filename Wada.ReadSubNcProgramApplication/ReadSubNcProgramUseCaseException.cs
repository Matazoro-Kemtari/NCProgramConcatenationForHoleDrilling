using System.Runtime.Serialization;

namespace Wada.ReadSubNcProgramApplication
{
    [Serializable]
    public class ReadSubNcProgramUseCaseException : Exception
    {
        public ReadSubNcProgramUseCaseException()
        {
        }

        public ReadSubNcProgramUseCaseException(string? message) : base(message)
        {
        }

        public ReadSubNcProgramUseCaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadSubNcProgramUseCaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}