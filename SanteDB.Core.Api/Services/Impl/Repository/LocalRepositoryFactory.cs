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
using SanteDB.Core.Diagnostics;
using SanteDB.Core.Model.Acts;
using SanteDB.Core.Model.DataTypes;
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Services;
using System;
using System.Linq;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Registers the <see cref="IRepositoryService"/> instances with the core application context and provides a 
    /// <see cref="IServiceFactory"/> implementation to construct repository services.
    /// </summary>
    /// <remarks>
    /// <para>The instances of <see cref="IRepositoryService"/> which this service constructs contact directly with the 
    /// equivalent <see cref="IDataPersistenceService"/> for each object. The repository layers add business process
    /// logic for calling <see cref="IBusinessRulesService"/>, <see cref="IPrivacyEnforcementService"/>, and others as 
    /// necessary to ensure secure and safe access to the underlying data repositories. All requests to any <see cref="IRepositoryService"/>
    /// constructed by this service use the <see cref="AuthenticationContext"/> to establish "who" is performing the action.</para>
    /// </remarks>
    [ServiceProvider("Local (database) repository service", Dependencies = new Type[] { typeof(IDataPersistenceService) })]
    public class LocalRepositoryFactory : IServiceFactory
    {
        private readonly Type[] m_excludeServiceTypes = new Type[]
        {
            typeof(ISubscriptionRepository)
        };

        // Repository services
        private readonly Type[] r_repositoryServices = new Type[] {
                typeof(LocalConceptRepository),
                typeof(GenericLocalConceptRepository<ReferenceTerm>),
                typeof(GenericLocalConceptRepository<CodeSystem>),
                typeof(GenericLocalConceptRepository<ConceptSet>),
                typeof(GenericLocalMetadataRepository<IdentityDomain>),
                typeof(GenericLocalMetadataRepository<ExtensionType>),
                typeof(GenericLocalMetadataRepository<TemplateDefinition>),
                typeof(LocalBatchRepository),
                typeof(LocalMaterialRepository),
                typeof(LocalManufacturedMaterialRepository),
                typeof(LocalOrganizationRepository),
                typeof(LocalPlaceRepository),
                typeof(LocalEntityRelationshipRepository),
                typeof(LocalPatientRepository),
                typeof(LocalExtensionTypeRepository),
                typeof(LocalSecurityApplicationRepository),
                typeof(LocalSecurityDeviceRepository),
                typeof(LocalSecurityPolicyRepository),
                typeof(LocalSecurityRoleRepositoryService),
                typeof(LocalSecurityUserRepositoryService),
                typeof(LocalEntityRepository),
                typeof(LocalUserEntityRepository),
                typeof(LocalIdentityDomainRepository),
                typeof(GenericLocalMetadataRepository<DeviceEntity>),
                typeof(GenericLocalMetadataRepository<ApplicationEntity>),
                typeof(LocalSecurityRepositoryService),
                typeof(LocalAuditRepository),
                typeof(LocalProviderRepository),
                typeof(LocalConceptRepository),
                typeof(LocalTemplateDefinitionRepositoryService),
                typeof(LocalTemplateDefinitionRepositoryService),
            };

        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local (database) repository service";

        // Trace source
        private readonly Tracer m_tracer = Tracer.GetTracer(typeof(LocalRepositoryFactory));

        private IServiceManager m_serviceManager;

        /// <summary>
        /// Create new local repository service
        /// </summary>
        public LocalRepositoryFactory(IServiceManager serviceManager)
        {
            this.m_serviceManager = serviceManager;
        }

        /// <summary>
        /// Try to create <typeparamref name="TService"/>
        /// </summary>
        public bool TryCreateService<TService>(out TService serviceInstance)
        {
            if (this.TryCreateService(typeof(TService), out object service))
            {
                serviceInstance = (TService)service;
                return true;
            }
            serviceInstance = default(TService);
            return false;
        }

        /// <summary>
        /// Attempt to create the specified service
        /// </summary>
        public bool TryCreateService(Type serviceType, out object serviceInstance)
        {
            if (serviceType == typeof(LocalRepositoryFactory))
            {
                serviceInstance = this;
                return true;
            }
            else if (this.m_excludeServiceTypes.Contains(serviceType))
            {
                serviceInstance = null;
                return false;
            }

            // Is this service type in the services?
            var st = r_repositoryServices.FirstOrDefault(s => s == serviceType || serviceType.IsAssignableFrom(s));
            if (st == null && (typeof(IRepositoryService).IsAssignableFrom(serviceType) || serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IRepositoryService<>)))
            {
                if (serviceType.IsGenericType)
                {
                    var wrappedType = serviceType.GenericTypeArguments[0];
                    if (typeof(Act).IsAssignableFrom(wrappedType))
                    {
                        this.m_tracer.TraceVerbose("Adding Act repository service for {0}...", wrappedType.Name);
                        st = typeof(GenericLocalActRepository<>).MakeGenericType(wrappedType);
                    }
                    else if (typeof(Entity).IsAssignableFrom(wrappedType))
                    {
                        this.m_tracer.TraceVerbose("Adding Entity repository service for {0}...", wrappedType);
                        st = typeof(GenericLocalClinicalDataRepository<>).MakeGenericType(wrappedType);
                    }
                    else
                    {
                        this.m_tracer.TraceVerbose("Adding generic repository service for {0}...", wrappedType);
                        st = typeof(GenericLocalRepository<>).MakeGenericType(wrappedType);
                    }
                }
                else
                {
                    st = serviceType;
                }
            }
            else if (st == null)
            {
                serviceInstance = null;
                return false;
            }

            serviceInstance = this.m_serviceManager.CreateInjected(st);
            return true;
        }
    }
}