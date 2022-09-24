using System;
using System.Collections.Generic;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// A specialized exception whenever a pre-condition for an operation has failed
    /// </summary>
    public class PreconditionFailedException : Exception
    {

        /// <inheritdoc/>
        public PreconditionFailedException()
        {

        }

        /// <inheritdoc/>
        public PreconditionFailedException(String message) : base(message)
        {

        }

        /// <inheritdoc/>
        public PreconditionFailedException(String message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
