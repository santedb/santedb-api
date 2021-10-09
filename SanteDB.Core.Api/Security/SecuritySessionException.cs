/*
 * Copyright (C) 2021 - 2021, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: fyfej
 * Date: 2021-8-5
 */
using System;
using System.Security;

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
        TokenType,
        /// <summary>
        /// Token signature validation error
        /// </summary>
        SignatureFailure
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
