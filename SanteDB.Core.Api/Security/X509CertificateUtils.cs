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
 * Date: 2023-3-10
 */
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Configuration;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Services;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace SanteDB.Core.Security
{
    /// <summary>
    /// Utilities for x509 certificates
    /// </summary>
    public static class X509CertificateUtils
    {

        private static Tracer s_tracer = Tracer.GetTracer(typeof(X509CertificateUtils));

        /// <summary>
        /// Find a certiifcate from string values
        /// </summary>
        /// <returns>The certificate.</returns>
        /// <param name="findType">Find type.</param>
        /// <param name="storeLocation">Store location.</param>
        /// <param name="storeName">Store name.</param>
        /// <param name="findValue">Find value.</param>
        public static X509Certificate2 FindCertificate(
            String findType,
            String storeLocation,
            String storeName,
            String findValue)
        {
            X509FindType eFindType = X509FindType.FindByThumbprint;
            StoreLocation eStoreLocation = StoreLocation.CurrentUser;
            StoreName eStoreName = StoreName.My;

            if (!Enum.TryParse(findType, out eFindType))
            {
                s_tracer.TraceWarning("{0} not valid value for {1}, using {2} as default", findType, eFindType.GetType().Name, eFindType);
            }

            if (!Enum.TryParse(storeLocation, out eStoreLocation))
            {
                s_tracer.TraceWarning("{0} not valid value for {1}, using {2} as default", storeLocation, eStoreLocation.GetType().Name, eStoreLocation);
            }

            if (!Enum.TryParse(storeName, out eStoreName))
            {
                s_tracer.TraceWarning("{0} not valid value for {1}, using {2} as default", storeName, eStoreName.GetType().Name, eStoreName);
            }

            return FindCertificate(eFindType, eStoreLocation, eStoreName, findValue);
        }

        /// <summary>
        /// Install the <paramref name="certificate"/> to the specified <paramref name="storeName"/> in <see cref="StoreLocation.CurrentUser"/>
        /// </summary>
        /// <param name="storeName">The name of the certificate store to install the certificate into</param>
        /// <param name="certificate">The certificate to install</param>
        public static void InstallCertificate(StoreName storeName, X509Certificate2 certificate)
        {
            var secConfiguration = ApplicationServiceContext.Current?.GetService<IConfigurationManager>()?.GetSection<SecurityConfigurationSection>();
            var location = secConfiguration?.GetSecurityPolicy(Core.Configuration.SecurityPolicyIdentification.DefaultCertificateInstallLocation, StoreLocation.CurrentUser) ?? StoreLocation.CurrentUser;
            InstallCertificate(location, storeName, certificate);
        }

        /// <summary>
        /// Install a machine certificate for things like HTTP.sys hosting, or other system uses - you may not specify the location only certs go into MY
        /// </summary>
        public static void InstallMachineCertificate(X509Certificate2 certificate)
        {
            InstallCertificate(StoreLocation.LocalMachine, StoreName.My, certificate);
        }

        /// <summary>
        /// Install certificate utility
        /// </summary>
        private static void InstallCertificate(StoreLocation location, StoreName storeName, X509Certificate2 certificate) {

            // Audit that we have installed our certificate
            var audit = ApplicationServiceContext.Current?.GetService<IAuditService>()?.Audit()
                .WithTimestamp()
                .WithEventType(EventTypeCodes.SecurityAlert)
                .WithEventIdentifier(Model.Audit.EventIdentifierType.Import)
                .WithAction(Model.Audit.ActionType.Execute)
                .WithLocalDestination()
                .WithPrincipal()
                .WithSystemObjects(Model.Audit.AuditableObjectRole.SecurityResource, Model.Audit.AuditableObjectLifecycle.Import, certificate);

            try
            {
                using (var trustStore = new X509Store(storeName, location))
                {
                    trustStore.Open(OpenFlags.ReadWrite);
                    // Swap the certificate key store flags as appropriate for this location
                    var password = Guid.NewGuid().ToString();
                    var pfxData = certificate.Export(X509ContentType.Pfx, password);
                    var properCert = new X509Certificate2(pfxData, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable | (location == StoreLocation.CurrentUser ? X509KeyStorageFlags.UserKeySet : X509KeyStorageFlags.MachineKeySet));
                    trustStore.Add(properCert);
                    audit?.WithOutcome(Model.Audit.OutcomeIndicator.Success);
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

        /// <summary>
        /// Find the specified certificate
        /// </summary>
        /// <returns>The certificate.</returns>
        /// <param name="findType">Find type.</param>
        /// <param name="storeLocation">Store location.</param>
        /// <param name="storeName">Store name.</param>
        /// <param name="findValue">Find value.</param>
        public static X509Certificate2 FindCertificate(
            X509FindType findType,
            StoreLocation storeLocation,
            StoreName storeName,
            String findValue
        )
        {
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var matches = store.Certificates.Find(findType, findValue, false);
                if (matches.Count == 0)
                {
                    throw new FileNotFoundException("Certificate not found");
                }
                else if (matches.Count > 1)
                {
                    throw new InvalidOperationException("Too many matches");
                }
                else
                {
                    return matches[0];
                }
            }
            catch (Exception ex)
            {
                s_tracer.TraceError(ex.ToString());
                return null;
            }
            finally
            {
                store.Close();
            }
        }

    }
}

