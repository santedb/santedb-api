﻿/*
 * Based on OpenIZ, Copyright (C) 2015 - 2019 Mohawk College of Applied Arts and Technology
 * Copyright (C) 2019 - 2020, Fyfe Software Inc. and the SanteSuite Contributors (See NOTICE.md)
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
 * Date: 2019-11-27
 */
using SanteDB.Core.Services;
using System;
using System.Security.Principal;

namespace SanteDB.Core.Security.Services
{
    /// <summary>
    /// Represents a service which is capableof retrieving roles
    /// </summary>
    public interface IRoleProviderService : IServiceImplementation
    {
        /// <summary>
        /// Creates a role
        /// </summary>
        void CreateRole(String roleName, IPrincipal principal);

        /// <summary>
        /// Add users to roles
        /// </summary>
        void AddUsersToRoles(String[] users, String[] roles,  IPrincipal principal);

        /// <summary>
        /// Remove users from specified roles
        /// </summary>
        void RemoveUsersFromRoles(String[] users, String[] roles, IPrincipal principal);

        /// <summary>
        /// Find all users in a specified role
        /// </summary>
        String[] FindUsersInRole(String role);

        /// <summary>
        /// Get all roles
        /// </summary>
        String[] GetAllRoles();

        /// <summary>
        /// Get all roles
        /// </summary>
        String[] GetAllRoles(string userName);

        /// <summary>
        /// User user in the specified role
        /// </summary>
        bool IsUserInRole(String userName, String roleName);

        /// <summary>
        /// Is user in the specified role
        /// </summary>
        bool IsUserInRole(IPrincipal principal, String roleName);
    }

}

