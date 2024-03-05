/*
 * Copyright (C) 2021 - 2024, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2023-6-21
 */
using System;
using System.Runtime.Serialization;

namespace SanteDB.Core.Exceptions
{
    /// <summary>
    /// Thrown when an identity does not have a secret available.
    /// </summary>
    public class MissingTfaSecretException : ApplicationException
    {
        /// <summary>
        /// Creates a new missing TFA secret exception
        /// </summary>
        public MissingTfaSecretException()
        {
        }

        /// <summary>
        /// Creates a new missing TFA secret exception with the specified <paramref name="message"/>
        /// </summary>
        /// <param name="message">The message to include in the exception</param>
        public MissingTfaSecretException(string message) : base(message)
        {
        }

        /// <summary>
        /// Create the new <see cref="MissingTfaSecretException"/> with the specified <paramref name="message"/> caused by the <paramref name="innerException"/>
        /// </summary>
        /// <param name="message">The message to include in the exception</param>
        /// <param name="innerException">The exception which caused this exception to be thrown</param>
        public MissingTfaSecretException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Create a new <see cref="MissingTfaSecretException"/> 
        /// </summary>
        /// <param name="info">The serialization information which is included in the message</param>
        /// <param name="context">The streaming context to include</param>
        protected MissingTfaSecretException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
