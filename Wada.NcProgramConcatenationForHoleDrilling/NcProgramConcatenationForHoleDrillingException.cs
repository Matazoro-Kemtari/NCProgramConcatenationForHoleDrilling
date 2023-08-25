using System;
using System.Runtime.Serialization;

namespace Wada.NcProgramConcatenationForHoleDrilling
{
    [Serializable]
    internal class NcProgramConcatenationForHoleDrillingException : Exception
    {
        public NcProgramConcatenationForHoleDrillingException()
        {
        }

        public NcProgramConcatenationForHoleDrillingException(string? message) : base(message)
        {
        }

        public NcProgramConcatenationForHoleDrillingException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected NcProgramConcatenationForHoleDrillingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}