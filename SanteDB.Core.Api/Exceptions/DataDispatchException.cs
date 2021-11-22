using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// An exception related to the dispatching of data
    /// </summary>
    public class DataDispatchException : Exception
    {
        /// <summary>
        /// Create new dispatch exception with the specified <paramref name="message"/>
        /// </summary>
        public DataDispatchException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create new dispatch exception with specified <paramref name="message"/> caused by <paramref name="innerException"/>
        /// </summary>
        public DataDispatchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}