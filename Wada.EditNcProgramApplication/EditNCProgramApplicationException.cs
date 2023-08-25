using System.Runtime.Serialization;

namespace Wada.EditNCProgramApplication
{
    [Serializable]
    public class EditNCProgramApplicationException : Exception
    {
        public EditNCProgramApplicationException()
        {
        }

        public EditNCProgramApplicationException(string? message) : base(message)
        {
        }

        public EditNCProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EditNCProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}