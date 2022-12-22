using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Authentication;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// An <see cref="AuthenticationException"/> that is thrown when an attempt to authenticate a principal requires a second factor for auth.
    /// </summary>
    public class TfaRequiredAuthenticationException : AuthenticationException
    {

        /// <inheritdoc />
        public TfaRequiredAuthenticationException()
        {
        }

        /// <inheritdoc />
        public TfaRequiredAuthenticationException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public TfaRequiredAuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected TfaRequiredAuthenticationException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
