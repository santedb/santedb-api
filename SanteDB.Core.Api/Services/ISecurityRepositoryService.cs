﻿/*
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2021-2-9
 */
using SanteDB.Core.Model.Entities;
using SanteDB.Core.Model.Query;
using SanteDB.Core.Model.Roles;
using SanteDB.Core.Model.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// Security repository service is responsible for the maintenance of security entities
    /// </summary>
    public interface ISecurityRepositoryService : IServiceImplementation
    {
        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="userId">The id of the user.</param>
        /// <param name="password">The new password of the user.</param>
        /// <returns>Returns the updated user.</returns>
        SecurityUser ChangePassword(Guid userId, String password);

        /// <summary>
        /// Gets the specified provider entity from the specified identity
        /// </summary>
        /// <param name="identity">The identity to resolve to a provider</param>
        /// <returns>The provider entity if the user has one</returns>
        Provider GetProviderEntity(IIdentity identity);

        /// <summary>
        /// Creates a user with a specified password.
        /// </summary>
        /// <param name="userInfo">The security user.</param>
        /// <param name="password">The password.</param>
        /// <returns>Returns the newly created user.</returns>
        SecurityUser CreateUser(SecurityUser userInfo, String password);

        /// <summary>
        /// Get a user by user name
        /// </summary>
        SecurityUser GetUser(String userName);

        /// <summary>
        /// Get the specified security policy by OID
        /// </summary>
        SecurityPolicy GetPolicy(String policyOid);

        /// <summary>
        /// Gets a specific role.
        /// </summary>
        /// <param name="roleName">The id of the role to retrieve.</param>
        /// <returns>Returns the role.</returns>
        SecurityRole GetRole(String roleName);

        /// <summary>
        /// Locks a device principal
        /// </summary>
        void LockDevice(Guid key);

        /// <summary>
        /// Locks an application
        /// </summary>
        void LockApplication(Guid key);

        /// <summary>
        /// Removes a lock from a device
        /// </summary>
        void UnlockDevice(Guid key);

        /// <summary>
        /// Removes a lock from an application
        /// </summary>
        void UnlockApplication(Guid key);

        /// <summary>
        /// Gets the specified security user based on the principal
        /// </summary>
        SecurityUser GetUser(IIdentity identity);

        /// <summary>
        /// Get the user entity
        /// </summary>
        UserEntity GetUserEntity(IIdentity identity);

        /// <summary>
        /// Locks a specific user.
        /// </summary>
        /// <param name="userId">The id of the user to lock.</param>
        void LockUser(Guid userId);

        /// <summary>
        /// Unlocks a specific user.
        /// </summary>
        /// <param name="userId">The id of the user to be unlocked.</param>
        void UnlockUser(Guid userId);

        /// <summary>
        /// Get the provenance object
        /// </summary>
        SecurityProvenance GetProvenance(Guid provenanceId);

        /// <summary>
        /// Find provenance objects matching the specified object
        /// </summary>
        IEnumerable<SecurityProvenance> FindProvenance(Expression<Func<SecurityProvenance, bool>> query, int offset, int? count, out int totalResults, Guid queryId, params ModelSort<SecurityProvenance>[] orderBy);
    }
}