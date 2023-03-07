using SanteDB.Core.Diagnostics;
using SanteDB.Core.i18n;
using SanteDB.Core.Security.Certs;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Configures SanteDB with a series of default keys for signing
    /// </summary>
    public class RsaKeyInitializationService : IServiceImplementation
    {
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(RsaKeyInitializationService));

        /// <inheritdoc/>
        public string ServiceName => "Auto Convert HS256 -> RS256 Configuration";

        /// <summary>
        /// DI constructor
        /// </summary>
        public RsaKeyInitializationService(
            IConfigurationManager configurationManager,
            IServiceManager serviceManager,
            ICertificateGeneratorService certificateGeneratorService = null,
            ICertificateAuthorityService certificateAuthorityService = null)
        {
            var securityConfiguration = configurationManager.GetSection<SecurityConfigurationSection>();

            if (securityConfiguration.Signatures.Any(o => o.Algorithm == SignatureAlgorithm.HS256))
            {
                if (certificateGeneratorService != null)
                {
                    this.m_tracer.TraceWarning("--- HMAC256 KEYS FOUND IN YOUR CONFIGURATION - FINDING OR GENERATING RSA KEYS ---");
                    foreach (var k in securityConfiguration.Signatures.Where(o => o.Algorithm == SignatureAlgorithm.HS256).ToArray())
                    {
                        var keySubject = $"CN=SanteDB {k.KeyName}, OID.2.5.6.11={ApplicationServiceContext.Current.ApplicationName}, DC={k.KeyName}";
                        X509Certificate2 certificate = X509CertificateUtils.FindCertificate(X509FindType.FindBySubjectDistinguishedName, StoreLocation.CurrentUser, StoreName.My, keySubject);

                        if (certificate != null)
                        {
                        }
                        else if (certificateAuthorityService != null) // generate and sign
                        {
                            using (AuthenticationContext.EnterSystemContext())
                            {
                                // Doesn't exist so generate one
                                this.m_tracer.TraceInfo("Will generate and sign {0}", keySubject);
                                var privateKey = certificateGeneratorService.CreateKeyPair(2048);
                                var csr = certificateGeneratorService.CreateSigningRequest(privateKey, new X500DistinguishedName(keySubject), X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyAgreement);
                                var certRequest = certificateAuthorityService.SubmitSigningRequest(csr);
                                X509Certificate2 signedCertificate = null;
                                if (certRequest.Status != CertificateSigningRequestStatus.Approved)
                                {
                                    signedCertificate = certificateAuthorityService.Approve(certRequest);
                                }
                                else
                                {
                                    signedCertificate = certificateAuthorityService.GetCertificate(certRequest);
                                }
                                certificate = certificateGeneratorService.Combine(signedCertificate, privateKey, friendlyName: $"SanteDB Signing Key {k.KeyName}");

                                X509CertificateUtils.InstallCertificate(StoreName.My, certificate);
                            }
                        }
                        else // self-signed
                        {
                            this.m_tracer.TraceInfo("Will generate and sign {0}", keySubject);
                            var privateKey = certificateGeneratorService.CreateKeyPair(2048);
                            certificate = certificateGeneratorService.CreateSelfSignedCertificate(privateKey, new X500DistinguishedName(keySubject), new TimeSpan(365, 0, 0, 0), X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyAgreement);
                            X509CertificateUtils.InstallCertificate(StoreName.My, certificate);
                        }

                        this.m_tracer.TraceWarning("Replace key {0} with FindByThumbprint={1}", k.KeyName, certificate.Thumbprint);

                        k.Algorithm = SignatureAlgorithm.RS256;
                        k.Certificate = certificate;
                        k.FindTypeSpecified = k.StoreNameSpecified = k.StoreLocationSpecified = true;
                        k.StoreName = StoreName.My;
                        k.StoreLocation = StoreLocation.CurrentUser;
                        k.FindType = X509FindType.FindByThumbprint;
                        k.FindValue = certificate.Thumbprint;
                    }
                }
            }
        }
    }
}
