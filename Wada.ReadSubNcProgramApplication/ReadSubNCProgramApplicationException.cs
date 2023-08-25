using System.Runtime.Serialization;

namespace Wada.ReadSubNCProgramApplication
{
    [Serializable]
    public class ReadSubNCProgramApplicationException : Exception
    {
        public ReadSubNCProgramApplicationException()
        {
        }

        public ReadSubNCProgramApplicationException(string? message) : base(message)
        {
        }

        public ReadSubNCProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ReadSubNCProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}