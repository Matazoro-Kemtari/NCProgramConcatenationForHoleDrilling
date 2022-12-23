using System;
using System.Runtime.Serialization;

namespace Wada.NCProgramConcatenationForHoleDrilling
{
    [Serializable]
    internal class NCProgramConcatenationForHoleDrillingException : Exception
    {
        public NCProgramConcatenationForHoleDrillingException()
        {
        }

        public NCProgramConcatenationForHoleDrillingException(string message) : base(message)
        {
        }

        public NCProgramConcatenationForHoleDrillingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NCProgramConcatenationForHoleDrillingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}