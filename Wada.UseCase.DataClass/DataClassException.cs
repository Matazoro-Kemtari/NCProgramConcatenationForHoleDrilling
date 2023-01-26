using System.Runtime.Serialization;

namespace Wada.UseCase.DataClass
{
    [Serializable]
    internal class DataClassException : Exception
    {
        public DataClassException()
        {
        }

        public DataClassException(string? message) : base(message)
        {
        }

        public DataClassException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DataClassException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}