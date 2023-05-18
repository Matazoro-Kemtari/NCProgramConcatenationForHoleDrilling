using System.Runtime.Serialization;

namespace Wada.EditNcProgramApplication
{
    [Serializable]
    public class EditNcProgramUseCaseException : Exception
    {
        public EditNcProgramUseCaseException()
        {
        }

        public EditNcProgramUseCaseException(string? message) : base(message)
        {
        }

        public EditNcProgramUseCaseException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected EditNcProgramUseCaseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}