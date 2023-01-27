using System.Runtime.Serialization;

namespace Wada.UseCase.DataClass
{
    public class DirectedOperationNotFoundException : Exception
    {
        public DirectedOperationNotFoundException()
        {
        }

        public DirectedOperationNotFoundException(string? message) : base(message)
        {
        }

        public DirectedOperationNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DirectedOperationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
