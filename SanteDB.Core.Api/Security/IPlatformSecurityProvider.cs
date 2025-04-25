/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Security related methods implemented for a particular platform.
    /// </summary>
    public interface IPlatformSecurityProvider
    {
        /// <summary>
        /// Checks if an assembly is trusted by the platform.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        bool IsAssemblyTrusted(Assembly assembly);
        /// <summary>
        /// Checks if a certificate is trusted by the platform.
        /// </summary>
        /// <param name="certificate">The certificate to validate trust for.</param>
        /// <param name="asOfDate">The date to use to validate the validiy of the certificate of. Use this for example to validate when a package was assembled.</param>
        /// <returns>True if the platform considers the certificate trusted. False otherwise.</returns>
        /// <remarks>
        ///     This method does not attempt to define what trusted is simply by the method signature. The exact definition of trust is outside the scope of the api. 
        /// </remarks>
        bool IsCertificateTrusted(X509Certificate2 certificate, DateTimeOffset? asOfDate = null);
        /// <summary>
        /// Install a certificate into a platform store.
        /// </summary>
        /// <param name="certificate">The certificate to install in the store.</param>
        /// <param name="storeName">The store name to install the certificate in. Defaults to <see cref="StoreName.My"/>.</param>
        /// <param name="storeLocation">The store location to install the certificate in. Defaults to <see cref="StoreLocation.CurrentUser"/></param>
        /// <returns>True if the certificate was installed in the store successfully. False otherwise.</returns>
        /// <remarks>
        ///     <paramref name="storeName"/> and <paramref name="storeLocation"/> may be ignored on platforms that do not support windows-style certificate stores like Linux and Android.
        /// </remarks>
        bool TryInstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser);
        /// <summary>
        /// Remove a certificate from a platform store.
        /// </summary>
        /// <param name="certificate">The certificate to try to remove from the platform.</param>
        /// <param name="storeName">The store name to remove the certificate from. Defaults to <see cref="StoreName.My"/>.</param>
        /// <param name="storeLocation">The store location to remove the certificate from. Defaults to <see cref="StoreLocation.CurrentUser"/>.</param>
        /// <returns>True if the certificate was removed from the location, False otherwise.</returns>
        /// <remarks>
        ///     <paramref name="storeName"/> and <paramref name="storeLocation"/> may be ignored on platforms that do not support windows-style certificate stores like Linux and Android.
        /// </remarks>
        bool TryUninstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType">The search type to perform.</param>
        /// <param name="findValue">The search term to use to find the certificate.</param>
        /// <param name="certificate">When a certificate is found, it will be returned with this parameter and the result is <c>True</c>. This parameter is set to null if the result is <c>False</c>.</param>
        /// <param name="validOnly">True if only valid certificates should be returned</param>
        /// <returns>True if a certificate was found, False otherwise.</returns>
        bool TryGetCertificate(X509FindType findType, object findValue, out X509Certificate2 certificate, bool validOnly = false);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType">The search type to perform.</param>
        /// <param name="findValue">The search term to use to find the certificate.</param>
        /// <param name="storeName">The store name to search for the certificate in. Defaults to <see cref="StoreName.My"/>.</param>
        /// <param name="certificate">When a certificate is found, it will be returned with this parameter and the result is <c>True</c>. This parameter is set to null if the result is <c>False</c>.</param>
        /// <param name="validOnly">True if only valid certificates should be returned</param>
        /// <returns>True if a certificate was found, False otherwise.</returns>
        /// <remarks>
        ///     <paramref name="storeName"/> may be ignored on platforms that do not support windows-style certificate stores like Linux and Android.
        /// </remarks>
        bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, out X509Certificate2 certificate, bool validOnly = false);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType">The search type to perform.</param>
        /// <param name="findValue">The search term to use to find the certificate.</param>
        /// <param name="storeName">The store name to search for the certificate in. Defaults to <see cref="StoreName.My"/>.</param>
        /// <param name="storeLocation">The store location to search for the certificate in. Defaults to <see cref="StoreLocation.CurrentUser"/>.</param>
        /// <param name="certificate">When a certificate is found, it will be returned with this parameter and the result is <c>True</c>. This parameter is set to null if the result is <c>False</c>.</param>
        /// <param name="validOnly">True if only valid certificates should be returned</param>
        /// <returns>True if a certificate was found, False otherwise.</returns>
        /// <remarks>
        ///     <paramref name="storeName"/> and <paramref name="storeLocation"/> may be ignored on platforms that do not support windows-style certificate stores like Linux and Android.
        /// </remarks>
        bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate, bool validOnly = false);
        /// <summary>
        /// Find all certificates using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType">The search type to perform.</param>
        /// <param name="findValue">The search term to use to find the certificate.</param>
        /// <param name="storeName">The store name to search for the certificate in. Defaults to <see cref="StoreName.My"/>.</param>
        /// <param name="storeLocation">The store location to search for the certificate in. Defaults to <see cref="StoreLocation.CurrentUser"/>.</param>
        /// <param name="validOnly"><c>True</c> to only return certificates that are considered valid. Invalid certificates are ones that do not have a verifiable chain of trust, are expired, or contain invalid extensions.</param>
        /// <returns>An enumerable collection of any certificates found, or an empty result set.</returns>
        /// <remarks>
        ///     <paramref name="storeName"/> and <paramref name="storeLocation"/> may be ignored on platforms that do not support windows-style certificate stores like Linux and Android.
        /// </remarks>
        IEnumerable<X509Certificate2> FindAllCertificates(X509FindType findType, object findValue, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser, bool validOnly = true);
    }
}
