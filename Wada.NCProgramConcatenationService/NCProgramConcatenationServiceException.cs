using System.Runtime.Serialization;

namespace Wada.NCProgramConcatenationService
{
    [Serializable]
    public class NCProgramConcatenationServiceException : Exception
    {
        public NCProgramConcatenationServiceException()
        {
        }

        public NCProgramConcatenationServiceException(string? message) : base(message)
        {
        }

        public NCProgramConcatenationServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NCProgramConcatenationServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}