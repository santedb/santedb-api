﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Security related methods implemented for a particular platform.
    /// </summary>
    public interface IPlatformSecurityProvider
    {
        /// <summary>
        /// Verifies the trust of a certificate that is used for applet signing.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool VerifyTrustForAppletCertificate(X509Certificate2 certificate);
        /// <summary>
        /// Verifies the trust of a certificate that is used for code signing.
        /// </summary>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool VerifyTrustForCodeCertificate(X509Certificate2 certificate);
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
        bool TryGetCertificate(X509FindType findType, string findValue, out X509Certificate2 certificate);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="storeName"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool TryGetCertificate(X509FindType findType, string findValue, StoreName storeName, out X509Certificate2 certificate);
        /// <summary>
        /// Find a certificate using <paramref name="findType"/> and <paramref name="findValue"/>.
        /// </summary>
        /// <param name="findType"></param>
        /// <param name="findValue"></param>
        /// <param name="storeName"></param>
        /// <param name="storeLocation"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        bool TryGetCertificate(X509FindType findType, string findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate);
    }
}
