/*
 * Copyright 2015-2018 Mohawk College of Applied Arts and Technology
 *
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
 * User: justin
 * Date: 2018-11-19
 */
using SanteDB.Core.Model.Security;
using System;
using System.Collections.Generic;

namespace SanteDB.Core.Security
{
    /// <summary>
	/// Represents a contract for a wrapper service to allow interoperation between the 
    /// 
	/// </summary>
	public interface ISecurityInformationService
    {
        /// Get active policies for the specified securable type
        /// </summary>
        IEnumerable<SecurityPolicyInstance> GetActivePolicies(object securable);

        /// <summary>
        /// Change password of a principal
        /// </summary>
        void ChangePassword(String userName, String password);

        /// <summary>
        /// Add users to roles
        /// </summary>
        void AddUsersToRoles(String[] users, String[] roles);

        /// <summary>
        /// Remove users from roles
        /// </summary>
        void RemoveUsersFromRoles(String[] users, String[] roles);

        /// <summary>
        /// Get all roles from database
        /// </summary>
        String[] GetAllRoles();

        /// <summary>
        /// Determine if user is in role
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        bool IsUserInRole(String user, String role);
    }

}
