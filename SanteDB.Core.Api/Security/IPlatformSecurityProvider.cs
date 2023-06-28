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
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool IsCertificateTrusted(X509Certificate2 certificate);
        /// <summary>
        /// Install a certificate into a platform store.
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <returns></returns>
        bool TryInstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser);
        /// <summary>
        /// Remove a certificate from a platform store.
        /// </summary>
        /// <param name="certificate"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <returns></returns>
        bool TryUninstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool TryGetCertificate(X509FindType findType, object findValue, out X509Certificate2 certificate);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="storeName"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, out X509Certificate2 certificate);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate);
        /// <summary>
        /// Find all certificates 
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <returns></returns>
        IEnumerable<X509Certificate2> FindAllCertificates(X509FindType findType, object findValue, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser, bool validOnly = true);
    }
}
