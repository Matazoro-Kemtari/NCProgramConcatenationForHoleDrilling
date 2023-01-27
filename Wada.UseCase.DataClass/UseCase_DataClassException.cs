using System.Runtime.Serialization;

namespace Wada.UseCase.DataClass
{
    [Serializable]
    public class UseCase_DataClassException : Exception
    {
        public UseCase_DataClassException()
        {
        }

        public UseCase_DataClassException(string? message) : base(message)
        {
        }

        public UseCase_DataClassException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected UseCase_DataClassException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}