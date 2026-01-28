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
using System;
using System.Security.Principal;

namespace SanteDB.Core.Services
{
    /// <summary>
    /// A resource locking service
    /// </summary>
    [System.ComponentModel.Description("Resource Checkout/Locking Provider")]
    public interface IResourceCheckoutService : IServiceImplementation
    {
        /// <summary>
        /// Try to get a lock on the resource for editing
        /// </summary>
        /// <param name="key">The object key to lock</param>
        bool Checkout<T>(Guid key);

        /// <summary>
        /// Release the lock on the specified key
        /// </summary>
        bool Checkin<T>(Guid key);

        /// <summary>
        /// Attempts to perform a soft checkout - this is equivalent to attempting to take a lock 
        /// but not actually taking it
        /// </summary>
        /// <typeparam name="T">The type of object to check</typeparam>
        /// <param name="key">The key of the object referenced by <typeparamref name="T"/></param>
        /// <param name="currentOwner">The current owner of the lock if one exists</param>
        /// <returns>True if the object is checked out</returns>
        bool IsCheckedout<T>(Guid key, out IIdentity currentOwner);
    }
}