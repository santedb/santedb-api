using NUnit.Framework;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SanteDB.Core.Api.Test
{
    /// <summary>
    /// Mono platform security provider tests 
    /// </summary>
    [TestFixture]
    public class MonoPlatformSecurityProviderTest
    {

        private X509Certificate2 GetRandomPfx()
        {
            using (var s = this.GetType().Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.random.pfx"))
            {
                var buffer = new byte[s.Length];
                s.Read(buffer, 0, (int)s.Length);
                return new X509Certificate2(buffer, "Testing123", X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            }
        }

        private X509Certificate2 GetLumonCer()
        {
            using (var s = this.GetType().Assembly.GetManifestResourceStream("SanteDB.Core.Api.Test.test.lumon.com.cer"))
            {
                var buffer = new byte[s.Length];
                s.Read(buffer, 0, (int)s.Length);
                return new X509Certificate2(buffer);
            }
        }

        private void RemoveTestCert(X509Certificate2 cert) {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                if(certs.Count == 1)
                {
                    store.Remove(certs[0]);
                }
            }
        }

        private bool HasCertificate(X509Certificate2 cert)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false).Count > 0;
            }
        }


        [Test]
        public void TestDoesInstallCertificate()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", TestContext.CurrentContext.TestDirectory);
            var provider = new MonoPlatformSecurityProvider();

            var lumon = this.GetLumonCer();
            var random = this.GetRandomPfx();
            this.RemoveTestCert(lumon);
            this.RemoveTestCert(random);
            Assert.IsTrue(random.HasPrivateKey);

            // Test the installation of regular certificates with no private key
            Assert.IsFalse(this.HasCertificate(lumon));
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindByThumbprint, lumon.Thumbprint, out _));
            Assert.IsTrue(provider.TryInstallCertificate(lumon));
            Assert.IsTrue(this.HasCertificate(lumon));
            Assert.IsTrue(provider.TryGetCertificate(X509FindType.FindByThumbprint, lumon.Thumbprint, out _));
            Assert.IsTrue(provider.TryUninstallCertificate(lumon));
            Assert.IsFalse(this.HasCertificate(lumon));

            // Test the install of the private key certificates into MY which should result in a file being returned
            Assert.IsFalse(this.HasCertificate(random));
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out _));
            Assert.IsTrue(provider.TryInstallCertificate(random));
            Assert.IsFalse(this.HasCertificate(random)); // The OS store does not have the certificate
            Assert.IsTrue(provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out _));
            Assert.IsTrue(provider.TryGetCertificate(X509FindType.FindBySubjectDistinguishedName, random.Subject, out _));
            Assert.IsTrue(provider.TryUninstallCertificate(random));
            Assert.IsFalse(this.HasCertificate(random)); // The OS store does not have the certificate
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindBySubjectDistinguishedName, random.Subject, out _));
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out _));

        }
    }
}
