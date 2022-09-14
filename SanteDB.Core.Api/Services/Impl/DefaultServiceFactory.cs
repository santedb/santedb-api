/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
 * Date: 2022-5-30
 */
using SanteDB.Core.Data.Initialization;
using SanteDB.Core.Jobs;
using SanteDB.Core.Notifications;
using SanteDB.Core.Protocol;
using SanteDB.Core.Security;
using SanteDB.Core.Services.Impl.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SanteDB.Core.Services.Impl
{
    /// <summary>
    /// Represents a service factory that constructs the "default" services if no other services are registered
    /// </summary>
    public class DefaultServiceFactory : IServiceFactory
    {
        private readonly Type[] m_defaultServices = new Type[]
        {
            typeof(DefaultPolicyEnforcementService),
            typeof(LocalTagPersistenceService),
            typeof(LocalProtocolRepositoryService),
            typeof(DefaultJobManagerService),
            typeof(XmlFileJobScheduleManager),
            typeof(XmlFileJobStateManager),
            typeof(DefaultNotificationService),
            typeof(SimplePatchService),
            typeof(DataInitializationService),
            typeof(LocalMailMessageService),
            typeof(SimpleCarePlanService),
            typeof(AesSymmetricCrypographicProvider),
            typeof(DefaultDataSigningService),
            typeof(DefaultPolicyDecisionService),
            typeof(DefaultPolicyEnforcementService),
            typeof(DefaultTfaRelayService),
            typeof(SHA256PasswordHashingService),
            typeof(SimpleTfaSecretGenerator),
            typeof(CachedResourceCheckoutService),
            typeof(JwsResourcePointerService),
            typeof(FileSystemDispatcherQueueService),
            typeof(LocalRepositoryFactory),
            typeof(RegexPasswordValidator),
            typeof(DefaultOperatingSystemInfoService),
            typeof(DefaultNetworkInformationService)
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
            var si = this.m_defaultServices.FirstOrDefault(o => serviceType.IsAssignableFrom(o));
            if(si == null)
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
