using SanteDB.Core.Jobs;
using SanteDB.Core.Notifications;
using SanteDB.Core.Protocol;
using SanteDB.Core.Security;
using SanteDB.Core.Services.Impl.Repository;
using SanteDB.Server.Core.Services.Impl;
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
            typeof(LocalRepositoryFactory)
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
