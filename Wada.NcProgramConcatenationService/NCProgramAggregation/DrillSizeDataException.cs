using System.Runtime.Serialization;

namespace Wada.NcProgramConcatenationService.NcProgramAggregation
{
    public class DrillSizeDataException : DomainException
    {
        public DrillSizeDataException()
        {
        }

        public DrillSizeDataException(string? message) : base(message)
        {
        }

        public DrillSizeDataException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DrillSizeDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
