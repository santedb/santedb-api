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
using NUnit.Framework;
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Diagnostics.Tracing;
using SanteDB.Core.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Security.Cryptography.X509Certificates;
using ZstdSharp;

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

        private void RemoveTestCert(X509Certificate2 cert)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadWrite);
                var certs = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                if (certs.Count == 1)
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
            Tracer.AddWriter(new ConsoleTraceWriter(System.Diagnostics.Tracing.EventLevel.LogAlways, String.Empty, new Dictionary<String, EventLevel>()), System.Diagnostics.Tracing.EventLevel.LogAlways);
            AppDomain.CurrentDomain.SetData("DataDirectory", TestContext.CurrentContext.TestDirectory);
            var provider = new MonoPlatformSecurityProvider();

            var lumon = this.GetLumonCer();
            var random = this.GetRandomPfx();
            this.RemoveTestCert(lumon);
            this.RemoveTestCert(random);
            provider.TryUninstallCertificate(random);
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
            provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out var randomTryGet);
            // TEST that random does have PK
            Assert.IsTrue(randomTryGet.HasPrivateKey, "Certificate is missing private key");
            Assert.IsTrue(this.HasCertificate(random)); // The OS store does not have the certificate
            Assert.IsTrue(provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out _));
            Assert.IsTrue(provider.TryGetCertificate(X509FindType.FindBySubjectDistinguishedName, random.Subject, out _));
            Assert.IsTrue(provider.TryUninstallCertificate(random));
            Assert.IsFalse(this.HasCertificate(random)); // The OS store does not have the certificate
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindBySubjectDistinguishedName, random.Subject, out _));
            Assert.IsFalse(provider.TryGetCertificate(X509FindType.FindByThumbprint, random.Thumbprint, out _));

        }
    }
}
