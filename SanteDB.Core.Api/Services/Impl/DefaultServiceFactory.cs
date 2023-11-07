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
using SanteDB.Core.Data.Import;
using SanteDB.Core.Data.Initialization;
using SanteDB.Core.Http;
using SanteDB.Core.Jobs;
using SanteDB.Core.Notifications.Email;
using SanteDB.Core.Notifications.Templating;
using SanteDB.Core.Cdss;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Audit;
using SanteDB.Core.Security.Services;
using SanteDB.Core.Security.Tfa;
using SanteDB.Core.Services.Impl.Repository;
using System;
using System.Linq;

#pragma warning disable CS0612
namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a service factory that constructs the "default" services if no other services are registered
    /// </summary>
    public class DefaultServiceFactory : IServiceFactory
    {
        private readonly Type[] m_defaultServices = new Type[]
        {
            typeof(FileSystemDataStreamManager),
            typeof(DefaultForeignDataImporter),
            typeof(DefaultPolicyEnforcementService),
            typeof(LocalTagPersistenceService),
            typeof(LocalProtocolRepositoryService),
            typeof(DefaultJobManagerService),
            typeof(XmlFileJobScheduleManager),
            typeof(XmlFileJobStateManager),
            typeof(DefaultThreadPoolService),
            typeof(SimplePatchService),
            typeof(DefaultOperatingSystemInfoService),
            typeof(DataInitializationService),
            typeof(RestClientFactory),
            typeof(LocalMailMessageService),
            typeof(SimpleDecisionSupportService),
            typeof(AesSymmetricCrypographicProvider),
            typeof(FrameworkMailService),
            typeof(DefaultDataSigningService),
            typeof(DefaultPolicyDecisionService),
            typeof(DefaultPolicyEnforcementService),
            typeof(SHA256PasswordHashingService),
            typeof(Rfc4226TfaCodeProvider),
            typeof(CachedResourceCheckoutService),
            typeof(SimpleNotificationTemplateFiller),
            typeof(JwsResourcePointerService),
            typeof(FileSystemDispatcherQueueService),
            typeof(LocalRepositoryFactory),
            typeof(RegexPasswordValidator),
            typeof(DefaultOperatingSystemInfoService),
            typeof(DefaultNetworkInformationService),
            typeof(SimpleSessionTokenEncodingService),
            typeof(SimpleSessionTokenResolver),
            typeof(DefaultAuditService),
            typeof(CachedResourceCheckoutService),
            typeof(DefaultPlatformSecurityProvider)
        };
        private readonly IServiceManager m_serviceManager;

        /// <summary>
        /// DI constructor
        /// </summary>
        public DefaultServiceFactory(IServiceManager serviceManager)
        {
            this.m_serviceManager = serviceManager;
        }

        /// <summary>
        /// Try to create a service form the d
        /// </summary>
        public bool TryCreateService<TService>(out TService serviceInstance)
        {
            var retVal = this.TryCreateService(typeof(TService), out var si);
            serviceInstance = (TService)si;
            return retVal;
        }

        /// <summary>
        /// Try to create the specified service type
        /// </summary>
        public bool TryCreateService(Type serviceType, out object serviceInstance)
        {
            // HACK: If running on Mono certificates with private keys need to be stored on the file
            if (serviceType == typeof(IPlatformSecurityProvider) && Type.GetType("Mono.Runtime") != null)
            {
                serviceInstance = this.m_serviceManager.CreateInjected<MonoPlatformSecurityProvider>();
                return true;
            }

            var si = this.m_defaultServices.FirstOrDefault(o => serviceType.IsAssignableFrom(o));
            if (si == null)
            {
                serviceInstance = null;
                return false;
            }
            else
            {
                serviceInstance = this.m_serviceManager.CreateInjected(si);
                return true;
            }
        }
    }
}
#pragma warning restore