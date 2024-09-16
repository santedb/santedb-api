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
 */
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Http
{
    /// <summary>
    /// Fired when there are invalid certificate is encountered
    /// </summary>
    public interface ICertificateValidator
    {
        /// <summary>
        /// Determines if the remote certificate is valid
        /// </summary>
        /// <returns><c>true</c>, if certificate was validated, <c>false</c> otherwise.</returns>
        /// <param name="certificate">Certificate.</param>
        /// <param name="chain">Chain.</param>
        bool ValidateCertificate(X509Certificate2 certificate, X509Chain chain);
    }
}