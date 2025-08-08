/*
 * Copyright (C) 2021 - 2025, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
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
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using SanteDB.Core.Security;
using SanteDB.Core.Security.Principal;
using SanteDB.Core.Security.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Services.Impl.Repository
{
    /// <summary>
    /// Represents a security repository service that uses the direct local services
    /// </summary>
    public class LocalSecurityRepositoryService : ISecurityRepositoryService, ILocalServiceProvider<ISecurityRepositoryService>
    {
        /// <summary>
        /// Gets the service name
        /// </summary>
        public string ServiceName => "Local Security Repository Service";

        /// <inheritdoc/>
        public ISecurityRepositoryService LocalProvider => this;

        // Localization Service
        private readonly ILocalizationService m_localizationService;
        private readonly IAdhocCacheService m_adhocCacheService;
        private readonly IPolicyEnforcementService m_pepService;

        // User repo
        private IRepositoryService<SecurityUser> m_userRepository;
        private readonly IRepositoryService<DeviceEntity> m_deviceEntityRepository;

        // App repo
        private IRepositoryService<SecurityApplication> m_applicationRepository;

        // Device repo
        private IRepositoryService<SecurityDevice> m_deviceRepository;

        // Policy repo
        private IRepositoryService<SecurityPolicy> m_policyRepository;

        // Role repository
        private IRepositoryService<SecurityRole> m_roleRepository;

        // User Entity repository
        private IRepositoryService<UserEntity> m_userEntityRepository;

        // Provenance
        private IDataPersistenceService<SecurityProvenance> m_provenancePersistence;

        // IdP
        private IIdentityProviderService m_identityProviderService;

        // App IdP
        private IApplicationIdentityProviderService m_applicationIdentityProvider;
        private readonly IRepositoryService<ApplicationEntity> m_applicationEntityRepository;

        // Dev IdP
        private IDeviceIdentityProviderService m_deviceIdentityProvider;

        // Role provider
        private IRoleProviderService m_roleProvider;

        /// <summary>
        /// Creates a new local security repository service
        /// </summary>
        public LocalSecurityRepositoryService(
            IRepositoryService<SecurityUser> userRepository,
            IRepositoryService<SecurityApplication> applicationRepository,
            IRepositoryService<SecurityRole> roleRepository,
            IRepositoryService<SecurityDevice> deviceRepository,
            IRepositoryService<SecurityPolicy> policyRepository,
            IRepositoryService<UserEntity> userEntityRepository,
            IRepositoryService<ApplicationEntity> applicationEntityRepository,
            IRepositoryService<DeviceEntity> deviceEntityRepository,
            IDataPersistenceService<SecurityProvenance> provenanceRepository,
            IRoleProviderService roleProviderService,
            IIdentityProviderService identityProviderService,
            IApplicationIdentityProviderService applicationIdentityProvider,
            IDeviceIdentityProviderService deviceIdentityProvider,
            IPolicyEnforcementService pepService,
            ILocalizationService localizationService,
            IAdhocCacheService adhocCacheService)
        {
            this.m_pepService = pepService;
            this.m_userRepository = userRepository;
            this.m_applicationIdentityProvider = applicationIdentityProvider;
            this.m_applicationEntityRepository = applicationEntityRepository;
            this.m_deviceEntityRepository = deviceEntityRepository;
            this.m_applicationRepository = applicationRepository;
            this.m_identityProviderService = identityProviderService;
            this.m_provenancePersistence = provenanceRepository;
            this.m_deviceIdentityProvider = deviceIdentityProvider;
            this.m_deviceRepository = deviceRepository;
            this.m_policyRepository = policyRepository;
            this.m_roleRepository = roleRepository;
            this.m_userEntityRepository = userEntityRepository;
            this.m_roleProvider = roleProviderService;
            this.m_localizationService = localizationService;
            this.m_adhocCacheService = adhocCacheService;
        }

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="password">The new password of the user.</param>
        /// <returns>Returns the updated user.</returns>
        public SecurityUser ChangePassword(Guid userId, string password)
        {
            var securityUser = this.m_userRepository?.Get(userId);
            if (securityUser == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.server.core.securityUser"));
            }
            this.m_identityProviderService.ChangePassword(securityUser.UserName, password, AuthenticationContext.Current.Principal);
            return securityUser;
        }

        /// <summary>
        /// Change password
        /// </summary>
        public void ChangePassword(string userName, string password)
        {
            this.m_identityProviderService.ChangePassword(userName, password, AuthenticationContext.Current.Principal);
        }


        /// <summary>
        /// Get the policy information in the model format
        /// </summary>
        public SecurityPolicy GetPolicy(string policyOid)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);
            return this.m_policyRepository.Find(o => o.Oid == policyOid).SingleOrDefault();
        }

        /// <summary>
        /// Get the security provenance
        /// </summary>
        public SecurityProvenance GetProvenance(Guid provenanceId)
        {
            return this.m_provenancePersistence.Get(provenanceId, null, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get the specified role
        /// </summary>
        public SecurityRole GetRole(string roleName)
        {
            return this.m_roleRepository?.Find(o => o.Name == roleName).SingleOrDefault();
        }

        /// <summary>
        /// Gets a specific user.
        /// </summary>
        /// <param name="userName">The id of the user to retrieve.</param>
        /// <returns>Returns the user.</returns>
        public SecurityUser GetUser(String userName)
        {
            // As the identity service may be LDAP, best to call it to get an identity name
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);
            var cacheKey = $"sec.ulook.{userName}";
            if (this.m_adhocCacheService.TryGet<Guid>(cacheKey, out var userKey))
            {
                return this.m_userRepository.Get(userKey); // Can use cache
            }
            else
            {
                var retVal = this.m_userRepository.Find(u => u.UserName == userName).FirstOrDefault();
                this.m_adhocCacheService.Add(cacheKey, retVal.Key);
                return retVal;
            }
        }

        /// <summary>
        /// Get the specified user based on identity
        /// </summary>
        public SecurityUser GetUser(IIdentity identity)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);
            return this.GetUser(identity.Name);
        }

        /// <summary>
        /// Get user entity from identity
        /// </summary>
        public UserEntity GetUserEntity(IIdentity identity)
        {
            var cacheKey = $"sec.ulook.ue.{identity.Name}";
            if (this.m_adhocCacheService.TryGet<Guid>(cacheKey, out var uuid))
            {
                return this.m_userEntityRepository.Get(uuid);
            }
            else
            {
                var retVal = this.m_userEntityRepository?.Find(o => o.SecurityUser.UserName == identity.Name).FirstOrDefault();
                this.m_adhocCacheService.Add(cacheKey, retVal.Key);
                return retVal;
            }
        }

        /// <summary>
        /// Locks a specific user.
        /// </summary>
        /// <param name="userId">The id of the user to lock.</param>
        public void LockUser(Guid userId)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AlterIdentity);

            var securityUser = this.m_userRepository.Get(userId);
            if (securityUser == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_identityProviderService.SetLockout(securityUser.UserName, true, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Unlocks a specific user.
        /// </summary>
        /// <param name="userId">The id of the user to be unlocked.</param>
        public void UnlockUser(Guid userId)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AlterIdentity);

            var securityUser = this.m_userRepository?.Get(userId);
            if (securityUser == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_identityProviderService.SetLockout(securityUser.UserName, false, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get the specified provider entity
        /// </summary>
        public Provider GetProviderEntity(IIdentity identity)
        {
            return ApplicationServiceContext.Current.GetService<IRepositoryService<Provider>>()
                .Find(o => o.Relationships.Where(r => r.RelationshipType.Mnemonic == "AssignedEntity").Any(r => (r.SourceEntity as UserEntity).SecurityUser.UserName == identity.Name)).FirstOrDefault();
        }

        /// <summary>
        /// Set the user's roles to only those in the roles array
        /// </summary>
        public void SetUserRoles(SecurityUser user, string[] roles)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.AlterRoles);

            if (!user.Key.HasValue)
            {
                user = this.GetUser(user.UserName);
            }

            this.m_roleProvider.RemoveUsersFromRoles(new String[] { user.UserName }, this.m_roleProvider.GetAllRoles().Where(o => !roles.Contains(o)).ToArray(), AuthenticationContext.Current.Principal);
            this.m_roleProvider.AddUsersToRoles(new string[] { user.UserName }, roles, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Lock a device
        /// </summary>
        public void LockDevice(Guid key)
        {

            this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateDevice);

            var securityDevice = this.m_deviceRepository?.Get(key);
            if (securityDevice == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_deviceIdentityProvider.SetLockout(securityDevice.Name, true, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Locks the specified application
        /// </summary>
        public void LockApplication(Guid key)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateApplication);
            var securityApplication = this.m_applicationRepository?.Get(key);
            if (securityApplication == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_applicationIdentityProvider.SetLockout(securityApplication.Name, true, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Unlocks the specified device
        /// </summary>
        public void UnlockDevice(Guid key)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateDevice);

            var securityDevice = this.m_deviceRepository?.Get(key);
            if (securityDevice == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_deviceIdentityProvider.SetLockout(securityDevice.Name, false, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Unlock the specified application
        /// </summary>
        public void UnlockApplication(Guid key)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.CreateApplication);
            var securityApplication = this.m_applicationRepository?.Get(key);
            if (securityApplication == null)
            {
                throw new KeyNotFoundException(this.m_localizationService.GetString("error.type.KeyNotFoundException.userMessage"));
            }
            this.m_applicationIdentityProvider.SetLockout(securityApplication.Name, false, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Find provenance
        /// </summary>
        public IQueryResultSet<SecurityProvenance> FindProvenance(Expression<Func<SecurityProvenance, bool>> query)
        {
            return this.m_provenancePersistence.Query(query, AuthenticationContext.Current.Principal);
        }

        /// <summary>
        /// Get the security entity from the specified principal
        /// </summary>
        /// <param name="principal">The principal to be fetched</param>
        public SecurityEntity GetSecurityEntity(IPrincipal principal)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);
            switch (principal.Identity)// Device credential
            {
                case IDeviceIdentity deviceIdentity:
                    return this.GetDevice(deviceIdentity);
                case IApplicationIdentity applicationIdentity:
                    return this.GetApplication(applicationIdentity);
                default:
                    return this.GetUser(principal.Identity);
            }
        }

        /// <inheritdoc/>
        public Entity GetCdrEntity(IPrincipal principal)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);
            switch (principal.Identity)// Device credential
            {
                case IDeviceIdentity deviceIdentity:
                    return this.m_deviceEntityRepository.Find(o => o.SecurityDevice.Name.ToLowerInvariant() == principal.Identity.Name.ToLowerInvariant()).FirstOrDefault();
                case IApplicationIdentity applicationIdentity:
                    return this.m_applicationEntityRepository.Find(o => o.SecurityApplication.Name.ToLowerInvariant() == principal.Identity.Name.ToLowerInvariant()).FirstOrDefault();
                default:
                    return (Entity)this.GetProviderEntity(principal.Identity) ?? this.GetUserEntity(principal.Identity);
            }

        }

        /// <summary>
        /// Get device from name
        /// </summary>
        public SecurityDevice GetDevice(string deviceName)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);

            if (String.IsNullOrEmpty(deviceName))
            {
                throw new ArgumentNullException(this.m_localizationService.GetString("error.type.ArgumentNullException.param", new
                {
                    param = nameof(deviceName)
                }));
            }
            return this.m_deviceRepository.Find(o => o.Name == deviceName).FirstOrDefault();
        }

        /// <summary>
        /// Get application from name
        /// </summary>
        public SecurityApplication GetApplication(string applicationName)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);

            if (String.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(this.m_localizationService.GetString("error.type.ArgumentNullException.param", new
                {
                    param = nameof(applicationName)
                }));
            }

            return this.m_applicationRepository.Find(o => o.Name == applicationName).FirstOrDefault();
        }

        /// <summary>
        /// Get device
        /// </summary>
        public SecurityDevice GetDevice(IIdentity identity)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);

            if (identity == null)
            {
                throw new ArgumentNullException(this.m_localizationService.GetString("error.type.ArgumentNullException.param", new
                {
                    param = nameof(identity)
                }));
            }

            return this.GetDevice(identity.Name);
        }

        /// <summary>
        /// Get application
        /// </summary>
        public SecurityApplication GetApplication(IIdentity identity)
        {
            this.m_pepService.Demand(PermissionPolicyIdentifiers.ReadMetadata);

            if (identity == null)
            {
                throw new ArgumentNullException(this.m_localizationService.GetString("error.type.ArgumentNullException.param", new
                {
                    param = nameof(identity)
                }));
            }

            return this.GetApplication(identity.Name);
        }

        /// <summary>
        /// Get the security identifier for the provided <paramref name="identity"/>
        /// </summary>
        /// <param name="identity">The identity for which the security identifier should be retrieved</param>
        /// <returns>The SID of <paramref name="identity"/></returns>
        public Guid GetSid(IIdentity identity)
        {
            switch (identity)
            {
                case IDeviceIdentity did:
                    return this.m_deviceIdentityProvider.GetSid(identity.Name);
                case IApplicationIdentity aid:
                    return this.m_applicationIdentityProvider.GetSid(identity.Name);
                default:
                    return this.m_identityProviderService.GetSid(identity.Name);
            }
        }

        /// <inheritdoc/>
        public string ResolveName(Guid sid) =>
            this.m_userRepository.Get(sid)?.UserName ??
            this.m_applicationRepository.Get(sid)?.Name ??
            this.m_deviceRepository.Get(sid)?.Name;
    }
}