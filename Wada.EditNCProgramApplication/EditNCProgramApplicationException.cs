using System.Runtime.Serialization;

namespace Wada.EditNcProgramApplication
{
    [Serializable]
    public class EditNcProgramApplicationException : Exception
    {
        public EditNcProgramApplicationException()
        {
        }

        public EditNcProgramApplicationException(string? message) : base(message)
        {
        }

        public EditNcProgramApplicationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EditNcProgramApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}