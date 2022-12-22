using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Thrown when an identity does not have a secret available.
    /// </summary>
    public class MissingTfaSecretException : ApplicationException
    {
        public MissingTfaSecretException()
        {
        }

        public MissingTfaSecretException(string message) : base(message)
        {
        }

        public MissingTfaSecretException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingTfaSecretException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
