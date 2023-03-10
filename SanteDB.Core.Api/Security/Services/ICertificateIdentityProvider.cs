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
 * Date: 2023-3-10
 */
using SanteDB.Core.Security.Principal;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{

    /// <summary>
    /// An <see cref="IIdentityProviderService"/> which uses certificate authentication services
    /// </summary>
    public interface ICertificateIdentityProvider
    {


        /// <summary>
        /// Fired after an authentication request has been made.
        /// </summary>
        event EventHandler<AuthenticatedEventArgs> Authenticated;

        /// <summary>
        /// Fired prior to an authentication request being made.
        /// </summary>
        event EventHandler<AuthenticatingEventArgs> Authenticating;

        /// <summary>
        /// Authenticates a <see cref="IPrincipal"/> instance based on the certificate mapping 
        /// for <paramref name="authenticationCertificate"/>
        /// </summary>
        /// <remarks>Implementations of this interface should validate that the certificate is authenticate and 
        /// that it is not revoked.</remarks>
        /// <param name="authenticationCertificate">The public key which is used to authenticate the principal</param>
        /// <returns>The authenticated prinicpal</returns>
        IPrincipal Authenticate(X509Certificate2 authenticationCertificate);

        /// <summary>
        /// Maps <paramref name="identityToBeMapped"/> to <paramref name="authenticationCertificate"/>
        /// so that calls to <see cref="Authenticate(X509Certificate2)"/> may establish security principals
        /// </summary>
        /// <param name="authenticationCertificate">The authentication certificate to associated with <paramref name="identityToBeMapped"/></param>
        /// <param name="identityToBeMapped">The identity (either a user, <see cref="IApplicationIdentity"/> or <see cref="IDeviceIdentity"/> to be mapped)</param>
        /// <param name="authenticatedPrincipal">The prinicpal which is assigning this association</param>
        void AddIdentityMap(IIdentity identityToBeMapped, X509Certificate2 authenticationCertificate, IPrincipal authenticatedPrincipal);

        /// <summary>
        /// Removes the certificate mapping between <paramref name="identityToBeUnMapped"/> and 
        /// <paramref name="authenticationCertificate"/>
        /// </summary>
        /// <param name="identityToBeUnMapped">The identity which is being removed from the certificate mapping</param>
        /// <param name="authenticationCertificate">The authentication certificate to remove</param>
        /// <param name="authenticatedPrincipal">The principal which is removing the certificate mapping</param>
        bool RemoveIdentityMap(IIdentity identityToBeUnMapped, X509Certificate2 authenticationCertificate, IPrincipal authenticatedPrincipal);

        /// <summary>
        /// Get the <see cref="X509Certificate2"/> which has been mapped to <paramref name="identityOfCertificte"/>
        /// </summary>
        /// <param name="identityOfCertificte">The identity for which the certificate should be retrieved</param>
        /// <returns>The <see cref="X509Certificate2"/> which was mapped to <paramref name="identityOfCertificte"/> or null if none exists</returns>
        X509Certificate2 GetIdentityCertificate(IIdentity identityOfCertificte);

        /// <summary>
        /// Gets an un-authenticated identity object for <paramref name="authenticationCertificate"/>
        /// </summary>
        /// <param name="authenticationCertificate">The authentication certificate</param>
        /// <returns>The unauthenticated identity</returns>
        IIdentity GetCertificateIdentity(X509Certificate2 authenticationCertificate);
    }
}
