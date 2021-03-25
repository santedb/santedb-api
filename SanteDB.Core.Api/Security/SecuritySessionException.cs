using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace SanteDB.Core.Security
{

    /// <summary>
    /// Type of exception
    /// </summary>
    public enum SessionExceptionType
    {
        /// <summary>
        /// Session is not yet valid
        /// </summary>
        NotYetValid,
        /// <summary>
        /// Session has expired
        /// </summary>
        Expired,
        /// <summary>
        /// Session was not established
        /// </summary>
        NotEstablished,
        /// <summary>
        /// Session has invalid scope
        /// </summary>
        Scope,
        /// <summary>
        /// Other issue
        /// </summary>
        Other,
        /// <summary>
        /// Token is of invalid type
        /// </summary>
        TokenType
    }

    /// <summary>
    /// Represents a session exception
    /// </summary>
    public class SecuritySessionException : SecurityException
    {

        /// <summary>
        /// Creates a new security session exception
        /// </summary>
        public SecuritySessionException(SessionExceptionType type, ISession session, String message, Exception innerException) : base(message, innerException)
        {
            this.Type = type;
            this.Session = session;
        }

        /// <summary>
        /// Creates a new security session exception
        /// </summary>
        public SecuritySessionException(SessionExceptionType type, String message, Exception innerException) : this(type, null, message, innerException)
        {
        }

        /// <summary>
        /// Gets the type of exception 
        /// </summary>
        public SessionExceptionType Type { get; }

        /// <summary>
        /// Gets the impact session
        /// </summary>
        public ISession Session { get; }
    }
}
