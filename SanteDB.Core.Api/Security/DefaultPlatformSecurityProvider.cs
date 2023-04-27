using SanteDB.Core.Model.Audit;
using SanteDB.Core.Security.Audit;
using System;
using System.Collections.Generic;
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
        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, string findValue, out X509Certificate2 certificate)
        {
            return TryGetCertificate(findType, findValue, StoreName.My, StoreLocation.CurrentUser, out certificate);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, string findValue, StoreName storeName, out X509Certificate2 certificate)
        {
            return TryGetCertificate(findType, findValue, storeName, StoreLocation.CurrentUser, out certificate);
        }

        ///<inheritdoc />
        public bool TryGetCertificate(X509FindType findType, string findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate)
        {
            if (string.IsNullOrEmpty(findValue))
            {
                throw new ArgumentNullException(nameof(findValue));
            }

            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadOnly);

                    var certs = store.Certificates.Find(findType, findValue, true);

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
            var audit = X509CertificateUtils.AuditCertificateInstallation(certificate);

            try
            {
                using (var store = new X509Store(storeName, storeLocation))
                {
                    store.Open(OpenFlags.ReadWrite);

                    var password = Guid.NewGuid().ToString();

                    var certtext = certificate.Export(X509ContentType.Pfx, password);

                    var importcert = new X509Certificate2(certtext, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                    store.Add(importcert);

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
            var audit = X509CertificateUtils.AuditCertificateRemoval(certificate);

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
        public bool VerifyTrustForAppletCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc />
        public bool VerifyTrustForCodeCertificate(X509Certificate2 certificate)
        {
            throw new NotImplementedException();
        }
    }
}
