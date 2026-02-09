/*
 * Copyright (C) 2021 - 2026, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Platform-agnostic security provider. This provider is intended to provide common functionality across all platforms according to the .net platform capabilites.
    /// </summary>
    /// <remarks>
    /// Depending on the platform, some of these methods may not work and may require platform-specific implementations.
    /// </remarks>
    public class DefaultPlatformSecurityProvider : IPlatformSecurityProvider
    {

        private Tracer m_tracer = Tracer.GetTracer(typeof(DefaultPlatformSecurityProvider));

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultPlatformSecurityProvider()
        {
            if (Type.GetType("Mono.Runtime") != null)
            {
                this.m_tracer.TraceWarning("The DefaultPlatformSecurityProvider has known issues with Mono!");
            }

        }

        /// <inheritdoc/>
        /// <remarks>This is not required on Windows or Linux</remarks>
        public bool DemandPlatformServicePermission(PlatformServicePermission platformServicePermission) => true;

        /// <inheritdoc/>
        public IEnumerable<X509Certificate2> FindAllCertificates(X509FindType findType, object findValue, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser, bool validOnly = true)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                foreach (var cert in store.Certificates.Find(findType, findValue, validOnly))
                {
                    yield return cert;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsAssemblyTrusted(Assembly assembly)
        {
            assembly?.ValidateCodeIsSigned(false); // will throw if not valid
            return true;
        }

        /// <inheritdoc/>
        public bool IsCertificateTrusted(X509Certificate2 certificate, DateTimeOffset? asOfDate = null)
        {
            return certificate?.IsTrustedIntern(new X509Certificate2Collection(), asOfDate?.DateTime, out _) == true;
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, out X509Certificate2 certificate, bool validOnly = false)
        {
            return TryGetCertificate(findType, findValue, StoreName.My, StoreLocation.CurrentUser, out certificate, validOnly);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, out X509Certificate2 certificate, bool validOnly = false)
        {
            return TryGetCertificate(findType, findValue, storeName, StoreLocation.CurrentUser, out certificate, validOnly);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate, bool validOnly = false)
        {
            if (findValue == null)
            {
                throw new ArgumentNullException(nameof(findValue));
            }

            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadOnly);

                    var certs = store.Certificates.Find(findType, findValue, validOnly: validOnly); // since the user is asking for a specific certificate allow for searching of invalid certificates

                    if (certs.Count == 0)
                    {
                        certificate = null;
                        return false;
                    }

                    certificate = certs[0];

                    store.Close();

                    return true;
                }
            }
            catch (CryptographicException)
            {
                certificate = null;
                return false;
            }
        }

        ///<inheritdoc />
        public bool TryInstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            this.m_tracer.TraceInfo("Installing certificate {0} to {1}/{2}", certificate.Thumbprint, storeLocation, storeName);

            var audit = this.AuditCertificateInstallation(certificate);

#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var password = Guid.NewGuid().ToString();

                    var certtext = certificate.Export(X509ContentType.Pfx, password);

                    X509Certificate2 importcert = null;
                    if (storeLocation == StoreLocation.LocalMachine)
                    {
                        importcert = new X509Certificate2(certtext, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
                    }
                    else
                    {
                        importcert = new X509Certificate2(certtext, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                    }

                    store.Add(importcert);

                    this.m_tracer.TraceWarning("Certificate {0} has been installed to {1}/{2}", certificate.Subject, storeLocation, storeName);
                    audit?.WithOutcome(OutcomeIndicator.Success);

                    store.Close();

                    return true;
                }
            }
            catch (CryptographicException cex)
            {
                this.m_tracer.TraceWarning("Could not install {0} to {1}/{2} - {3}", certificate.Subject, storeLocation, storeName, cex);
                audit?.WithOutcome(OutcomeIndicator.SeriousFail);
                return false;
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                audit?.WithOutcome(Model.Audit.OutcomeIndicator.SeriousFail);
                throw;
            }
            finally
            {
                if (AuthenticationContext.Current.Principal != AuthenticationContext.SystemPrincipal)
                {
                    audit?.Send();
                }
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }

        ///<inheritdoc />
        public bool TryUninstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            var audit = this.AuditCertificateRemoval(certificate);

            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var thumbprint = certificate?.Thumbprint;

                    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);

                    if (certs.Count == 0)
                    {
                        audit?.WithOutcome(OutcomeIndicator.MinorFail);
                        return false;
                    }

                    foreach (var cert in certs)
                    {
                        store.Certificates.Remove(cert);
                    }
                    this.m_tracer.TraceWarning("Certificate {0} has been removed from {1}/{2}", certificate.Subject, storeLocation, storeName);

                    audit?.WithOutcome(OutcomeIndicator.Success);

                    store.Close();

                    return true;
                }
            }
            catch (CryptographicException)
            {
                audit?.WithOutcome(OutcomeIndicator.SeriousFail);
                return false;
            }
            catch (Exception ex) when (!(ex is StackOverflowException || ex is OutOfMemoryException))
            {
                audit?.WithOutcome(Model.Audit.OutcomeIndicator.SeriousFail);
                throw;
            }
            finally
            {
                audit?.Send();
            }
        }


        /// <summary>
        /// Create an audit builder for certificate installation.
        /// </summary>
        /// <param name="certificate">The certificate being installed.</param>
        /// <returns></returns>
        private IAuditBuilder AuditCertificateInstallation(X509Certificate2 certificate)
            => ApplicationServiceContext.Current?.GetAuditService()?.Audit() // Prevents circular dependency in dCDR
                .WithTimestamp()
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)

                .WithEventType(EventTypeCodes.SecurityAlert)
                .WithEventIdentifier(Model.Audit.EventIdentifierType.Import)
                .WithAction(Model.Audit.ActionType.Execute)
                .WithLocalDestination()
                .WithPrincipal()
                .WithSystemObjects(Model.Audit.AuditableObjectRole.SecurityResource, Model.Audit.AuditableObjectLifecycle.Import, certificate);

        /// <summary>
        /// Create an audit builder for certificate removal.
        /// </summary>
        /// <param name="certificate">The certificate being removed.</param>
        /// <returns></returns>
        private IAuditBuilder AuditCertificateRemoval(X509Certificate2 certificate)
            => ApplicationServiceContext.Current?.GetAuditService()?.Audit()
                .WithTimestamp()
                .WithSensitivity(Core.Model.Attributes.ResourceSensitivityClassification.Administrative)
                .WithEventType(EventTypeCodes.SecurityAlert)
                .WithEventIdentifier(Model.Audit.EventIdentifierType.SecurityAlert)
                .WithAction(Model.Audit.ActionType.Delete)
                .WithLocalDestination()
                .WithPrincipal()
                .WithSystemObjects(Model.Audit.AuditableObjectRole.SecurityResource, Model.Audit.AuditableObjectLifecycle.PermanentErasure, certificate);

    }
}
