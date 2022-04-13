using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace SanteDB.Core.Security
{

    /// <summary>
    /// An <see cref="IIdentityProviderService"/> which uses certificate authentication services
    /// </summary>
    public interface ICertificateIdentityProvider
    {

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
