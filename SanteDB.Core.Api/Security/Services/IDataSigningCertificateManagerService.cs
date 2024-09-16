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
using SanteDB.Core.Model.Query;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which manages associations between various identities 
    /// </summary>
    public interface IDataSigningCertificateManagerService : IServiceImplementation
    {
        /// <summary>
        /// Adds a signing certificate to the identity
        /// </summary>
        /// <param name="identity">The identity to add signing credentials for</param>
        /// <param name="x509Certificate">The certificate name</param>
        /// <param name="principal">The principal performing the operation</param>
        void AddSigningCertificate(IIdentity identity, X509Certificate2 x509Certificate, IPrincipal principal);

        /// <summary>
        /// Removes a signing certificate from an identity
        /// </summary>
        /// <param name="identity">The identity to remove the signing credentials for</param>
        /// <param name="x509Certificate">The certificate to remove</param>
        /// <param name="principal">The principal performing the operation</param>
        void RemoveSigningCertificate(IIdentity identity, X509Certificate2 x509Certificate, IPrincipal principal);

        /// <summary>
        /// Gets signing certificates associated with <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The identity for which signing credentials should be obtained</param>
        /// <returns>Registered signing credentials</returns>
        IEnumerable<X509Certificate2> GetSigningCertificates(IIdentity identity);

        /// <summary>
        /// Get all the signing certificates that are registered for <paramref name="classOfIdentity"/>
        /// </summary>
        /// <param name="classOfIdentity">The classification of the identities for which certificates should be retrieved</param>
        /// <param name="filter">The query expression to filter</param>
        /// <returns>A query result set of all the signing certificates</returns>
        IEnumerable<X509Certificate2> GetSigningCertificates(Type classOfIdentity, NameValueCollection filter);
        
        /// <summary>
        /// Get any configured signing certificate based on the thumbprint
        /// </summary>
        bool TryGetSigningCertificateByThumbprint(String x509Thumbprint, out X509Certificate2 certificate);

        /// <summary>
        /// Get configured certificate by hash
        /// </summary>
        bool TryGetSigningCertificateByHash(byte[] x509hash, out X509Certificate2 certificate);

        /// <summary>
        /// Get all associated identities for the provided <paramref name="certificate"/>
        /// </summary>
        /// <param name="certificate">The certificate for which there is an identity assigned</param>
        /// <returns>The known identities for the certificate</returns>
        IEnumerable<IIdentity> GetCertificateIdentities(X509Certificate2 certificate);

    }
}
