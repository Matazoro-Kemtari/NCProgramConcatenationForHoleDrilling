using System.Runtime.Serialization;

namespace Wada.ReadSubNcProgramApplication
{
    [Serializable]
    public class ReadSubNcProgramApplicationException : Exception
    {
        public ReadSubNcProgramApplicationException()
        {
        }

        public ReadSubNcProgramApplicationException(string? message) : base(message)
        {
        }

        public ReadSubNcProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadSubNcProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}