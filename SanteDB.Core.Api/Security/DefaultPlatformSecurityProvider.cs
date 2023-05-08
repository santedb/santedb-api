using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
        public IEnumerable<X509Certificate2> FindAllCertificates(X509FindType findType, object findValue, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser, bool validOnly = true)
        {
            using(var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                foreach(var cert in store.Certificates.Find(findType, findValue, validOnly))
                {
                    yield return cert;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsAssemblyTrusted(Assembly assembly)
        {
            assembly.ValidateCodeIsSigned(false); // will throw if not valid
            return true;
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, out X509Certificate2 certificate)
        {
            return TryGetCertificate(findType, findValue, StoreName.My, StoreLocation.CurrentUser, out certificate);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, out X509Certificate2 certificate)
        {
            return TryGetCertificate(findType, findValue, storeName, StoreLocation.CurrentUser, out certificate);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate)
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

                    var certs = store.Certificates.Find(findType, findValue, false); // since the user is asking for a specific certificate allow for searching of invalid certificates

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
            var audit = this.AuditCertificateInstallation(certificate);

            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var password = Guid.NewGuid().ToString();

                    var certtext = certificate.Export(X509ContentType.Pfx, password);

                    var importcert = new X509Certificate2(certtext, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                    store.Add(importcert);

                    this.m_tracer.TraceWarning("Certificate {0} has been installed to {1}/{2}", certificate.Subject, storeLocation, storeName);
                    audit?.WithOutcome(OutcomeIndicator.Success);

                    store.Close();

                    return true;
                }
            }
            catch (CryptographicException cex)
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
            catch (CryptographicException cex)
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
            => ApplicationServiceContext.Current.GetService<IAuditService>()?.Audit() // Prevents circular dependency in dCDR
                .WithTimestamp()
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
            => ApplicationServiceContext.Current.GetService<IAuditService>()?.Audit()
                .WithTimestamp()
                .WithEventType(EventTypeCodes.SecurityAlert)
                .WithEventIdentifier(Model.Audit.EventIdentifierType.SecurityAlert)
                .WithAction(Model.Audit.ActionType.Delete)
                .WithLocalDestination()
                .WithPrincipal()
                .WithSystemObjects(Model.Audit.AuditableObjectRole.SecurityResource, Model.Audit.AuditableObjectLifecycle.PermanentErasure, certificate);

    }
}
