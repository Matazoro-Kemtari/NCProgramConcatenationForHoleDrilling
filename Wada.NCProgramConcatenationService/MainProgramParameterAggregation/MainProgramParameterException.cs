using System.Runtime.Serialization;

namespace Wada.NCProgramConcatenationService.MainProgramParameterAggregation
{
    public class MainProgramParameterException : DomainException
    {
        public MainProgramParameterException()
        {
        }

        public MainProgramParameterException(string? message) : base(message)
        {
        }

        public MainProgramParameterException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MainProgramParameterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
