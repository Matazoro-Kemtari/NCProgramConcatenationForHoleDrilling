using System.Runtime.Serialization;

namespace Wada.NcProgramConcatenationService
{
    [Serializable]
    public class DirectedOperationToolDiameterNotFoundException : DomainException
    {
        public DirectedOperationToolDiameterNotFoundException()
        {
        }

        public DirectedOperationToolDiameterNotFoundException(string? message) : base(message)
        {
        }

        public DirectedOperationToolDiameterNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DirectedOperationToolDiameterNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}