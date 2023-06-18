/*
 * Copyright (C) 2021 - 2023, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-5-19
 */
using System;
using System.Runtime.Serialization;
using System.Security.Authentication;

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
