﻿using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// An implementation of the <see cref="IPlatformSecurityProvider"/> for mono which 
    /// handles the storage of certificates with private keys in a separate place
    /// </summary>
    public class MonoPlatformSecurityProvider : IPlatformSecurityProvider
    {

        // Tracer
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(MonoPlatformSecurityProvider));
        // Key store location
        private readonly String m_monoPrivateKeyStoreLocation;
        // Mono private key store
        private readonly ConcurrentDictionary<String, X509Certificate2> m_monoPrivateKeyStore = new ConcurrentDictionary<String, X509Certificate2>();
        private readonly IAuditService m_auditService;

        /// <summary>
        /// Mono platform security provider
        /// </summary>
        public MonoPlatformSecurityProvider(IAuditService auditService = null)
        {
            this.m_monoPrivateKeyStoreLocation = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString(), ".x509");
            this.InitializeMonoKeyStore();
            this.m_auditService = auditService;
            this.m_tracer.TraceWarning("!!!! WARNING: Using the Mono Certificate Manager Platform Service - there are additional security considerations that must be taken into" +
                " account when using X509 certificates with private keys in this context");
        }

        /// <summary>
        /// Compute a password for the file
        /// </summary>
        private String ComputePass(String targetFileName) =>
            SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(targetFileName)).HexEncode();

        /// <summary>
        /// Initialize the mono keystore
        /// </summary>
        private void InitializeMonoKeyStore()
        {
            if (!Directory.Exists(this.m_monoPrivateKeyStoreLocation))
            {
                Directory.CreateDirectory(this.m_monoPrivateKeyStoreLocation);
                var directoryInfo = new DirectoryInfo(this.m_monoPrivateKeyStoreLocation);
                directoryInfo.Attributes |= FileAttributes.Hidden | FileAttributes.Encrypted;
            }

            foreach (var k in Directory.EnumerateFiles(this.m_monoPrivateKeyStoreLocation, "*.pfx"))
            {
                try
                {
                    var cert = new X509Certificate2(k, this.ComputePass(k)); // TODO: Allow the configuration of a password from the launcher (docker, app, etc.)
                    if(cert.Thumbprint != Path.GetFileNameWithoutExtension(k))
                    {
                        throw new SecurityException($"{Path.GetFileNameWithoutExtension(k)}!={cert.Thumbprint}");
                    }
                    this.m_monoPrivateKeyStore.TryAdd(cert.Thumbprint, cert);
                }
                catch (Exception e)
                {
                    this.m_tracer.TraceError("Could not load PFX {0} - {1}", k, e.ToHumanReadableString());
                }
            }
        }

        /// <inheritdoc/>
        public bool IsAssemblyTrusted(Assembly assembly)
        {
            assembly.ValidateCodeIsSigned(false); // will throw
            return true;
        }

        /// <inheritdoc/>
        public bool TryGetCertificate(X509FindType findType, object findValue, out X509Certificate2 certificate) => this.TryGetCertificate(findType, findValue, StoreName.My, out certificate);

        /// <inheritdoc/>
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, out X509Certificate2 certificate) => this.TryGetCertificate(findType, findValue, storeName, StoreLocation.CurrentUser, out certificate);

        /// <inheritdoc/>
        public bool TryGetCertificate(X509FindType findType, object findValue, StoreName storeName, StoreLocation storeLocation, out X509Certificate2 certificate)
        {
            if(findValue == null)
            {
                throw new ArgumentNullException(nameof(findValue));
            }

            // Try to get the certificate from the underlying store 
            using (var store = new X509Store(storeName, StoreLocation.CurrentUser))
            {
                try
                {
                    store.Open(OpenFlags.ReadOnly);
                    var matches = store.Certificates.Find(findType, findValue, false);
                    if (matches.Count == 0)
                    {
                        // Look from PFX on the hard disk
                        if (storeName == StoreName.My && storeLocation == StoreLocation.CurrentUser)
                        {
                            switch(findType)
                            {
                                case X509FindType.FindByThumbprint:
                                    return this.m_monoPrivateKeyStore.TryGetValue(findValue.ToString(), out certificate);
                                case X509FindType.FindBySubjectDistinguishedName:
                                    certificate = this.m_monoPrivateKeyStore.FirstOrDefault(o => o.Value.Subject.Equals(findValue.ToString(), StringComparison.OrdinalIgnoreCase)).Value;
                                    return certificate != null;
                                case X509FindType.FindBySubjectName:
                                    certificate = this.m_monoPrivateKeyStore.FirstOrDefault(o => o.Value.Subject.ToLowerInvariant().Contains(findValue.ToString().ToLowerInvariant())).Value;
                                    return certificate != null;
                            }
                        }
                        certificate = null;
                        return false;
                    }
                    else if (matches.Count == 1)
                    {
                        certificate = matches[0];
                        return true;
                    }
                    else
                    {
                        throw new SecurityException(ErrorMessages.CERTIFICATE_NOT_FOUND);
                    }
                }
                catch (Exception ex)
                {
                    throw new SecurityException(ErrorMessages.CERTIFICATE_NOT_FOUND, ex);
                }
                finally
                {
                    store.Close();
                }
            }
        }

        /// <inheritdoc/>
        public bool TryInstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            var audit = this.AuditCertificateInstallation(certificate);
            try
            {
                // Does the certificate being installed have a private key?
                if (certificate.HasPrivateKey && storeLocation == StoreLocation.CurrentUser && storeName == StoreName.My && this.m_monoPrivateKeyStore.TryAdd(certificate.Thumbprint, certificate))
                {
                    var path = Path.ChangeExtension(Path.Combine(this.m_monoPrivateKeyStoreLocation, certificate.Thumbprint), "pfx");
                    using (var fs = File.Create(path))
                    {
                        var buffer = certificate.Export(X509ContentType.Pfx, this.ComputePass(path)); // TODO: Add password to configuration
                                                                                                        // TODO: Ensure the certificate is exportable
                        fs.Write(buffer, 0, buffer.Length);
                    }
                    audit?.WithOutcome(Model.Audit.OutcomeIndicator.Success);
                    return true;
                }
                else if (!certificate.HasPrivateKey)
                {
                    using (var store = new X509Store(storeName, storeLocation))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Add(certificate);
                        store.Close();
                    }
                    audit?.WithOutcome(Model.Audit.OutcomeIndicator.Success);
                    return true;
                }
                else
                {
                    throw new PlatformNotSupportedException($"Installing {certificate.Subject} to {storeName} with a private key is not supported on this platform");
                }
            }
            catch
            {
                audit?.WithOutcome(Model.Audit.OutcomeIndicator.SeriousFail);
                throw;
            }
            finally
            {
                audit?.Send();
            }
        }

        /// <inheritdoc/>
        public bool TryUninstallCertificate(X509Certificate2 certificate, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser)
        {

            var audit = this.AuditCertificateRemoval(certificate);
            try
            {
                // Does the certificate being installed have a private key?
                if (certificate.HasPrivateKey && 
                    storeName == StoreName.My &&
                    storeLocation == StoreLocation.CurrentUser && 
                    this.m_monoPrivateKeyStore.TryRemove(certificate.Thumbprint, out var oldCert))
                {
                    File.Delete(Path.ChangeExtension(Path.Combine(this.m_monoPrivateKeyStoreLocation, certificate.Thumbprint), "pfx"));
                }
                else
                {
                    using (var store = new X509Store(storeName, storeLocation))
                    {
                        store.Open(OpenFlags.ReadWrite);
                        store.Remove(certificate);
                        store.Close();
                    }
                }
                audit?.WithOutcome(Model.Audit.OutcomeIndicator.Success);
                return true;
            }
            catch
            {
                audit?.WithOutcome(Model.Audit.OutcomeIndicator.MinorFail);
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
            => this.m_auditService?.Audit()
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
            => this.m_auditService?.Audit()
                .WithTimestamp()
                .WithEventType(EventTypeCodes.SecurityAlert)
                .WithEventIdentifier(Model.Audit.EventIdentifierType.SecurityAlert)
                .WithAction(Model.Audit.ActionType.Delete)
                .WithLocalDestination()
                .WithPrincipal()
                .WithSystemObjects(Model.Audit.AuditableObjectRole.SecurityResource, Model.Audit.AuditableObjectLifecycle.PermanentErasure, certificate);

        /// <inheritdoc/>
        public IEnumerable<X509Certificate2> FindAllCertificates(X509FindType findType, object findValue, StoreName storeName = StoreName.My, StoreLocation storeLocation = StoreLocation.CurrentUser, bool validOnly = true)
        {
            // First return the internal certificate which match if they are applicable
            if(storeName == StoreName.My && storeLocation == StoreLocation.CurrentUser)
            {
                foreach(var itm in this.m_monoPrivateKeyStore)
                {
                    if(validOnly && !new X509Chain().Build(itm.Value))
                    {
                        continue; 
                    }

                    switch(findType)
                    {
                        case X509FindType.FindByThumbprint:
                            if (itm.Value.Thumbprint.Equals(findValue))
                            {
                                yield return itm.Value;
                            }
                            break;
                        case X509FindType.FindBySubjectDistinguishedName:
                            if(itm.Value.Subject.Equals(findValue.ToString(), StringComparison.OrdinalIgnoreCase))
                            {
                                yield return itm.Value;
                            }
                            break;
                        case X509FindType.FindBySubjectName:
                            if(itm.Value.Subject.ToLowerInvariant().Contains(findValue.ToString().ToLowerInvariant()))
                            {
                                yield return itm.Value;
                            }
                            break;
                    }
                }
            }

            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                foreach(var cert in store.Certificates.Find(findType, findValue, validOnly))
                {
                    yield return cert;
                }
            }
        }
    }
}
