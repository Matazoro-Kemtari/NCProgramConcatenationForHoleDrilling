using System.Runtime.Serialization;

namespace Wada.NCProgramConcatenationService
{
    [Serializable]
    public class DirectedOperationToolDiameterNotFoundException : Exception
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